using System.Globalization;
using KeyBuddy.Converters;
using KeyBuddy.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBuddy.Tests;

[TestClass]
public class ModifierAndConverterTests
{
    private ModifierState _modifiers = null!;
    private KeyDisplayConverter _displayConverter = null!;

    [TestInitialize]
    public void Setup()
    {
        _modifiers = new ModifierState();
        _displayConverter = new KeyDisplayConverter();
    }

    [TestMethod]
    public void TestShiftToggleCycle()
    {
        Assert.AreEqual(ShiftState.Off, _modifiers.Shift);

        // Click once -> One-shot
        _modifiers.ToggleShift();
        Assert.AreEqual(ShiftState.Once, _modifiers.Shift);
        Assert.IsTrue(_modifiers.IsShiftActive);
        Assert.IsFalse(_modifiers.IsShiftLocked);

        // Click second time -> Locked
        _modifiers.ToggleShift();
        Assert.AreEqual(ShiftState.Locked, _modifiers.Shift);
        Assert.IsTrue(_modifiers.IsShiftActive);
        Assert.IsTrue(_modifiers.IsShiftLocked);

        // Click third time -> Off
        _modifiers.ToggleShift();
        Assert.AreEqual(ShiftState.Off, _modifiers.Shift);
        Assert.IsFalse(_modifiers.IsShiftActive);
    }

    [TestMethod]
    public void TestOneShotShiftConsumption()
    {
        _modifiers.Shift = ShiftState.Once;
        _modifiers.ConsumeOneShotShift();
        Assert.AreEqual(ShiftState.Off, _modifiers.Shift);

        // Locked shift should not be consumed by a single key
        _modifiers.Shift = ShiftState.Locked;
        _modifiers.ConsumeOneShotShift();
        Assert.AreEqual(ShiftState.Locked, _modifiers.Shift);
    }

    [TestMethod]
    public void TestModifierResetSafety()
    {
        _modifiers.Shift = ShiftState.Once;
        _modifiers.Ctrl = true;
        _modifiers.Alt = true;
        _modifiers.WindowsKey = true;

        _modifiers.ResetAll();

        Assert.AreEqual(ShiftState.Off, _modifiers.Shift);
        Assert.IsFalse(_modifiers.Ctrl);
        Assert.IsFalse(_modifiers.Alt);
        Assert.IsFalse(_modifiers.WindowsKey);
    }

    [TestMethod]
    public void TestLetterKeyDisplayWithShiftAndCaps()
    {
        var keyA = KeyModel.Create("A", "a", VirtualKeys.VK_A, type: KeyType.Standard);

        // Default: lowercase 'a'
        object result1 = _displayConverter.Convert(
            new object[] { keyA, ShiftState.Off, false },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("a", result1);

        // Shift active: uppercase 'A'
        object result2 = _displayConverter.Convert(
            new object[] { keyA, ShiftState.Once, false },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("A", result2);

        // Caps Lock active: uppercase 'A'
        object result3 = _displayConverter.Convert(
            new object[] { keyA, ShiftState.Off, true },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("A", result3);

        // Caps Lock + Shift: inverts to lowercase 'a'
        object result4 = _displayConverter.Convert(
            new object[] { keyA, ShiftState.Once, true },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("a", result4);
    }

    [TestMethod]
    public void TestSymbolKeyDisplayWithShift()
    {
        var key1 = KeyModel.Create("1", "1", VirtualKeys.VK_1, secondary: "!");

        // Without shift: '1'
        object res1 = _displayConverter.Convert(
            new object[] { key1, ShiftState.Off, false },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("1", res1);

        // With shift: '!'
        object res2 = _displayConverter.Convert(
            new object[] { key1, ShiftState.Once, false },
            typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.AreEqual("!", res2);
    }
}
