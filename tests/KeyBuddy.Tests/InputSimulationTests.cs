using System.Collections.Generic;
using KeyBuddy.Models;
using KeyBuddy.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBuddy.Tests;

public class MockKeyboardInputService : IKeyboardInputService
{
    public List<string> EventsLogged = new();
    public bool ModifiersReleased = false;

    public void SendKeyPress(KeyModel key, ModifierState modifiers)
    {
        string prefix = "";
        if (modifiers.Ctrl) prefix += "CTRL+";
        if (modifiers.Alt) prefix += "ALT+";
        if (modifiers.WindowsKey) prefix += "WIN+";
        if (modifiers.IsShiftActive) prefix += "SHIFT+";

        EventsLogged.Add($"{prefix}{key.DisplayPrimary}");

        // Simulate release
        modifiers.ConsumeOneShotShift();
        modifiers.Ctrl = false;
        modifiers.Alt = false;
        modifiers.WindowsKey = false;
    }

    public void SendKeyDown(KeyModel key, ModifierState modifiers) => EventsLogged.Add($"DOWN_{key.DisplayPrimary}");
    public void SendKeyUp(KeyModel key) => EventsLogged.Add($"UP_{key.DisplayPrimary}");
    public void SendUnicodeText(string text) => EventsLogged.Add($"UNICODE:{text}");
    public void ReleaseAllModifiers() => ModifiersReleased = true;
    public void SynchronizeLockStates(ModifierState modifiers) { }
}

[TestClass]
public class InputSimulationTests
{
    [TestMethod]
    public void TestShortcutCtrlCExecution()
    {
        var mockService = new MockKeyboardInputService();
        var modifiers = new ModifierState();

        modifiers.Ctrl = true;
        var keyC = KeyModel.Create("C", "C", VirtualKeys.VK_C);

        mockService.SendKeyPress(keyC, modifiers);

        Assert.AreEqual(1, mockService.EventsLogged.Count);
        Assert.AreEqual("CTRL+C", mockService.EventsLogged[0]);
        // Modifiers must be reset after combination
        Assert.IsFalse(modifiers.Ctrl);
    }

    [TestMethod]
    public void TestModifierShiftPlusLetterExecution()
    {
        var mockService = new MockKeyboardInputService();
        var modifiers = new ModifierState();

        modifiers.Shift = ShiftState.Once;
        var keyA = KeyModel.Create("A", "a", VirtualKeys.VK_A);

        mockService.SendKeyPress(keyA, modifiers);

        Assert.AreEqual(1, mockService.EventsLogged.Count);
        Assert.AreEqual("SHIFT+a", mockService.EventsLogged[0]);
        Assert.AreEqual(ShiftState.Off, modifiers.Shift);
    }

    [TestMethod]
    public void TestMultiModifierWinPlusEExecution()
    {
        var mockService = new MockKeyboardInputService();
        var modifiers = new ModifierState();

        modifiers.WindowsKey = true;
        var keyE = KeyModel.Create("E", "E", VirtualKeys.VK_E);

        mockService.SendKeyPress(keyE, modifiers);

        Assert.AreEqual(1, mockService.EventsLogged.Count);
        Assert.AreEqual("WIN+E", mockService.EventsLogged[0]);
        Assert.IsFalse(modifiers.WindowsKey);
    }

    [TestMethod]
    public void TestUnicodeDirectTextInjection()
    {
        var mockService = new MockKeyboardInputService();
        mockService.SendUnicodeText("Hello, World! 🚀");

        Assert.AreEqual(1, mockService.EventsLogged.Count);
        Assert.AreEqual("UNICODE:Hello, World! 🚀", mockService.EventsLogged[0]);
    }
}
