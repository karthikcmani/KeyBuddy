using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace KeyBuddy.Services;

public class SystemUtilityService
{
    private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "KeyBuddy";

    public void OpenWindowsOSK()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "osk.exe",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open OSK: {ex.Message}");
        }
    }

    public void OpenTaskManager()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskmgr.exe",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open Task Manager: {ex.Message}");
        }
    }

    public void OpenFileExplorer()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open Explorer: {ex.Message}");
        }
    }

    public void OpenWindowsSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open Settings: {ex.Message}");
        }
    }

    public void OpenVolumeSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:sound",
                UseShellExecute = true
            });
        }
        catch { }
    }

    public void SetStartupRun(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
            if (key == null) return;

            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            if (enable && !string.IsNullOrEmpty(exePath))
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set startup status: {ex.Message}");
        }
    }

    public bool IsStartupRunEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }
}
