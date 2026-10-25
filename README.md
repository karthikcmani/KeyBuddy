# KeyBuddy — Virtual Keyboard & Key-Rescue Utility for Windows 11 / 10

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010-0078D4.svg)](https://microsoft.com)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com)

**KeyBuddy** is a native, ultra-lightweight Windows desktop virtual keyboard and keyboard-rescue tool. 

If some keys on your laptop or mechanical keyboard have stopped working (due to hardware failure, matrix defect, or liquid spill), **KeyBuddy lets you keep working without buying a new laptop or keyboard.**

---

## ✨ Features

- 🎯 **Never Steals Keyboard Focus**: Built with native Win32 `WS_EX_NOACTIVATE` & `WM_MOUSEACTIVATE` architectures. Clicking keys on KeyBuddy delivers keystrokes directly into your target app (Notepad, Chrome, VS Code, Slack, Word, Terminal) without switching windows or stealing cursor focus.
- ⚡ **Rescue Quick Bar**: An ultra-compact floating widget ($300 \times 95$ px) showing only your chosen rescue keys (e.g., `P`, `4`, `⌫ Backspace`).
- ⌨️ **Hold-to-Replace Physical Key Shortcuts**:
  - Hold physical **`O`** $\rightarrow$ Types **`P`**
  - Hold physical **`2`** $\rightarrow$ Types **`4`**
  - Hold physical **`A`** $\rightarrow$ Types **`@`**
  - *Keep typing at high speed on your physical keyboard without reaching for the mouse!*
- 🔣 **Special Symbols Window**: One-click popup (`~#`) providing all special symbols (`~`, `!`, `@`, `#`, `$`, `%`, `^`, `&`, `*`, `(`, `)`, `_`) with square, comfortable buttons.
- 🎹 **Full Keyboard Mode**: Instant toggle to access the full standard keyboard with Function Row (`F1`–`F12`), Navigation Cluster (`Home`, `End`, `PgUp`, `PgDn`), and Numpad.
- 📌 **Always-on-Top & Position Memory**: Pin it to float above full-screen browsers. KeyBuddy automatically remembers its last coordinates across restarts.
- 🚀 **Auto-Start on Boot**: Optional zero-fuss launch at Windows startup.
- 🔒 **Safe & Private**: 100% offline, zero data collection, no telemetry, no administrator privileges required.

---

## 🚀 Quick Start (Running KeyBuddy)

### Requirements
- Windows 10 or Windows 11 (64-bit)
- No runtime installation needed (standalone self-contained executable)

### Build & Run
Clone the repository and build:
```powershell
git clone https://github.com/karthikcmani/KeyBuddy.git
cd KeyBuddy
.\build.ps1
```

To publish as a self-contained single-file `.exe`:
```powershell
.\publish.ps1
```
The output executable will be generated at `publish\KeyBuddy.exe`.

---

## 🛠️ Tech Stack & Architecture

- **Language & Framework**: C# with .NET 8 WPF
- **Input Dispatch**: Windows Win32 `SendInput` API for real hardware scan codes and virtual keys
- **Window Management**: Win32 `SetWindowLongPtr` with `WS_EX_NOACTIVATE` (`0x08000000`)
- **Key Hooking**: Low-level Windows keyboard hook (`WH_KEYBOARD_LL`) for hold-to-replace shortcuts
- **Styling**: Windows 11 Fluent Dark UI theme

---

## 🤝 Contributing

Contributions are welcome! If you have ideas for additional accessibility options, layout presets, or custom key mappings:
1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'add amazing feature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for more information.
