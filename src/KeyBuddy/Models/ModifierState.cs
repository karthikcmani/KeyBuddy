using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KeyBuddy.Models;

public enum ShiftState
{
    Off,
    Once,  // active for 1 character, auto resets
    Locked // sticky / locked shift
}

public class ModifierState : INotifyPropertyChanged
{
    private ShiftState _shift = ShiftState.Off;
    private bool _capsLock;
    private bool _ctrl;
    private bool _alt;
    private bool _windows;
    private bool _numLock;

    public ShiftState Shift
    {
        get => _shift;
        set
        {
            if (_shift != value)
            {
                _shift = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsShiftActive));
                OnPropertyChanged(nameof(IsShiftLocked));
                OnPropertyChanged(nameof(ShiftStatusText));
            }
        }
    }

    public bool IsShiftActive => _shift != ShiftState.Off;
    public bool IsShiftLocked => _shift == ShiftState.Locked;

    public string ShiftStatusText => _shift switch
    {
        ShiftState.Once => "SHIFT (1x)",
        ShiftState.Locked => "SHIFT (LOCK)",
        _ => "Shift: Off"
    };

    public bool CapsLock
    {
        get => _capsLock;
        set
        {
            if (_capsLock != value)
            {
                _capsLock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CapsLockStatusText));
            }
        }
    }

    public string CapsLockStatusText => _capsLock ? "CAPS: ON" : "CAPS: OFF";

    public bool Ctrl
    {
        get => _ctrl;
        set
        {
            if (_ctrl != value)
            {
                _ctrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CtrlStatusText));
            }
        }
    }

    public string CtrlStatusText => _ctrl ? "CTRL: ON" : "Ctrl: Off";

    public bool Alt
    {
        get => _alt;
        set
        {
            if (_alt != value)
            {
                _alt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AltStatusText));
            }
        }
    }

    public string AltStatusText => _alt ? "ALT: ON" : "Alt: Off";

    public bool WindowsKey
    {
        get => _windows;
        set
        {
            if (_windows != value)
            {
                _windows = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowsStatusText));
            }
        }
    }

    public string WindowsStatusText => _windows ? "WIN: ON" : "Win: Off";

    public bool NumLock
    {
        get => _numLock;
        set
        {
            if (_numLock != value)
            {
                _numLock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NumLockStatusText));
            }
        }
    }

    public string NumLockStatusText => _numLock ? "NUM: ON" : "Num: Off";

    public void ToggleShift()
    {
        Shift = _shift switch
        {
            ShiftState.Off => ShiftState.Once,
            ShiftState.Once => ShiftState.Locked,
            ShiftState.Locked => ShiftState.Off,
            _ => ShiftState.Off
        };
    }

    public void ConsumeOneShotShift()
    {
        if (Shift == ShiftState.Once)
        {
            Shift = ShiftState.Off;
        }
    }

    public void ResetTemporaryModifiers()
    {
        if (Shift == ShiftState.Once)
        {
            Shift = ShiftState.Off;
        }
        Ctrl = false;
        Alt = false;
        WindowsKey = false;
    }

    public void ResetAll()
    {
        Shift = ShiftState.Off;
        Ctrl = false;
        Alt = false;
        WindowsKey = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
