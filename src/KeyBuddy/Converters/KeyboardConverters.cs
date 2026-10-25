using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using KeyBuddy.Models;

namespace KeyBuddy.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is bool flag && flag;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class KeyDisplayConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return string.Empty;

        var key = values[0] as KeyModel;
        var shiftState = values[1] is ShiftState s ? s : ShiftState.Off;
        var capsLock = values[2] is bool b && b;

        if (key == null) return string.Empty;

        bool isShiftActive = shiftState != ShiftState.Off;

        // If secondary symbol exists (e.g. 1 -> !, - -> _)
        if (!string.IsNullOrEmpty(key.DisplaySecondary))
        {
            return isShiftActive ? key.DisplaySecondary : key.DisplayPrimary;
        }

        // Standard letters: affected by Shift and Caps Lock
        if (key.KeyType == KeyType.Standard && key.DisplayPrimary.Length == 1 && char.IsLetter(key.DisplayPrimary[0]))
        {
            // XOR logic: CapsLock XOR Shift produces uppercase
            bool uppercase = capsLock ^ isShiftActive;
            return uppercase ? key.DisplayPrimary.ToUpperInvariant() : key.DisplayPrimary.ToLowerInvariant();
        }

        return key.DisplayPrimary;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class KeyActiveConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;

        var key = values[0] as KeyModel;
        var modState = values[1] as ModifierState;

        if (key == null || modState == null) return false;

        return key.Id switch
        {
            "LSHIFT" or "RSHIFT" => modState.IsShiftActive,
            "LCTRL" or "RCTRL" => modState.Ctrl,
            "LALT" or "RALT" => modState.Alt,
            "LWIN" => modState.WindowsKey,
            "CAPS" => modState.CapsLock,
            "NUM_LOCK" => modState.NumLock,
            _ => false
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
