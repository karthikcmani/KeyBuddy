using System;
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

    private const int VK_O = 0x4F;
    private const int VK_2 = 0x32;
    private const int VK_NUMPAD2 = 0x62;
    private const int VK_A = 0x41;

    private readonly IKeyboardInputService _inputService;
    private readonly ModifierState _modifiers;

    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _proc;

    // Timer and states for 'O' -> 'P'
    private readonly System.Threading.Timer _longPressOTimer;
    private bool _isODown;
    private bool _longPressOTriggered;

    // Timer and states for '2' -> '4'
    private readonly System.Threading.Timer _longPress2Timer;
    private bool _is2Down;
    private bool _longPress2Triggered;

    // Timer and states for 'A' -> '@'
    private readonly System.Threading.Timer _longPressATimer;
    private bool _isADown;
    private bool _longPressATriggered;

    // Milliseconds to consider a hold/long-press
    public int LongPressThresholdMs { get; set; } = 350;

    public bool IsEnabled { get; set; } = true;

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
        _longPressOTimer = new System.Threading.Timer(OnLongPressOCallback, null, Timeout.Infinite, Timeout.Infinite);
        _longPress2Timer = new System.Threading.Timer(OnLongPress2Callback, null, Timeout.Infinite, Timeout.Infinite);
        _longPressATimer = new System.Threading.Timer(OnLongPressACallback, null, Timeout.Infinite, Timeout.Infinite);
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
        _longPressOTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _longPress2Timer.Change(Timeout.Infinite, Timeout.Infinite);
        _longPressATimer.Change(Timeout.Infinite, Timeout.Infinite);
        _isODown = false;
        _longPressOTriggered = false;
        _is2Down = false;
        _longPress2Triggered = false;
        _isADown = false;
        _longPressATriggered = false;
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

                // 1. Long press 'O' -> 'P'
                if (kbd.vkCode == VK_O)
                {
                    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                    {
                        if (!_isODown)
                        {
                            _isODown = true;
                            _longPressOTriggered = false;
                            _longPressOTimer.Change(LongPressThresholdMs, Timeout.Infinite);
                        }
                    }
                    else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                    {
                        _isODown = false;
                        _longPressOTimer.Change(Timeout.Infinite, Timeout.Infinite);

                        if (_longPressOTriggered)
                        {
                            _longPressOTriggered = false;
                            return (IntPtr)1; // Block keyup event
                        }
                    }
                }

                // 2. Long press '2' (number row or numpad) -> '4'
                if (kbd.vkCode == VK_2 || kbd.vkCode == VK_NUMPAD2)
                {
                    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                    {
                        if (!_is2Down)
                        {
                            _is2Down = true;
                            _longPress2Triggered = false;
                            _longPress2Timer.Change(LongPressThresholdMs, Timeout.Infinite);
                        }
                    }
                    else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                    {
                        _is2Down = false;
                        _longPress2Timer.Change(Timeout.Infinite, Timeout.Infinite);

                        if (_longPress2Triggered)
                        {
                            _longPress2Triggered = false;
                            return (IntPtr)1; // Block keyup event
                        }
                    }
                }

                // 3. Long press 'A' -> '@'
                if (kbd.vkCode == VK_A)
                {
                    if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                    {
                        if (!_isADown)
                        {
                            _isADown = true;
                            _longPressATriggered = false;
                            _longPressATimer.Change(LongPressThresholdMs, Timeout.Infinite);
                        }
                    }
                    else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                    {
                        _isADown = false;
                        _longPressATimer.Change(Timeout.Infinite, Timeout.Infinite);

                        if (_longPressATriggered)
                        {
                            _longPressATriggered = false;
                            return (IntPtr)1; // Block keyup event
                        }
                    }
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void OnLongPressOCallback(object? state)
    {
        if (_isODown && !_longPressOTriggered)
        {
            _longPressOTriggered = true;

            // 1. Backspace the 'o' character that Windows already typed
            var bsKey = KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK);
            _inputService.SendKeyPress(bsKey, _modifiers);

            Thread.Sleep(20);

            // 2. Type 'P' (or 'p')
            var pKey = KeyModel.Create("P", "p", VirtualKeys.VK_P);
            _inputService.SendKeyPress(pKey, _modifiers);
        }
    }

    private void OnLongPress2Callback(object? state)
    {
        if (_is2Down && !_longPress2Triggered)
        {
            _longPress2Triggered = true;

            // 1. Backspace the '2' character that Windows already typed
            var bsKey = KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK);
            _inputService.SendKeyPress(bsKey, _modifiers);

            Thread.Sleep(20);

            // 2. Type '4' (or '$' if Shift is held)
            var fourKey = KeyModel.Create("4", "4", VirtualKeys.VK_4);
            _inputService.SendKeyPress(fourKey, _modifiers);
        }
    }

    private void OnLongPressACallback(object? state)
    {
        if (_isADown && !_longPressATriggered)
        {
            _longPressATriggered = true;

            // 1. Backspace the 'a' character that Windows already typed
            var bsKey = KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK);
            _inputService.SendKeyPress(bsKey, _modifiers);

            Thread.Sleep(20);

            // 2. Send Unicode '@' directly
            _inputService.SendUnicodeText("@");
        }
    }

    public void Dispose()
    {
        Stop();
        _longPressOTimer.Dispose();
        _longPress2Timer.Dispose();
        _longPressATimer.Dispose();
    }
}
