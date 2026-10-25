using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using KeyBuddy.Models;
using KeyBuddy.Native;
using KeyBuddy.Services;
using KeyBuddy.ViewModels;

namespace KeyBuddy;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly SystemUtilityService _sysUtils;
    private readonly DispatcherTimer _foregroundPollTimer;
    private readonly SpecialKeysWindow _specialKeysWindow;
    private readonly PhysicalKeyHookService _physicalKeyHook;
    private HwndSource? _hwndSource;

    public MainWindow()
    {
        InitializeComponent();

        var inputService = new KeyboardInputService();
        var layoutService = new KeyboardLayoutService();
        var targetManager = new TargetWindowManager();
        _sysUtils = new SystemUtilityService();

        _viewModel = new MainViewModel(inputService, layoutService, targetManager, _sysUtils);
        DataContext = _viewModel;

        _specialKeysWindow = new SpecialKeysWindow(inputService);
        _physicalKeyHook = new PhysicalKeyHookService(inputService, _viewModel.Modifiers);

        _viewModel.OnRequestToggleSpecialSymbols += ToggleSpecialKeysWindow;
        _viewModel.OnRequestModeChanged += ApplyLayoutModeDimensions;

        // Populate dynamic key layouts
        BuildQuickBarGrid();
        BuildMainKeyboardGrid();
        BuildNavClusterGrid();
        BuildNumpadGrid();

        // Timer to monitor foreground window changes without stealing focus
        _foregroundPollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _foregroundPollTimer.Tick += OnForegroundPollTick;
        _foregroundPollTimer.Start();

        Loaded += OnMainWindowLoaded;
        Closing += OnMainWindowClosing;
    }

    private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        IntPtr hwnd = helper.Handle;

        // Apply WS_EX_NOACTIVATE style so clicking the window NEVER steals keyboard focus!
        int exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt32();
        exStyle |= NativeMethods.WS_EX_NOACTIVATE;
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));

        // Hook window message loop for WM_MOUSEACTIVATE
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(WndProc);

        // Ensure Windows Startup registration if requested
        if (_viewModel.Settings.StartWithWindows)
        {
            _sysUtils.SetStartupRun(true);
        }

        // Start physical key hook for dynamic hold-to-replace shortcuts
        _physicalKeyHook.LoadRules(_viewModel.Settings.CustomMappingRules, _viewModel.Settings.LongPressThresholdMs);
        _physicalKeyHook.Start();

        ApplyLayoutModeDimensions();
        ValidateWindowBounds();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_MOUSEACTIVATE)
        {
            // Return MA_NOACTIVATE: window does not become the active window on click!
            handled = true;
            return new IntPtr(NativeMethods.MA_NOACTIVATE);
        }

        return IntPtr.Zero;
    }

    private void ValidateWindowBounds()
    {
        // Ensure the window is within visible screen bounds
        double screenWidth = SystemParameters.VirtualScreenWidth;
        double screenHeight = SystemParameters.VirtualScreenHeight;

        if (Left < 0 || Left > screenWidth - 100)
        {
            Left = 50;
        }
        if (Top < 0 || Top > screenHeight - 100)
        {
            Top = screenHeight - Height - 80;
        }
    }

    private void OnForegroundPollTick(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        IntPtr currentForeground = NativeMethods.GetForegroundWindow();
        if (currentForeground != IntPtr.Zero && currentForeground != helper.Handle)
        {
            _viewModel.TargetManager.RegisterPossibleTarget(currentForeground, helper.Handle);
        }
    }

    private void BuildMainKeyboardGrid()
    {
        MainKeyboardGrid.RowDefinitions.Clear();
        MainKeyboardGrid.Children.Clear();

        var rows = _viewModel.MainRows;
        for (int r = 0; r < rows.Count; r++)
        {
            MainKeyboardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var rowGrid = new Grid();
            var keyList = rows[r];

            for (int c = 0; c < keyList.Count; c++)
            {
                var key = keyList[c];
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(key.WidthUnits, GridUnitType.Star)
                });

                var btn = CreateKeyControl(key);
                Grid.SetColumn(btn, c);
                rowGrid.Children.Add(btn);
            }

            Grid.SetRow(rowGrid, r);
            MainKeyboardGrid.Children.Add(rowGrid);
        }
    }

    private void BuildNavClusterGrid()
    {
        NavClusterGrid.RowDefinitions.Clear();
        NavClusterGrid.ColumnDefinitions.Clear();
        NavClusterGrid.Children.Clear();

        var rows = _viewModel.NavigationRows;
        for (int r = 0; r < rows.Count; r++)
        {
            NavClusterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        for (int c = 0; c < 3; c++)
        {
            NavClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < rows[r].Count; c++)
            {
                var key = rows[r][c];
                var btn = CreateKeyControl(key);
                Grid.SetRow(btn, r);
                Grid.SetColumn(btn, c);
                NavClusterGrid.Children.Add(btn);
            }
        }
    }

    private void BuildNumpadGrid()
    {
        NumpadGrid.RowDefinitions.Clear();
        NumpadGrid.ColumnDefinitions.Clear();
        NumpadGrid.Children.Clear();

        var rows = _viewModel.NumpadRows;
        for (int r = 0; r < rows.Count; r++)
        {
            NumpadGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        for (int c = 0; c < 4; c++)
        {
            NumpadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int r = 0; r < rows.Count; r++)
        {
            int colIndex = 0;
            for (int c = 0; c < rows[r].Count; c++)
            {
                var key = rows[r][c];
                var btn = CreateKeyControl(key);
                Grid.SetRow(btn, r);
                Grid.SetColumn(btn, colIndex);

                int colSpan = (int)Math.Round(key.WidthUnits);
                if (colSpan > 1)
                {
                    Grid.SetColumnSpan(btn, colSpan);
                }
                colIndex += colSpan;

                NumpadGrid.Children.Add(btn);
            }
        }
    }

    private ContentControl CreateKeyControl(KeyModel key)
    {
        var template = (DataTemplate)FindResource("KeyButtonTemplate");
        var control = new ContentControl
        {
            Content = key,
            ContentTemplate = template,
            Focusable = false,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        return control;
    }

    private void OnKeyButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is KeyModel key)
        {
            _viewModel.ExecuteKeyMouseDown(key);
        }
    }

    private void OnKeyButtonPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is KeyModel key)
        {
            _viewModel.ExecuteKeyMouseUp(key);
        }
    }

    private void ToggleSpecialKeysWindow()
    {
        if (_specialKeysWindow.IsVisible)
        {
            _specialKeysWindow.Hide();
        }
        else
        {
            // Position it aligned to the same left and width as the 3-key drag bar
            _specialKeysWindow.Left = Left;
            _specialKeysWindow.Width = Width;

            // Height is comfortably calculated or defaults to 140
            double desiredHeight = Math.Max(120, Height * 1.3);
            _specialKeysWindow.Height = desiredHeight;

            if (Top >= desiredHeight + 10)
            {
                _specialKeysWindow.Top = Top - desiredHeight - 6;
            }
            else
            {
                _specialKeysWindow.Top = Top + Height + 6;
            }
            _specialKeysWindow.Show();
        }
    }

    private bool _explicitCloseRequested = false;

    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
            _viewModel.Settings.WindowLeft = Left;
            _viewModel.Settings.WindowTop = Top;
            _viewModel.Settings.WindowWidth = Width;
            _viewModel.Settings.WindowHeight = Height;
            _viewModel.Settings.Save();
        }
    }

    private void OnToggleTopmostClick(object sender, RoutedEventArgs e)
    {
        _viewModel.Settings.AlwaysOnTop = !_viewModel.Settings.AlwaysOnTop;
        _viewModel.Settings.Save();
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        _explicitCloseRequested = true;
        Close();
    }

    private void OnStartupCheckboxClick(object sender, RoutedEventArgs e)
    {
        _sysUtils.SetStartupRun(_viewModel.Settings.StartWithWindows);
        _viewModel.Settings.Save();
    }

    private void BuildQuickBarGrid()
    {
        QuickBarGrid.RowDefinitions.Clear();
        QuickBarGrid.ColumnDefinitions.Clear();
        QuickBarGrid.Children.Clear();

        var keys = _viewModel.Settings.QuickBarKeys;
        if (keys == null || keys.Count == 0)
        {
            keys = new List<string> { "P", "4", "Backspace" };
        }

        // Dynamically adjust window width if more than 3 keys
        if (_viewModel.Settings.MiniTrioOnly)
        {
            double neededWidth = Math.Max(280, keys.Count * 95);
            if (Width != neededWidth)
            {
                Width = neededWidth;
            }
        }

        for (int i = 0; i < keys.Count; i++)
        {
            string keyName = keys[i];
            bool isWide = string.Equals(keyName, "Backspace", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(keyName, "Enter", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(keyName, "Space", StringComparison.OrdinalIgnoreCase);

            QuickBarGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(isWide ? 1.35 : 1.0, GridUnitType.Star)
            });

            var btn = CreateQuickBarButton(keyName);
            Grid.SetColumn(btn, i);
            QuickBarGrid.Children.Add(btn);
        }
    }

    private Button CreateQuickBarButton(string keyName)
    {
        var btn = new Button
        {
            Style = (Style)FindResource("VirtualKeyButtonStyle"),
            Margin = new Thickness(3),
            Cursor = Cursors.Hand,
            Focusable = false
        };

        var keyModel = ResolveKeyModel(keyName);
        btn.Command = _viewModel.KeyClickCommand;
        btn.CommandParameter = keyModel;

        if (keyModel.CanRepeat)
        {
            btn.PreviewMouseDown += OnKeyButtonPreviewMouseDown;
            btn.PreviewMouseUp += OnKeyButtonPreviewMouseUp;
        }

        var viewbox = new Viewbox
        {
            MaxHeight = 72,
            MaxWidth = 110,
            Stretch = System.Windows.Media.Stretch.Uniform
        };

        var tb = new TextBlock
        {
            Text = GetDisplaySymbol(keyName),
            FontWeight = FontWeights.Bold,
            Foreground = GetKeyColor(keyName)
        };

        viewbox.Child = tb;
        btn.Content = viewbox;
        return btn;
    }

    private static string GetDisplaySymbol(string keyName)
    {
        return keyName.ToUpperInvariant() switch
        {
            "BACKSPACE" or "BS" => "⌫",
            "ENTER" or "RETURN" => "↵",
            "SPACE" => "␣",
            "TAB" => "⇥",
            "DELETE" or "DEL" => "Del",
            "ESC" or "ESCAPE" => "Esc",
            "UP" => "▲",
            "DOWN" => "▼",
            "LEFT" => "◀",
            "RIGHT" => "▶",
            _ => keyName
        };
    }

    private static System.Windows.Media.Brush GetKeyColor(string keyName)
    {
        return keyName.ToUpperInvariant() switch
        {
            "BACKSPACE" or "BS" or "DEL" or "DELETE" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x8A, 0x65)),
            "ENTER" or "RETURN" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x69, 0xF0, 0xAE)),
            "SPACE" or "TAB" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xD5, 0x4F)),
            "ESC" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x53, 0x50)),
            _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x64, 0xB5, 0xF6))
        };
    }

    private static KeyModel ResolveKeyModel(string keyName)
    {
        string norm = keyName.Trim().ToUpperInvariant();
        return norm switch
        {
            "BACKSPACE" or "BS" => KeyModel.Create("BACKSPACE", "Backspace", VirtualKeys.VK_BACK, canRepeat: true),
            "ENTER" or "RETURN" => KeyModel.Create("ENTER", "Enter", VirtualKeys.VK_RETURN),
            "SPACE" => KeyModel.Create("SPACE", " ", VirtualKeys.VK_SPACE),
            "TAB" => KeyModel.Create("TAB", "Tab", VirtualKeys.VK_TAB),
            "DELETE" or "DEL" => KeyModel.Create("DELETE", "Delete", VirtualKeys.VK_DELETE, canRepeat: true),
            "ESC" or "ESCAPE" => KeyModel.Create("ESC", "Esc", VirtualKeys.VK_ESCAPE),
            "UP" => KeyModel.Create("UP", "Up", VirtualKeys.VK_UP, canRepeat: true),
            "DOWN" => KeyModel.Create("DOWN", "Down", VirtualKeys.VK_DOWN, canRepeat: true),
            "LEFT" => KeyModel.Create("LEFT", "Left", VirtualKeys.VK_LEFT, canRepeat: true),
            "RIGHT" => KeyModel.Create("RIGHT", "Right", VirtualKeys.VK_RIGHT, canRepeat: true),
            _ => norm.Length == 1 && char.IsLetter(norm[0])
                ? KeyModel.Create(norm, norm.ToLowerInvariant(), (ushort)norm[0])
                : norm.Length == 1 && char.IsDigit(norm[0])
                    ? KeyModel.Create(norm, norm, (ushort)(0x30 + (norm[0] - '0')))
                    : KeyModel.Create(keyName, keyName, 0, specialAction: $"Unicode:{keyName}")
        };
    }

    private void OnOpenSettingsWindowClick(object sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow(_viewModel.Settings, OnSettingsApplied);
        win.Owner = this;
        win.ShowDialog();
    }

    public void ApplyLayoutModeDimensions()
    {
        if (_viewModel.Settings.MiniTrioOnly)
        {
            int keyCount = _viewModel.Settings.QuickBarKeys?.Count ?? 3;
            double desiredWidth = Math.Max(280, keyCount * 78);
            Width = desiredWidth;
            Height = 95;
            MinWidth = 200;
            MinHeight = 85;
            ResizeMode = ResizeMode.NoResize;
            _viewModel.Settings.WindowWidth = Width;
            _viewModel.Settings.WindowHeight = Height;
            BuildQuickBarGrid();
        }
        else
        {
            if (Width < 700 || Height < 260)
            {
                Width = 980;
                Height = 320;
            }
            MinWidth = 650;
            MinHeight = 240;
            ResizeMode = ResizeMode.CanResize;
            _viewModel.Settings.WindowWidth = Width;
            _viewModel.Settings.WindowHeight = Height;
        }

        ValidateWindowBounds();
    }

    private void OnSettingsApplied(AppSettings newSettings)
    {
        Topmost = newSettings.AlwaysOnTop;
        Opacity = newSettings.WindowOpacity;
        _sysUtils.SetStartupRun(newSettings.StartWithWindows);

        _physicalKeyHook.LoadRules(newSettings.CustomMappingRules, newSettings.LongPressThresholdMs);

        ApplyLayoutModeDimensions();
    }

    private void OnMainWindowClosing(object? sender, CancelEventArgs e)
    {
        // Never close unless the user explicitly clicked the close button ('✕')
        if (!_explicitCloseRequested)
        {
            e.Cancel = true;
            WindowState = WindowState.Normal;
            Show();
            Activate();
            return;
        }

        _foregroundPollTimer.Stop();
        _physicalKeyHook.Dispose();
        _specialKeysWindow.Close();
        _hwndSource?.RemoveHook(WndProc);
        _viewModel.Cleanup();
    }
}