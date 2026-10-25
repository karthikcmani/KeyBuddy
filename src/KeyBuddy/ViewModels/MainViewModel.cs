using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using KeyBuddy.Models;
using KeyBuddy.Services;

namespace KeyBuddy.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IKeyboardInputService _inputService;
    private readonly KeyboardLayoutService _layoutService;
    private readonly TargetWindowManager _targetManager;
    private readonly SystemUtilityService _sysUtils;
    private readonly DispatcherTimer _repeatTimer;
    private KeyModel? _repeatingKey;

    public ModifierState Modifiers { get; }
    public AppSettings Settings { get; }

    public List<List<KeyModel>> FunctionRows { get; }
    public List<List<KeyModel>> MainRows { get; }
    public List<List<KeyModel>> NavigationRows { get; }
    public List<List<KeyModel>> NumpadRows { get; }

    public TargetWindowManager TargetManager => _targetManager;

    private string _unicodeTextInput = string.Empty;
    public string UnicodeTextInput
    {
        get => _unicodeTextInput;
        set { _unicodeTextInput = value; OnPropertyChanged(); }
    }

    private bool _isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set { _isSettingsOpen = value; OnPropertyChanged(); }
    }

    private bool _isFnMenuOpen;
    public bool IsFnMenuOpen
    {
        get => _isFnMenuOpen;
        set { _isFnMenuOpen = value; OnPropertyChanged(); }
    }

    private bool _isFirstRunGuideOpen;
    public bool IsFirstRunGuideOpen
    {
        get => _isFirstRunGuideOpen;
        set { _isFirstRunGuideOpen = value; OnPropertyChanged(); }
    }

    public KeyModel QuickPKey { get; } = KeyModel.Create("P_QUICK", "P", VirtualKeys.VK_P, secondary: "P");
    public KeyModel QuickBackspaceKey { get; } = KeyModel.Create("BACKSPACE_QUICK", "⌫ Backspace", VirtualKeys.VK_BACK);
    public KeyModel Quick4Key { get; } = KeyModel.Create("4_QUICK", "4", VirtualKeys.VK_4, secondary: "$");

    public ICommand KeyClickCommand { get; }
    public ICommand KeyMouseDownCommand { get; }
    public ICommand KeyMouseUpCommand { get; }
    public ICommand ResetModifiersCommand { get; }
    public ICommand ToggleSettingsCommand { get; }
    public ICommand ToggleFnMenuCommand { get; }
    public ICommand ToggleFirstRunGuideCommand { get; }
    public ICommand SendUnicodeCommand { get; }
    public ICommand OpenOskCommand { get; }
    public ICommand OpenTaskMgrCommand { get; }
    public ICommand OpenExplorerCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand FnActionCommand { get; }
    public ICommand ToggleCompactModeCommand { get; }
    public ICommand ToggleMiniTrioCommand { get; }
    public ICommand ToggleSpecialSymbolsCommand { get; }

    public event Action? OnRequestToggleSpecialSymbols;

    public MainViewModel(
        IKeyboardInputService inputService,
        KeyboardLayoutService layoutService,
        TargetWindowManager targetManager,
        SystemUtilityService sysUtils)
    {
        _inputService = inputService;
        _layoutService = layoutService;
        _targetManager = targetManager;
        _sysUtils = sysUtils;

        Modifiers = new ModifierState();
        Settings = AppSettings.Load();

        FunctionRows = _layoutService.GetFunctionRow();
        MainRows = _layoutService.GetMainRows();
        NavigationRows = _layoutService.GetNavigationCluster();
        NumpadRows = _layoutService.GetNumpadRows();

        _repeatTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(60) // smooth repeat interval
        };
        _repeatTimer.Tick += OnRepeatTimerTick;

        KeyClickCommand = new RelayCommand(param => ExecuteKeyClick(param as KeyModel));
        KeyMouseDownCommand = new RelayCommand(param => ExecuteKeyMouseDown(param as KeyModel));
        KeyMouseUpCommand = new RelayCommand(param => ExecuteKeyMouseUp(param as KeyModel));
        ResetModifiersCommand = new RelayCommand(() =>
        {
            Modifiers.ResetAll();
            _inputService.ReleaseAllModifiers();
        });

        ToggleSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
        ToggleFnMenuCommand = new RelayCommand(() => IsFnMenuOpen = !IsFnMenuOpen);
        ToggleFirstRunGuideCommand = new RelayCommand(() => IsFirstRunGuideOpen = !IsFirstRunGuideOpen);

        SendUnicodeCommand = new RelayCommand(() =>
        {
            if (!string.IsNullOrEmpty(UnicodeTextInput))
            {
                _inputService.SendUnicodeText(UnicodeTextInput);
                UnicodeTextInput = string.Empty;
            }
        });

        OpenOskCommand = new RelayCommand(_sysUtils.OpenWindowsOSK);
        OpenTaskMgrCommand = new RelayCommand(_sysUtils.OpenTaskManager);
        OpenExplorerCommand = new RelayCommand(_sysUtils.OpenFileExplorer);
        OpenSettingsCommand = new RelayCommand(_sysUtils.OpenWindowsSettings);

        FnActionCommand = new RelayCommand(param => ExecuteFnAction(param?.ToString()));
        ToggleCompactModeCommand = new RelayCommand(() =>
        {
            Settings.CompactMode = !Settings.CompactMode;
            OnPropertyChanged(nameof(Settings));
            Settings.Save();
        });
        ToggleMiniTrioCommand = new RelayCommand(() =>
        {
            Settings.MiniTrioOnly = !Settings.MiniTrioOnly;
            OnPropertyChanged(nameof(Settings));
            Settings.Save();
        });
        ToggleSpecialSymbolsCommand = new RelayCommand(() =>
        {
            OnRequestToggleSpecialSymbols?.Invoke();
        });

        // Initialize state
        _inputService.SynchronizeLockStates(Modifiers);
    }

    public void ExecuteKeyClick(KeyModel? key)
    {
        if (key == null) return;

        if (key.SpecialAction == "FnQuickActions")
        {
            IsFnMenuOpen = !IsFnMenuOpen;
            return;
        }

        if (key.SpecialAction == "None") return;

        switch (key.Id)
        {
            case "LSHIFT":
            case "RSHIFT":
                Modifiers.ToggleShift();
                return;

            case "LCTRL":
            case "RCTRL":
                Modifiers.Ctrl = !Modifiers.Ctrl;
                return;

            case "LALT":
            case "RALT":
                Modifiers.Alt = !Modifiers.Alt;
                return;

            case "LWIN":
                Modifiers.WindowsKey = !Modifiers.WindowsKey;
                return;

            case "CAPS":
                _inputService.SendKeyPress(key, Modifiers);
                return;

            case "NUM_LOCK":
                _inputService.SendKeyPress(key, Modifiers);
                return;

            default:
                _inputService.SendKeyPress(key, Modifiers);
                break;
        }
    }

    public void ExecuteKeyMouseDown(KeyModel? key)
    {
        if (key == null || !key.CanRepeat) return;

        // Auto-repeat for Backspace, Delete, Arrow keys
        if (key.VirtualKey == VirtualKeys.VK_BACK ||
            key.VirtualKey == VirtualKeys.VK_DELETE ||
            key.VirtualKey == VirtualKeys.VK_LEFT ||
            key.VirtualKey == VirtualKeys.VK_RIGHT ||
            key.VirtualKey == VirtualKeys.VK_UP ||
            key.VirtualKey == VirtualKeys.VK_DOWN)
        {
            _repeatingKey = key;
            _repeatTimer.Interval = TimeSpan.FromMilliseconds(400); // initial delay before repeating
            _repeatTimer.Start();
        }
    }

    public void ExecuteKeyMouseUp(KeyModel? key)
    {
        if (_repeatingKey != null)
        {
            _repeatTimer.Stop();
            _repeatingKey = null;
        }
    }

    private void OnRepeatTimerTick(object? sender, EventArgs e)
    {
        _repeatTimer.Interval = TimeSpan.FromMilliseconds(60); // fast repeat rate
        if (_repeatingKey != null)
        {
            _inputService.SendKeyPress(_repeatingKey, Modifiers);
        }
    }

    private void ExecuteFnAction(string? action)
    {
        IsFnMenuOpen = false;
        switch (action)
        {
            case "Mute":
                _inputService.SendKeyPress(KeyModel.Create("VOL_MUTE", "", 0xAD, isExtended: true), Modifiers);
                break;
            case "VolDown":
                _inputService.SendKeyPress(KeyModel.Create("VOL_DOWN", "", 0xAE, isExtended: true), Modifiers);
                break;
            case "VolUp":
                _inputService.SendKeyPress(KeyModel.Create("VOL_UP", "", 0xAF, isExtended: true), Modifiers);
                break;
            case "PlayPause":
                _inputService.SendKeyPress(KeyModel.Create("MEDIA_PLAY", "", 0xB3, isExtended: true), Modifiers);
                break;
            case "PrevTrack":
                _inputService.SendKeyPress(KeyModel.Create("MEDIA_PREV", "", 0xB1, isExtended: true), Modifiers);
                break;
            case "NextTrack":
                _inputService.SendKeyPress(KeyModel.Create("MEDIA_NEXT", "", 0xB0, isExtended: true), Modifiers);
                break;
            case "Project":
                // Win+P
                Modifiers.WindowsKey = true;
                _inputService.SendKeyPress(KeyModel.Create("P", "P", VirtualKeys.VK_P), Modifiers);
                break;
            case "LockScreen":
                // Win+L
                Modifiers.WindowsKey = true;
                _inputService.SendKeyPress(KeyModel.Create("L", "L", VirtualKeys.VK_L), Modifiers);
                break;
            case "Settings":
                _sysUtils.OpenWindowsSettings();
                break;
            case "SoundSettings":
                _sysUtils.OpenVolumeSettings();
                break;
            case "TaskMgr":
                _sysUtils.OpenTaskManager();
                break;
        }
    }

    public void Cleanup()
    {
        _repeatTimer.Stop();
        _inputService.ReleaseAllModifiers();
        Settings.Save();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
