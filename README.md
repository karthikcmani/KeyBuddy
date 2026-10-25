# KeyBuddy — Custom On-Screen Keyboard for Windows 11

KeyBuddy is a native, production-quality Windows On-Screen Keyboard application built with **C# / .NET 8** and **WPF**. It is designed specifically for scenarios where physical laptop keyboards become intermittent or unresponsive (such as hardware faults on ASUS VivoBook laptops).

Unlike simple text injectors or web-based simulated keyboards, **KeyBuddy** is a true virtual keyboard:
- Utilizes the Windows `SendInput` API to dispatch real scan codes and virtual keys to **any active Windows application** (Notepad, Microsoft Edge, Google Chrome, VS Code, Word, Terminal, File Explorer, etc.).
- Utilizes the Win32 `WS_EX_NOACTIVATE` and `WM_MOUSEACTIVATE` window styles so that **clicking keys on the virtual keyboard never steals keyboard focus** from your target application.
- Supports **single clicks**, **press-and-hold repetition** (for Backspace, Delete, and Arrow keys), sticky **Shift** modes (one-shot and locked), **Caps Lock synchronization**, modifier combinations (**Ctrl+C**, **Ctrl+V**, **Win+E**, **Alt+Tab**, etc.), and safety auto-release.
- Features a **collapsible full layout**: Function row (F1–F12, Esc, PrtSc, Ins, Del), complete QWERTY rows, Navigation cluster (Home, End, PgUp, PgDn, Ins, Del), and a toggleable numeric keypad (Numpad).
- Includes **emergency mouse-accessible fallback buttons** to launch Windows OSK, Task Manager, File Explorer, and Windows Settings with one click.

---

## 🚀 Quick Launch (Self-Contained Executable)

A fully self-contained, standalone single-file executable is built and ready in the repository:

```
d:\On-Screen Keyboard\publish\KeyBuddy.exe
```

- **Zero dependencies**: No need to install any additional .NET runtime.
- **Double-click to run**: Runs immediately on Windows 11 x64.
- **No administrator privileges required**: Operates cleanly in standard user space.

---

## 🛠️ How to Build and Publish

### 1. Build Solution (Release)
Run the provided build script:
```powershell
.\build.ps1
```
Or directly via .NET CLI:
```powershell
C:\Users\Asus\AppData\Local\Microsoft\dotnet\dotnet.exe build KeyBuddy.sln -c Release
```

### 2. Publish Self-Contained Executable
Run the provided publish script:
```powershell
.\publish.ps1
```
Or directly:
```powershell
C:\Users\Asus\AppData\Local\Microsoft\dotnet\dotnet.exe publish src/KeyBuddy/KeyBuddy.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

---

## 🖥️ Desktop Shortcut Setup

To create a shortcut to KeyBuddy directly on your Windows Desktop:

1. Right-click on your Windows Desktop.
2. Select **New > Shortcut**.
3. In the location box, enter:
   ```
   "d:\On-Screen Keyboard\publish\KeyBuddy.exe"
   ```
4. Name the shortcut **KeyBuddy**.
5. Click **Finish**.

Alternatively, you can run this one-line PowerShell command:
```powershell
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$([Environment]::GetFolderPath('Desktop'))\KeyBuddy.lnk")
$Shortcut.TargetPath = "d:\On-Screen Keyboard\publish\KeyBuddy.exe"
$Shortcut.WorkingDirectory = "d:\On-Screen Keyboard\publish"
$Shortcut.Description = "KeyBuddy On-Screen Keyboard"
$Shortcut.Save()
```

---

## 📖 Usage Guide

### 1. Targeting an Application
1. Click inside any application where you want to type (e.g. Notepad, Chrome, or VS Code).
2. Look at the top status bar of KeyBuddy: it will display **🎯 Target: [App Name]**.
3. Click any key on KeyBuddy; the keystroke is delivered directly to your target window without switching focus away.

### 2. Typing Letters, Numbers & Symbols
- Clicking letter keys sends lowercase letters by default.
- If **Caps Lock** or **Shift** is enabled, letters automatically capitalize.
- For keys with dual symbols (such as `1 / !` or `, / <`), activating Shift automatically flips the key labels and sends the shifted symbol.

### 3. Modifiers & Combinations
- **Shift**:
  - *Click once*: Activates one-shot Shift (`SHIFT (1x)`). The next key pressed will be capitalized / shifted, and Shift automatically deactivates.
  - *Click twice*: Locks Shift (`SHIFT (LOCK)`). Useful for typing all caps or multiple symbols in a row without holding.
  - *Click a third time*: Turns Shift off.
- **Ctrl, Alt, Win**:
  - Click the modifier button (e.g., `Ctrl`), its status pill illuminates at the top.
  - Click your target key (e.g., `C` for copy, `V` for paste, `A` for select all).
  - The combination is dispatched safely via `SendInput`, and the modifier turns off automatically.
- **Emergency Modifier Reset**:
  - Click the **🔄 Reset Mods** button on the status bar anytime to ensure no keys remain logically held down.

### 4. Press and Hold Repetition
- Press and hold **Backspace**, **Delete**, or any **Arrow key** to smoothly repeat deletion or cursor navigation.
- Release the mouse button to stop repeating immediately.

### 5. Floating Mode & Window Controls
- **Always On Top (📌 Pin)**: Click to toggle always-on-top so KeyBuddy stays visible over full-screen browser or document windows.
- **Draggable Window**: Click and drag anywhere along the top title bar to reposition the keyboard.
- **Window Opacity**: Open **⚙️ Settings** to adjust opacity from 30% to 100%.
- **Compact Mode**: Collapse the function row and clusters to save vertical screen space.
- **Window Persistence**: Last window position, size, and settings are automatically saved in `%APPDATA%\KeyBuddy\settings.json` and restored on startup with multi-monitor boundary validation.

### 6. Emergency Quick Access
If your physical keyboard completely stops responding:
- Click **⚡ TaskMgr** to launch Windows Task Manager directly.
- Click **📁 Files** to open File Explorer.
- Click **🪟 WinOSK** to open the built-in Windows On-Screen Keyboard as a backup.
- Click **⚙️ WinSet** to open Windows Settings.
- Click **Fn** on the bottom row to open the quick system actions overlay (volume control, mute, media controls, Lock screen Win+L, Project display Win+P).

---

## 🧪 Testing Results

### Automated Test Suite (`KeyBuddy.Tests`)
Run using:
```powershell
C:\Users\Asus\AppData\Local\Microsoft\dotnet\dotnet.exe test
```

**Results:**
- Total Tests: **14**
- Passed: **14**
- Failed: **0**
- Duration: **~50 ms**

Test areas covered:
1. `TestFunctionRowContainsAllKeys`: Validates Esc, F1–F12, PrtSc, Ins, Del.
2. `TestMainRowsContainAllStandardRows`: Validates 5 standard QWERTY rows, control keys, Tab, Enter, Space.
3. `TestNavigationClusterKeys`: Validates Insert, Home, PgUp, Delete, End, PgDn.
4. `TestNumpadKeys`: Validates standard 10-key numpad keys, operators, and Num Lock.
5. `TestShiftToggleCycle`: Validates Off -> Once -> Locked -> Off transition cycle.
6. `TestOneShotShiftConsumption`: Verifies that single-character Shift automatically resets while locked Shift persists.
7. `TestModifierResetSafety`: Verifies reset of all simulated modifier states.
8. `TestLetterKeyDisplayWithShiftAndCaps`: Validates letter casing and XOR logic between Caps Lock and Shift.
9. `TestSymbolKeyDisplayWithShift`: Validates numeric and punctuation shift symbol switching.
10. `TestShortcutCtrlCExecution`: Validates Ctrl+C combination dispatch and modifier release.
11. `TestModifierShiftPlusLetterExecution`: Validates Shift+letter combination.
12. `TestMultiModifierWinPlusEExecution`: Validates Windows key combinations.
13. `TestUnicodeDirectTextInjection`: Validates Unicode character sending.
14. `TestKeyActiveConverter`: Validates UI button active state converters.

---

## 🔒 Reliability, Security & Architecture
- **No Background Keylogger**: KeyBuddy does not install global low-level keyboard hooks and does not record or transmit keystrokes.
- **100% Offline**: No network calls, telemetry, accounts, or subscriptions.
- **Non-Activating Window**: Uses Win32 window style `WS_EX_NOACTIVATE` (0x08000000) and intercepts `WM_MOUSEACTIVATE` returning `MA_NOACTIVATE`. This is Microsoft's officially documented approach for on-screen keyboards.
- **UIPI (User Interface Privilege Isolation)**: When typing into administrative / elevated windows, Windows UIPI prevents non-elevated apps from sending input. If typing into an elevated app (e.g. an Administrator terminal), launch KeyBuddy as Administrator.

---

## 📂 Project Structure

```
d:\On-Screen Keyboard\
├── KeyBuddy.sln                     # Main Visual Studio / .NET solution
├── build.ps1                        # Build script for Release build
├── publish.ps1                      # Publish script for self-contained executable
├── README.md                        # Documentation and user guide
├── src\
│   └── KeyBuddy\
│       ├── KeyBuddy.csproj          # WPF Project configuration (.NET 8 Windows)
│       ├── App.xaml                 # Application definition & resource merge
│       ├── App.xaml.cs              # Startup logic
│       ├── MainWindow.xaml          # Full XAML Keyboard UI & Overlays
│       ├── MainWindow.xaml.cs       # Win32 focus handling & dynamic rendering
│       ├── Models\
│       │   ├── KeyModel.cs          # Key data model
│       │   ├── VirtualKeys.cs       # Virtual Key constants
│       │   ├── ModifierState.cs     # Modifier state & shift lock logic
│       │   └── AppSettings.cs       # Persistent user settings model
│       ├── Native\
│       │   └── NativeMethods.cs     # Win32 SendInput, GetForegroundWindow, SetWindowLongPtr
│       ├── Services\
│       │   ├── KeyboardInputService.cs   # SendInput execution & modifier safety
│       │   ├── KeyboardLayoutService.cs  # Layout definitions (Function, QWERTY, Nav, Numpad)
│       │   ├── TargetWindowManager.cs    # Focus preservation & target detection
│       │   └── SystemUtilityService.cs   # Windows OSK, TaskMgr, Startup registry
│       ├── ViewModels\
│       │   ├── MainViewModel.cs     # MVVM ViewModel coordinating logic
│       │   └── RelayCommand.cs      # ICommand implementation
│       ├── Converters\
│       │   └── KeyboardConverters.cs# XAML MultiValueConverters for Shift/Caps
│       └── Themes\
│           └── KeyboardTheme.xaml   # Windows 11 Fluent dark theme styling
├── tests\
│   ├── KeyBuddy.Tests\              # Automated Unit Test Project
│   │   ├── KeyBuddy.Tests.csproj
│   │   ├── LayoutAndModelTests.cs
│   │   ├── ModifierAndConverterTests.cs
│   │   └── InputSimulationTests.cs
│   └── KeyBuddy.LiveTest\           # Live Windows integration test harness
└── publish\
    └── KeyBuddy.exe                 # Self-contained executable ready to run
```
