using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using KeyBuddy.Native;
using KeyBuddy.Services;

namespace KeyBuddy;

public partial class SpecialKeysWindow : Window
{
    private readonly IKeyboardInputService _inputService;
    private HwndSource? _hwndSource;

    public SpecialKeysWindow(IKeyboardInputService inputService)
    {
        InitializeComponent();
        _inputService = inputService;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        IntPtr hwnd = helper.Handle;

        // Apply WS_EX_NOACTIVATE style so clicking symbol buttons does NOT steal keyboard focus from target application!
        int exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt32();
        exStyle |= NativeMethods.WS_EX_NOACTIVATE;
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));

        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_MOUSEACTIVATE)
        {
            handled = true;
            return new IntPtr(NativeMethods.MA_NOACTIVATE);
        }

        if (msg == 0x0084 /* WM_NCHITTEST */)
        {
            int x = (short)(lParam.ToInt32() & 0xFFFF);
            int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
            Point pt = PointFromScreen(new Point(x, y));

            const int border = 6;
            bool left = pt.X <= border;
            bool right = pt.X >= ActualWidth - border;
            bool top = pt.Y <= border;
            bool bottom = pt.Y >= ActualHeight - border;

            if (top && left) { handled = true; return new IntPtr(13); }
            if (top && right) { handled = true; return new IntPtr(14); }
            if (bottom && left) { handled = true; return new IntPtr(16); }
            if (bottom && right) { handled = true; return new IntPtr(17); }
            if (left) { handled = true; return new IntPtr(10); }
            if (right) { handled = true; return new IntPtr(11); }
            if (top) { handled = true; return new IntPtr(12); }
            if (bottom) { handled = true; return new IntPtr(15); }
        }

        return IntPtr.Zero;
    }

    private void OnSymbolClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string symbol)
        {
            _inputService.SendUnicodeText(symbol);
        }
    }

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cancel close and hide instead, so it can be re-shown fast
        e.Cancel = true;
        Hide();
    }
}
