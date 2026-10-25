using System;
using System.Diagnostics;
using System.Text;
using KeyBuddy.Native;

namespace KeyBuddy.Services;

public class TargetWindowManager
{
    private IntPtr _lastTargetHwnd = IntPtr.Zero;
    private string _lastTargetTitle = "None (Click any app)";
    private string _lastProcessName = "None";

    public IntPtr LastTargetHwnd => _lastTargetHwnd;
    public string LastTargetTitle => _lastTargetTitle;
    public string LastProcessName => _lastProcessName;

    public event Action<IntPtr, string>? TargetWindowChanged;

    public void RegisterPossibleTarget(IntPtr hwnd, IntPtr myWindowHwnd)
    {
        if (hwnd == IntPtr.Zero || hwnd == myWindowHwnd) return;

        // Skip our own process windows
        NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
        if (pid == Process.GetCurrentProcess().Id) return;

        // Get root ancestor
        IntPtr rootHwnd = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOT);
        if (rootHwnd == IntPtr.Zero) rootHwnd = hwnd;

        if (!NativeMethods.IsWindow(rootHwnd) || !NativeMethods.IsWindowVisible(rootHwnd)) return;

        var sb = new StringBuilder(256);
        NativeMethods.GetWindowText(rootHwnd, sb, 256);
        string title = sb.ToString().Trim();

        string procName = "App";
        try
        {
            var proc = Process.GetProcessById((int)pid);
            procName = proc.ProcessName;
        }
        catch { }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = $"{procName} (Window)";
        }

        if (_lastTargetHwnd != rootHwnd || _lastTargetTitle != title)
        {
            _lastTargetHwnd = rootHwnd;
            _lastTargetTitle = title;
            _lastProcessName = procName;
            TargetWindowChanged?.Invoke(_lastTargetHwnd, _lastTargetTitle);
        }
    }

    public bool EnsureTargetFocus(IntPtr myWindowHwnd)
    {
        if (_lastTargetHwnd != IntPtr.Zero && NativeMethods.IsWindow(_lastTargetHwnd))
        {
            IntPtr currentForeground = NativeMethods.GetForegroundWindow();
            if (currentForeground != _lastTargetHwnd)
            {
                NativeMethods.SetForegroundWindow(_lastTargetHwnd);
            }
            return true;
        }

        // Try getting foreground if valid
        IntPtr foreground = NativeMethods.GetForegroundWindow();
        if (foreground != IntPtr.Zero && foreground != myWindowHwnd)
        {
            RegisterPossibleTarget(foreground, myWindowHwnd);
            return true;
        }

        return false;
    }

    public void ClearTarget()
    {
        _lastTargetHwnd = IntPtr.Zero;
        _lastTargetTitle = "None (Click any app)";
        _lastProcessName = "None";
        TargetWindowChanged?.Invoke(_lastTargetHwnd, _lastTargetTitle);
    }
}
