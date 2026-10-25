using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using KeyBuddy.Models;
using KeyBuddy.Native;
using KeyBuddy.Services;

namespace KeyBuddy.LiveTest;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("KeyBuddy Live Windows End-to-End Simulation Test");
        Console.WriteLine("==================================================");

        string testFilePath = Path.Combine(Path.GetTempPath(), "KeyBuddy_Test_Output.txt");
        if (File.Exists(testFilePath)) File.Delete(testFilePath);
        File.WriteAllText(testFilePath, "");

        Console.WriteLine($"Starting Notepad with file: {testFilePath}");
        var proc = Process.Start("notepad.exe", testFilePath);
        Thread.Sleep(2000); // Give Notepad time to launch and become foreground window

        try
        {
            IntPtr notepadHwnd = proc.MainWindowHandle;
            if (notepadHwnd == IntPtr.Zero)
            {
                notepadHwnd = NativeMethods.GetForegroundWindow();
            }

            Console.WriteLine($"Notepad HWND: {notepadHwnd}");
            NativeMethods.SetForegroundWindow(notepadHwnd);
            Thread.Sleep(500);

            var inputService = new KeyboardInputService();
            var modifiers = new ModifierState();

            // 1. Send "Hello"
            Console.WriteLine("Typing: 'Hello'");
            // Shift + H
            modifiers.Shift = ShiftState.Once;
            inputService.SendKeyPress(KeyModel.Create("H", "h", VirtualKeys.VK_H), modifiers);
            Thread.Sleep(50);
            inputService.SendKeyPress(KeyModel.Create("E", "e", VirtualKeys.VK_E), modifiers);
            Thread.Sleep(50);
            inputService.SendKeyPress(KeyModel.Create("L", "l", VirtualKeys.VK_L), modifiers);
            Thread.Sleep(50);
            inputService.SendKeyPress(KeyModel.Create("L", "l", VirtualKeys.VK_L), modifiers);
            Thread.Sleep(50);
            inputService.SendKeyPress(KeyModel.Create("O", "o", VirtualKeys.VK_O), modifiers);
            Thread.Sleep(50);

            // Space
            inputService.SendKeyPress(KeyModel.Create("SPACE", "Space", VirtualKeys.VK_SPACE), modifiers);
            Thread.Sleep(50);

            // 2. Send Unicode text
            Console.WriteLine("Typing Unicode text: 'KeyBuddy 123'");
            inputService.SendUnicodeText("KeyBuddy 123");
            Thread.Sleep(100);

            // 3. Test Backspace
            Console.WriteLine("Simulating Backspace (deleting '3')");
            inputService.SendKeyPress(KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK), modifiers);
            Thread.Sleep(100);

            // 4. Test Enter
            Console.WriteLine("Simulating Enter newline");
            inputService.SendKeyPress(KeyModel.Create("ENTER", "Enter", VirtualKeys.VK_RETURN), modifiers);
            Thread.Sleep(100);

            // 5. Test Ctrl+S to save in Notepad
            Console.WriteLine("Simulating Ctrl+S (Save)");
            modifiers.Ctrl = true;
            inputService.SendKeyPress(KeyModel.Create("S", "s", VirtualKeys.VK_S), modifiers);
            Thread.Sleep(1000);

            Console.WriteLine("Verification complete! Keystrokes dispatched via SendInput.");
        }
        finally
        {
            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
            }
            catch { }
        }

        if (File.Exists(testFilePath))
        {
            string savedContent = File.ReadAllText(testFilePath);
            Console.WriteLine($"Saved content in Notepad file:\n'{savedContent}'");
        }
    }
}
