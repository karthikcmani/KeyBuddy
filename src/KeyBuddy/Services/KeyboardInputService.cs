using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using KeyBuddy.Models;
using KeyBuddy.Native;

namespace KeyBuddy.Services;

public interface IKeyboardInputService
{
    void SendKeyPress(KeyModel key, ModifierState modifiers);
    void SendKeyDown(KeyModel key, ModifierState modifiers);
    void SendKeyUp(KeyModel key);
    void SendUnicodeText(string text);
    void ReleaseAllModifiers();
    void SynchronizeLockStates(ModifierState modifiers);
}

public class KeyboardInputService : IKeyboardInputService
{
    private readonly HashSet<ushort> _physicallyHeldKeys = new();

    public void SynchronizeLockStates(ModifierState modifiers)
    {
        // Low-order bit of GetKeyState indicates toggle status
        short caps = NativeMethods.GetKeyState(VirtualKeys.VK_CAPITAL);
        modifiers.CapsLock = (caps & 0x0001) != 0;

        short num = NativeMethods.GetKeyState(VirtualKeys.VK_NUMLOCK);
        modifiers.NumLock = (num & 0x0001) != 0;
    }

    public void SendKeyPress(KeyModel key, ModifierState modifiers)
    {
        if (key.VirtualKey == 0) return;

        // If it's a lock key (Caps Lock or Num Lock), press and release it to toggle
        if (key.KeyType == KeyType.Lock)
        {
            SendSingleKey(key.VirtualKey, key.IsExtended);
            SynchronizeLockStates(modifiers);
            return;
        }

        var activeModifiers = new List<ushort>();
        if (modifiers.IsShiftActive && key.KeyType != KeyType.Modifier)
        {
            activeModifiers.Add(VirtualKeys.VK_SHIFT);
        }
        if (modifiers.Ctrl && key.KeyType != KeyType.Modifier)
        {
            activeModifiers.Add(VirtualKeys.VK_CONTROL);
        }
        if (modifiers.Alt && key.KeyType != KeyType.Modifier)
        {
            activeModifiers.Add(VirtualKeys.VK_MENU);
        }
        if (modifiers.WindowsKey && key.KeyType != KeyType.Modifier)
        {
            activeModifiers.Add(VirtualKeys.VK_LWIN);
        }

        var inputs = new List<NativeMethods.INPUT>();

        // 1. Modifiers DOWN
        foreach (var mod in activeModifiers)
        {
            inputs.Add(CreateKeyInput(mod, isKeyUp: false, isExtended: mod == VirtualKeys.VK_LWIN));
        }

        // 2. Target Key DOWN
        inputs.Add(CreateKeyInput(key.VirtualKey, isKeyUp: false, isExtended: key.IsExtended));

        // 3. Target Key UP
        inputs.Add(CreateKeyInput(key.VirtualKey, isKeyUp: true, isExtended: key.IsExtended));

        // 4. Modifiers UP
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            var mod = activeModifiers[i];
            inputs.Add(CreateKeyInput(mod, isKeyUp: true, isExtended: mod == VirtualKeys.VK_LWIN));
        }

        SendInputs(inputs.ToArray());

        // Modifiers consumption
        if (key.KeyType != KeyType.Modifier)
        {
            modifiers.ConsumeOneShotShift();
            modifiers.Ctrl = false;
            modifiers.Alt = false;
            modifiers.WindowsKey = false;
        }
    }

    public void SendKeyDown(KeyModel key, ModifierState modifiers)
    {
        if (key.VirtualKey == 0) return;

        var inputs = new List<NativeMethods.INPUT>();
        if (modifiers.IsShiftActive)
        {
            inputs.Add(CreateKeyInput(VirtualKeys.VK_SHIFT, isKeyUp: false, isExtended: false));
        }
        if (modifiers.Ctrl)
        {
            inputs.Add(CreateKeyInput(VirtualKeys.VK_CONTROL, isKeyUp: false, isExtended: false));
        }
        if (modifiers.Alt)
        {
            inputs.Add(CreateKeyInput(VirtualKeys.VK_MENU, isKeyUp: false, isExtended: false));
        }
        if (modifiers.WindowsKey)
        {
            inputs.Add(CreateKeyInput(VirtualKeys.VK_LWIN, isKeyUp: false, isExtended: true));
        }

        inputs.Add(CreateKeyInput(key.VirtualKey, isKeyUp: false, isExtended: key.IsExtended));
        SendInputs(inputs.ToArray());

        lock (_physicallyHeldKeys)
        {
            _physicallyHeldKeys.Add(key.VirtualKey);
        }
    }

    public void SendKeyUp(KeyModel key)
    {
        if (key.VirtualKey == 0) return;

        var inputs = new List<NativeMethods.INPUT>
        {
            CreateKeyInput(key.VirtualKey, isKeyUp: true, isExtended: key.IsExtended)
        };
        SendInputs(inputs.ToArray());

        lock (_physicallyHeldKeys)
        {
            _physicallyHeldKeys.Remove(key.VirtualKey);
        }
    }

    public void SendUnicodeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var inputs = new List<NativeMethods.INPUT>();
        foreach (char c in text)
        {
            if (c == '\r') continue;

            // Unicode keydown
            var down = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)c,
                        dwFlags = NativeMethods.KEYEVENTF_UNICODE,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
            // Unicode keyup
            var up = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.InputUnion
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)c,
                        dwFlags = NativeMethods.KEYEVENTF_UNICODE | NativeMethods.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            inputs.Add(down);
            inputs.Add(up);
        }

        SendInputs(inputs.ToArray());
    }

    public void ReleaseAllModifiers()
    {
        var modifiersToRelease = new[]
        {
            (VirtualKeys.VK_SHIFT, false),
            (VirtualKeys.VK_CONTROL, false),
            (VirtualKeys.VK_MENU, false),
            (VirtualKeys.VK_LWIN, true),
            (VirtualKeys.VK_RWIN, true),
            (VirtualKeys.VK_LSHIFT, false),
            (VirtualKeys.VK_RSHIFT, false),
            (VirtualKeys.VK_LCONTROL, false),
            (VirtualKeys.VK_RCONTROL, true),
            (VirtualKeys.VK_LMENU, false),
            (VirtualKeys.VK_RMENU, true)
        };

        var inputs = new List<NativeMethods.INPUT>();
        foreach (var (vk, isExt) in modifiersToRelease)
        {
            inputs.Add(CreateKeyInput(vk, isKeyUp: true, isExtended: isExt));
        }

        lock (_physicallyHeldKeys)
        {
            foreach (var key in _physicallyHeldKeys)
            {
                inputs.Add(CreateKeyInput(key, isKeyUp: true, isExtended: false));
            }
            _physicallyHeldKeys.Clear();
        }

        SendInputs(inputs.ToArray());
    }

    private void SendSingleKey(ushort vk, bool isExtended)
    {
        var inputs = new[]
        {
            CreateKeyInput(vk, isKeyUp: false, isExtended: isExtended),
            CreateKeyInput(vk, isKeyUp: true, isExtended: isExtended)
        };
        SendInputs(inputs);
    }

    private static NativeMethods.INPUT CreateKeyInput(ushort vk, bool isKeyUp, bool isExtended)
    {
        uint scanCode = NativeMethods.MapVirtualKey(vk, 0);
        uint flags = 0;
        if (isKeyUp) flags |= NativeMethods.KEYEVENTF_KEYUP;
        if (isExtended) flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = vk,
                    wScan = (ushort)scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero
                }
            }
        };
    }

    private static void SendInputs(NativeMethods.INPUT[] inputs)
    {
        if (inputs.Length == 0) return;
        NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
    }
}
