using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using KeyBuddy.Models;
using KeyBuddy.Native;
using KeyBuddy.Services;

namespace KeyBuddy.Services;

public class PhysicalKeyHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private readonly IKeyboardInputService _inputService;
    private readonly ModifierState _modifiers;

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _proc;

    public int LongPressThresholdMs { get; set; } = 350;
    public bool IsEnabled { get; set; } = true;

    private class ActiveHookRule
    {
        public KeyMappingRule Rule { get; set; } = null!;
        public uint PrimaryVk { get; set; }
        public uint? SecondaryVk { get; set; } // e.g. numpad equivalent
        public System.Threading.Timer? Timer { get; set; }
        public bool IsDown { get; set; }
        public bool Triggered { get; set; }
    }

    private readonly List<ActiveHookRule> _activeRules = new();
    private readonly object _lock = new();

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    public PhysicalKeyHookService(IKeyboardInputService inputService, ModifierState modifiers)
    {
        _inputService = inputService;
        _modifiers = modifiers;
    }

    public void LoadRules(IEnumerable<KeyMappingRule> rules, int thresholdMs)
    {
        lock (_lock)
        {
            LongPressThresholdMs = thresholdMs;

            // Dispose old timers
            foreach (var r in _activeRules)
            {
                r.Timer?.Dispose();
            }
            _activeRules.Clear();

            foreach (var rule in rules)
            {
                if (!rule.IsEnabled || string.IsNullOrWhiteSpace(rule.TriggerKey) || string.IsNullOrWhiteSpace(rule.OutputKey))
                {
                    continue;
                }

                var (pVk, sVk) = ResolveVk(rule.TriggerKey);
                if (pVk == 0) continue;

                var hookRule = new ActiveHookRule
                {
                    Rule = rule,
                    PrimaryVk = pVk,
                    SecondaryVk = sVk
                };

                hookRule.Timer = new System.Threading.Timer(OnRuleTimerCallback, hookRule, Timeout.Infinite, Timeout.Infinite);
                _activeRules.Add(hookRule);
            }
        }
    }

    private static (uint primary, uint? secondary) ResolveVk(string keyName)
    {
        string norm = keyName.Trim().ToUpperInvariant();
        if (norm.Length == 1)
        {
            char c = norm[0];
            if (c >= 'A' && c <= 'Z')
            {
                return ((uint)c, null);
            }
            if (c >= '0' && c <= '9')
            {
                uint baseVk = (uint)(0x30 + (c - '0'));
                uint numpadVk = (uint)(0x60 + (c - '0'));
                return (baseVk, numpadVk);
            }
        }

        return norm switch
        {
            "SPACE" => (0x20, null),
            "TAB" => (0x09, null),
            "ENTER" or "RETURN" => (0x0D, null),
            "ESC" or "ESCAPE" => (0x1B, null),
            "BACKSPACE" => (0x08, null),
            _ => (0, null)
        };
    }

    public void Start()
    {
        if (_hookId != IntPtr.Zero) return;

        _proc = HookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule?.ModuleName), 0);
    }

    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        lock (_lock)
        {
            foreach (var r in _activeRules)
            {
                r.Timer?.Change(Timeout.Infinite, Timeout.Infinite);
                r.IsDown = false;
                r.Triggered = false;
            }
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && IsEnabled)
        {
            var kbd = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

            // Ignore synthetic injected events from SendInput so we don't hook ourselves
            const uint LLKHF_INJECTED = 0x00000010;
            if ((kbd.flags & LLKHF_INJECTED) == 0)
            {
                int msg = wParam.ToInt32();

                lock (_lock)
                {
                    foreach (var rule in _activeRules)
                    {
                        if (kbd.vkCode == rule.PrimaryVk || (rule.SecondaryVk.HasValue && kbd.vkCode == rule.SecondaryVk.Value))
                        {
                            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                            {
                                if (!rule.IsDown)
                                {
                                    rule.IsDown = true;
                                    rule.Triggered = false;
                                    rule.Timer?.Change(LongPressThresholdMs, Timeout.Infinite);
                                }
                            }
                            else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                            {
                                rule.IsDown = false;
                                rule.Timer?.Change(Timeout.Infinite, Timeout.Infinite);

                                if (rule.Triggered)
                                {
                                    rule.Triggered = false;
                                    return (IntPtr)1; // Block keyup event
                                }
                            }
                        }
                    }
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void OnRuleTimerCallback(object? state)
    {
        if (state is not ActiveHookRule rule) return;

        lock (_lock)
        {
            if (!rule.IsDown || rule.Triggered) return;
            rule.Triggered = true;
        }

        try
        {
            // 1. Backspace the trigger character that Windows naturally typed
            var bsKey = KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK);
            _inputService.SendKeyPress(bsKey, _modifiers);

            Thread.Sleep(25);

            // 2. Type output key / text
            string output = rule.Rule.OutputKey;
            if (output.Length == 1 && char.IsLetterOrDigit(output[0]))
            {
                char outChar = output[0];
                uint outVk = 0;
                if (char.IsLetter(outChar))
                {
                    outVk = (uint)char.ToUpperInvariant(outChar);
                }
                else if (char.IsDigit(outChar))
                {
                    outVk = (uint)(0x30 + (outChar - '0'));
                }

                if (outVk != 0)
                {
                    var targetKey = KeyModel.Create(output, output, (ushort)outVk);
                    _inputService.SendKeyPress(targetKey, _modifiers);
                    return;
                }
            }

            // Fallback for special symbols (e.g. '@', '$', '#', etc.) or text
            _inputService.SendUnicodeText(output);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error executing hold-to-replace rule: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
        lock (_lock)
        {
            foreach (var r in _activeRules)
            {
                r.Timer?.Dispose();
            }
            _activeRules.Clear();
        }
    }
}
