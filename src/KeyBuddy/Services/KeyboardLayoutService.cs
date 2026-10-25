using System;
using System.Collections.Generic;
using KeyBuddy.Models;

namespace KeyBuddy.Services;

public class KeyboardLayoutService
{
    public List<List<KeyModel>> GetFunctionRow()
    {
        return new List<List<KeyModel>>
        {
            new List<KeyModel>
            {
                KeyModel.Create("ESC", "Esc", VirtualKeys.VK_ESCAPE, 1.2, KeyType.Function),
                KeyModel.Create("F1", "F1", VirtualKeys.VK_F1, 1.0, KeyType.Function),
                KeyModel.Create("F2", "F2", VirtualKeys.VK_F2, 1.0, KeyType.Function),
                KeyModel.Create("F3", "F3", VirtualKeys.VK_F3, 1.0, KeyType.Function),
                KeyModel.Create("F4", "F4", VirtualKeys.VK_F4, 1.0, KeyType.Function),
                KeyModel.Create("F5", "F5", VirtualKeys.VK_F5, 1.0, KeyType.Function),
                KeyModel.Create("F6", "F6", VirtualKeys.VK_F6, 1.0, KeyType.Function),
                KeyModel.Create("F7", "F7", VirtualKeys.VK_F7, 1.0, KeyType.Function),
                KeyModel.Create("F8", "F8", VirtualKeys.VK_F8, 1.0, KeyType.Function),
                KeyModel.Create("F9", "F9", VirtualKeys.VK_F9, 1.0, KeyType.Function),
                KeyModel.Create("F10", "F10", VirtualKeys.VK_F10, 1.0, KeyType.Function),
                KeyModel.Create("F11", "F11", VirtualKeys.VK_F11, 1.0, KeyType.Function),
                KeyModel.Create("F12", "F12", VirtualKeys.VK_F12, 1.0, KeyType.Function),
                KeyModel.Create("PRTSC", "PrtSc", VirtualKeys.VK_SNAPSHOT, 1.0, KeyType.Action),
                KeyModel.Create("INS", "Ins", VirtualKeys.VK_INSERT, 1.0, KeyType.Action, isExtended: true),
                KeyModel.Create("DEL", "Del", VirtualKeys.VK_DELETE, 1.0, KeyType.Action, isExtended: true)
            }
        };
    }

    public List<List<KeyModel>> GetMainRows()
    {
        var rows = new List<List<KeyModel>>();

        // Row 1: Numbers
        rows.Add(new List<KeyModel>
        {
            KeyModel.Create("OEM_3", "`", VirtualKeys.VK_OEM_3, 1.0, KeyType.Standard, "~"),
            KeyModel.Create("1", "1", VirtualKeys.VK_1, 1.0, KeyType.Standard, "!"),
            KeyModel.Create("2", "2", VirtualKeys.VK_2, 1.0, KeyType.Standard, "@"),
            KeyModel.Create("3", "3", VirtualKeys.VK_3, 1.0, KeyType.Standard, "#"),
            KeyModel.Create("4", "4", VirtualKeys.VK_4, 1.0, KeyType.Standard, "$"),
            KeyModel.Create("5", "5", VirtualKeys.VK_5, 1.0, KeyType.Standard, "%"),
            KeyModel.Create("6", "6", VirtualKeys.VK_6, 1.0, KeyType.Standard, "^"),
            KeyModel.Create("7", "7", VirtualKeys.VK_7, 1.0, KeyType.Standard, "&"),
            KeyModel.Create("8", "8", VirtualKeys.VK_8, 1.0, KeyType.Standard, "*"),
            KeyModel.Create("9", "9", VirtualKeys.VK_9, 1.0, KeyType.Standard, "("),
            KeyModel.Create("0", "0", VirtualKeys.VK_0, 1.0, KeyType.Standard, ")"),
            KeyModel.Create("OEM_MINUS", "-", VirtualKeys.VK_OEM_MINUS, 1.0, KeyType.Standard, "_"),
            KeyModel.Create("OEM_PLUS", "=", VirtualKeys.VK_OEM_PLUS, 1.0, KeyType.Standard, "+"),
            KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK, 2.0, KeyType.Action)
        });

        // Row 2: QWERTY
        rows.Add(new List<KeyModel>
        {
            KeyModel.Create("TAB", "Tab", VirtualKeys.VK_TAB, 1.5, KeyType.Action),
            KeyModel.Create("Q", "q", VirtualKeys.VK_Q, 1.0, KeyType.Standard, "Q"),
            KeyModel.Create("W", "w", VirtualKeys.VK_W, 1.0, KeyType.Standard, "W"),
            KeyModel.Create("E", "e", VirtualKeys.VK_E, 1.0, KeyType.Standard, "E"),
            KeyModel.Create("R", "r", VirtualKeys.VK_R, 1.0, KeyType.Standard, "R"),
            KeyModel.Create("T", "t", VirtualKeys.VK_T, 1.0, KeyType.Standard, "T"),
            KeyModel.Create("Y", "y", VirtualKeys.VK_Y, 1.0, KeyType.Standard, "Y"),
            KeyModel.Create("U", "u", VirtualKeys.VK_U, 1.0, KeyType.Standard, "U"),
            KeyModel.Create("I", "i", VirtualKeys.VK_I, 1.0, KeyType.Standard, "I"),
            KeyModel.Create("O", "o", VirtualKeys.VK_O, 1.0, KeyType.Standard, "O"),
            KeyModel.Create("P", "p", VirtualKeys.VK_P, 1.0, KeyType.Standard, "P"),
            KeyModel.Create("OEM_4", "[", VirtualKeys.VK_OEM_4, 1.0, KeyType.Standard, "{"),
            KeyModel.Create("OEM_6", "]", VirtualKeys.VK_OEM_6, 1.0, KeyType.Standard, "}"),
            KeyModel.Create("OEM_5", "\\", VirtualKeys.VK_OEM_5, 1.5, KeyType.Standard, "|")
        });

        // Row 3: ASDF
        rows.Add(new List<KeyModel>
        {
            KeyModel.Create("CAPS", "Caps Lock", VirtualKeys.VK_CAPITAL, 1.8, KeyType.Lock),
            KeyModel.Create("A", "a", VirtualKeys.VK_A, 1.0, KeyType.Standard, "A"),
            KeyModel.Create("S", "s", VirtualKeys.VK_S, 1.0, KeyType.Standard, "S"),
            KeyModel.Create("D", "d", VirtualKeys.VK_D, 1.0, KeyType.Standard, "D"),
            KeyModel.Create("F", "f", VirtualKeys.VK_F, 1.0, KeyType.Standard, "F"),
            KeyModel.Create("G", "g", VirtualKeys.VK_G, 1.0, KeyType.Standard, "G"),
            KeyModel.Create("H", "h", VirtualKeys.VK_H, 1.0, KeyType.Standard, "H"),
            KeyModel.Create("J", "j", VirtualKeys.VK_J, 1.0, KeyType.Standard, "J"),
            KeyModel.Create("K", "k", VirtualKeys.VK_K, 1.0, KeyType.Standard, "K"),
            KeyModel.Create("L", "l", VirtualKeys.VK_L, 1.0, KeyType.Standard, "L"),
            KeyModel.Create("OEM_1", ";", VirtualKeys.VK_OEM_1, 1.0, KeyType.Standard, ":"),
            KeyModel.Create("OEM_7", "'", VirtualKeys.VK_OEM_7, 1.0, KeyType.Standard, "\""),
            KeyModel.Create("ENTER", "Enter", VirtualKeys.VK_RETURN, 2.2, KeyType.Action)
        });

        // Row 4: ZXCV
        rows.Add(new List<KeyModel>
        {
            KeyModel.Create("LSHIFT", "Shift", VirtualKeys.VK_LSHIFT, 2.3, KeyType.Modifier),
            KeyModel.Create("Z", "z", VirtualKeys.VK_Z, 1.0, KeyType.Standard, "Z"),
            KeyModel.Create("X", "x", VirtualKeys.VK_X, 1.0, KeyType.Standard, "X"),
            KeyModel.Create("C", "c", VirtualKeys.VK_C, 1.0, KeyType.Standard, "C"),
            KeyModel.Create("V", "v", VirtualKeys.VK_V, 1.0, KeyType.Standard, "V"),
            KeyModel.Create("B", "b", VirtualKeys.VK_B, 1.0, KeyType.Standard, "B"),
            KeyModel.Create("N", "n", VirtualKeys.VK_N, 1.0, KeyType.Standard, "N"),
            KeyModel.Create("M", "m", VirtualKeys.VK_M, 1.0, KeyType.Standard, "M"),
            KeyModel.Create("OEM_COMMA", ",", VirtualKeys.VK_OEM_COMMA, 1.0, KeyType.Standard, "<"),
            KeyModel.Create("OEM_PERIOD", ".", VirtualKeys.VK_OEM_PERIOD, 1.0, KeyType.Standard, ">"),
            KeyModel.Create("OEM_2", "/", VirtualKeys.VK_OEM_2, 1.0, KeyType.Standard, "?"),
            KeyModel.Create("RSHIFT", "Shift", VirtualKeys.VK_RSHIFT, 2.7, KeyType.Modifier)
        });

        // Row 5: Controls & Space
        rows.Add(new List<KeyModel>
        {
            KeyModel.Create("LCTRL", "Ctrl", VirtualKeys.VK_LCONTROL, 1.5, KeyType.Modifier),
            KeyModel.Create("FN", "Fn", 0, 1.0, KeyType.Action, specialAction: "FnQuickActions", toolTip: "Quick System & Asus Shortcuts"),
            KeyModel.Create("LWIN", "Win", VirtualKeys.VK_LWIN, 1.2, KeyType.Modifier, isExtended: true),
            KeyModel.Create("LALT", "Alt", VirtualKeys.VK_LMENU, 1.3, KeyType.Modifier),
            KeyModel.Create("SPACE", "Space", VirtualKeys.VK_SPACE, 5.5, KeyType.Standard),
            KeyModel.Create("RALT", "Alt", VirtualKeys.VK_RMENU, 1.3, KeyType.Modifier),
            KeyModel.Create("RCTRL", "Ctrl", VirtualKeys.VK_RCONTROL, 1.5, KeyType.Modifier, isExtended: true),
            KeyModel.Create("LEFT", "←", VirtualKeys.VK_LEFT, 1.0, KeyType.Navigation, isExtended: true),
            KeyModel.Create("UP", "↑", VirtualKeys.VK_UP, 1.0, KeyType.Navigation, isExtended: true),
            KeyModel.Create("DOWN", "↓", VirtualKeys.VK_DOWN, 1.0, KeyType.Navigation, isExtended: true),
            KeyModel.Create("RIGHT", "→", VirtualKeys.VK_RIGHT, 1.0, KeyType.Navigation, isExtended: true)
        });

        return rows;
    }

    public List<List<KeyModel>> GetNavigationCluster()
    {
        return new List<List<KeyModel>>
        {
            new List<KeyModel>
            {
                KeyModel.Create("INS_NAV", "Ins", VirtualKeys.VK_INSERT, 1.0, KeyType.Navigation, isExtended: true),
                KeyModel.Create("HOME", "Home", VirtualKeys.VK_HOME, 1.0, KeyType.Navigation, isExtended: true),
                KeyModel.Create("PGUP", "PgUp", VirtualKeys.VK_PRIOR, 1.0, KeyType.Navigation, isExtended: true)
            },
            new List<KeyModel>
            {
                KeyModel.Create("DEL_NAV", "Del", VirtualKeys.VK_DELETE, 1.0, KeyType.Navigation, isExtended: true),
                KeyModel.Create("END", "End", VirtualKeys.VK_END, 1.0, KeyType.Navigation, isExtended: true),
                KeyModel.Create("PGDN", "PgDn", VirtualKeys.VK_NEXT, 1.0, KeyType.Navigation, isExtended: true)
            }
        };
    }

    public List<List<KeyModel>> GetNumpadRows()
    {
        return new List<List<KeyModel>>
        {
            new List<KeyModel>
            {
                KeyModel.Create("NUM_LOCK", "Num", VirtualKeys.VK_NUMLOCK, 1.0, KeyType.Lock, isExtended: true),
                KeyModel.Create("NUM_DIV", "/", VirtualKeys.VK_DIVIDE, 1.0, KeyType.Numpad, isExtended: true),
                KeyModel.Create("NUM_MUL", "*", VirtualKeys.VK_MULTIPLY, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_SUB", "-", VirtualKeys.VK_SUBTRACT, 1.0, KeyType.Numpad)
            },
            new List<KeyModel>
            {
                KeyModel.Create("NUM_7", "7", VirtualKeys.VK_NUMPAD7, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_8", "8", VirtualKeys.VK_NUMPAD8, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_9", "9", VirtualKeys.VK_NUMPAD9, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_ADD", "+", VirtualKeys.VK_ADD, 1.0, KeyType.Numpad)
            },
            new List<KeyModel>
            {
                KeyModel.Create("NUM_4", "4", VirtualKeys.VK_NUMPAD4, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_5", "5", VirtualKeys.VK_NUMPAD5, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_6", "6", VirtualKeys.VK_NUMPAD6, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_EMPTY", "", 0, 1.0, KeyType.Action, specialAction: "None") // spacer / visual balance
            },
            new List<KeyModel>
            {
                KeyModel.Create("NUM_1", "1", VirtualKeys.VK_NUMPAD1, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_2", "2", VirtualKeys.VK_NUMPAD2, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_3", "3", VirtualKeys.VK_NUMPAD3, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_ENTER", "Ent", VirtualKeys.VK_RETURN, 1.0, KeyType.Numpad, isExtended: true)
            },
            new List<KeyModel>
            {
                KeyModel.Create("NUM_0", "0", VirtualKeys.VK_NUMPAD0, 2.0, KeyType.Numpad),
                KeyModel.Create("NUM_DOT", ".", VirtualKeys.VK_DECIMAL, 1.0, KeyType.Numpad),
                KeyModel.Create("NUM_EMPTY2", "", 0, 1.0, KeyType.Action, specialAction: "None")
            }
        };
    }
}
