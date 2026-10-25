using System.Collections.Generic;
using KeyBuddy.Converters;
using KeyBuddy.Models;
using KeyBuddy.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyBuddy.Tests;

[TestClass]
public class LayoutAndModelTests
{
    private KeyboardLayoutService _layoutService = null!;

    [TestInitialize]
    public void Setup()
    {
        _layoutService = new KeyboardLayoutService();
    }

    [TestMethod]
    public void TestFunctionRowContainsAllKeys()
    {
        var fnRows = _layoutService.GetFunctionRow();
        Assert.AreEqual(1, fnRows.Count);
        var keys = fnRows[0];

        // Esc, F1-F12, PrtSc, Ins, Del = 16 keys
        Assert.AreEqual(16, keys.Count);
        Assert.AreEqual("ESC", keys[0].Id);
        Assert.AreEqual("DEL", keys[^1].Id);
    }

    [TestMethod]
    public void TestMainRowsContainAllStandardRows()
    {
        var mainRows = _layoutService.GetMainRows();
        // Number row, Tab row, Caps row, Shift row, Ctrl row = 5 rows
        Assert.AreEqual(5, mainRows.Count);

        // Check essential keys exist
        var row0 = mainRows[0]; // Number row: ` to Backspace
        Assert.AreEqual("OEM_3", row0[0].Id);
        Assert.AreEqual("BACKSPACE", row0[^1].Id);

        var row1 = mainRows[1]; // QWERTY: Tab to \
        Assert.AreEqual("TAB", row1[0].Id);
        Assert.AreEqual("OEM_5", row1[^1].Id);

        var row2 = mainRows[2]; // ASDF: Caps to Enter
        Assert.AreEqual("CAPS", row2[0].Id);
        Assert.AreEqual("ENTER", row2[^1].Id);

        var row3 = mainRows[3]; // ZXCV: LShift to RShift
        Assert.AreEqual("LSHIFT", row3[0].Id);
        Assert.AreEqual("RSHIFT", row3[^1].Id);

        var row4 = mainRows[4]; // Space & Controls
        Assert.IsTrue(row4.Exists(k => k.Id == "SPACE"));
        Assert.IsTrue(row4.Exists(k => k.Id == "LCTRL"));
        Assert.IsTrue(row4.Exists(k => k.Id == "LWIN"));
        Assert.IsTrue(row4.Exists(k => k.Id == "LALT"));
    }

    [TestMethod]
    public void TestNavigationClusterKeys()
    {
        var navRows = _layoutService.GetNavigationCluster();
        Assert.AreEqual(2, navRows.Count);
        Assert.AreEqual(3, navRows[0].Count); // Ins, Home, PgUp
        Assert.AreEqual(3, navRows[1].Count); // Del, End, PgDn
    }

    [TestMethod]
    public void TestNumpadKeys()
    {
        var numRows = _layoutService.GetNumpadRows();
        Assert.AreEqual(5, numRows.Count);
        Assert.IsTrue(numRows[0].Exists(k => k.Id == "NUM_LOCK"));
        Assert.IsTrue(numRows[3].Exists(k => k.Id == "NUM_ENTER"));
    }
}
