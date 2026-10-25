using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyBuddy.Models;

namespace KeyBuddy;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _onSaved;

    private readonly List<string> _workingKeys;
    private readonly List<KeyMappingRule> _workingRules;

    public SettingsWindow(AppSettings settings, Action<AppSettings> onSaved)
    {
        InitializeComponent();

        _settings = settings;
        _onSaved = onSaved;

        _workingKeys = new List<string>(settings.QuickBarKeys);
        _workingRules = settings.CustomMappingRules.Select(r => new KeyMappingRule
        {
            TriggerKey = r.TriggerKey,
            OutputKey = r.OutputKey,
            IsEnabled = r.IsEnabled,
            Description = r.Description
        }).ToList();

        LoadSettingsToUI();
    }

    private void LoadSettingsToUI()
    {
        // 1. Rescue Bar Keys
        RefreshKeysBadgeList();

        // 2. Rules
        RefreshRulesList();
        ThresholdSlider.Value = _settings.LongPressThresholdMs;
        ThresholdDisplay.Text = $"{_settings.LongPressThresholdMs} ms";

        // 3. General
        RadioRescueBar.IsChecked = _settings.MiniTrioOnly;
        RadioFullKeyboard.IsChecked = !_settings.MiniTrioOnly;

        CheckAlwaysOnTop.IsChecked = _settings.AlwaysOnTop;
        CheckStartup.IsChecked = _settings.StartWithWindows;

        OpacitySlider.Value = _settings.WindowOpacity;
        OpacityDisplay.Text = $"{Math.Round(_settings.WindowOpacity * 100)}%";
    }

    private void RefreshKeysBadgeList()
    {
        QuickKeysWrapPanel.Children.Clear();
        EmptyKeysWarning.Visibility = _workingKeys.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var key in _workingKeys)
        {
            var badge = CreateKeyBadge(key);
            QuickKeysWrapPanel.Children.Add(badge);
        }
    }

    private UIElement CreateKeyBadge(string keyName)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2D, 0x3D)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x4F, 0x53, 0x6E)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(8, 4, 6, 4),
            Margin = new Thickness(0, 0, 8, 8)
        };

        var stack = new StackPanel { Orientation = Orientation.Horizontal };

        var label = new TextBlock
        {
            Text = keyName,
            FontWeight = FontWeights.Bold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0xB5, 0xF6)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        stack.Children.Add(label);

        var removeBtn = new Button
        {
            Content = "✕",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x8A, 0x80)),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Padding = new Thickness(2, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Tag = keyName
        };
        removeBtn.Click += (s, e) =>
        {
            if (s is Button b && b.Tag is string k)
            {
                _workingKeys.Remove(k);
                RefreshKeysBadgeList();
            }
        };
        stack.Children.Add(removeBtn);

        border.Child = stack;
        return border;
    }

    private void OnAddKeyClick(object sender, RoutedEventArgs e)
    {
        string text = NewKeyInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        AddKeyToList(text);
        NewKeyInput.Text = string.Empty;
    }

    private void OnPresetKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string keyName)
        {
            AddKeyToList(keyName);
        }
    }

    private void AddKeyToList(string keyName)
    {
        if (!_workingKeys.Any(k => string.Equals(k, keyName, StringComparison.OrdinalIgnoreCase)))
        {
            _workingKeys.Add(keyName);
            RefreshKeysBadgeList();
        }
    }

    private void OnResetKeysClick(object sender, RoutedEventArgs e)
    {
        _workingKeys.Clear();
        _workingKeys.Add("P");
        _workingKeys.Add("4");
        _workingKeys.Add("Backspace");
        RefreshKeysBadgeList();
    }

    private void RefreshRulesList()
    {
        RulesListContainer.Children.Clear();
        EmptyRulesNotice.Visibility = _workingRules.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var rule in _workingRules)
        {
            var row = CreateRuleRow(rule);
            RulesListContainer.Children.Add(row);
        }
    }

    private UIElement CreateRuleRow(KeyMappingRule rule)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x24, 0x27, 0x33)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x3E, 0x52)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 0, 0, 6)
        };

        var dock = new DockPanel();

        var chk = new CheckBox
        {
            IsChecked = rule.IsEnabled,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        chk.Checked += (s, e) => rule.IsEnabled = true;
        chk.Unchecked += (s, e) => rule.IsEnabled = false;
        dock.Children.Add(chk);

        var delBtn = new Button
        {
            Content = "🗑️",
            FontSize = 12,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(4, 2, 4, 2),
            ToolTip = "Delete this shortcut",
            Tag = rule
        };
        delBtn.Click += (s, e) =>
        {
            if (s is Button b && b.Tag is KeyMappingRule r)
            {
                _workingRules.Remove(r);
                RefreshRulesList();
            }
        };
        DockPanel.SetDock(delBtn, Dock.Right);
        dock.Children.Add(delBtn);

        var textStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        textStack.Children.Add(new TextBlock
        {
            Text = "Hold ",
            Foreground = new SolidColorBrush(Color.FromRgb(0xCF, 0xD8, 0xDC)),
            FontSize = 12
        });
        textStack.Children.Add(new TextBlock
        {
            Text = $"'{rule.TriggerKey}'",
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0xC7, 0x84)),
            FontSize = 12
        });
        textStack.Children.Add(new TextBlock
        {
            Text = " ➔ Types ",
            Foreground = new SolidColorBrush(Color.FromRgb(0xCF, 0xD8, 0xDC)),
            FontSize = 12
        });
        textStack.Children.Add(new TextBlock
        {
            Text = $"'{GetRuleDisplayOutput(rule.OutputKey)}'",
            FontWeight = FontWeights.Bold,
            Foreground = GetRuleOutputBrush(rule.OutputKey),
            FontSize = 12
        });

        dock.Children.Add(textStack);
        border.Child = dock;
        return border;
    }

    private static string GetRuleDisplayOutput(string output)
    {
        return output.ToUpperInvariant() switch
        {
            "BACKSPACE" or "BS" or "⌫" => "⌫ Backspace",
            "ENTER" or "RETURN" or "↵" => "↵ Enter",
            "TAB" or "⇥" => "⇥ Tab",
            "SPACE" or "␣" => "␣ Space",
            "DELETE" or "DEL" => "Del Delete",
            "ESC" or "ESCAPE" => "Esc",
            _ => output
        };
    }

    private static SolidColorBrush GetRuleOutputBrush(string output)
    {
        return output.ToUpperInvariant() switch
        {
            "BACKSPACE" or "BS" or "⌫" => new SolidColorBrush(Color.FromRgb(0xFF, 0xAB, 0x91)),
            "ENTER" or "RETURN" or "↵" => new SolidColorBrush(Color.FromRgb(0xA5, 0xD6, 0xA7)),
            "TAB" or "⇥" => new SolidColorBrush(Color.FromRgb(0x90, 0xCA, 0xF9)),
            "SPACE" or "␣" => new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0x82)),
            "DELETE" or "DEL" => new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x80)),
            _ => new SolidColorBrush(Color.FromRgb(0x64, 0xB5, 0xF6))
        };
    }

    private void OnRulePresetOutputClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string text)
        {
            string key = text switch
            {
                "⌫ Backspace" => "Backspace",
                "↵ Enter" => "Enter",
                "⇥ Tab" => "Tab",
                "␣ Space" => "Space",
                "Del Delete" => "Delete",
                "Esc" => "Esc",
                _ => text
            };
            RuleOutputInput.Text = key;
        }
    }

    private void OnAddRuleClick(object sender, RoutedEventArgs e)
    {
        string trigger = RuleTriggerInput.Text.Trim();
        string output = RuleOutputInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(output))
        {
            MessageBox.Show("Please enter both the physical key to hold and the character to type.",
                "Incomplete Rule", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var newRule = new KeyMappingRule(trigger, output);
        _workingRules.Add(newRule);
        RefreshRulesList();

        RuleTriggerInput.Text = string.Empty;
        RuleOutputInput.Text = string.Empty;
    }

    private void OnThresholdSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ThresholdDisplay != null)
        {
            ThresholdDisplay.Text = $"{Math.Round(e.NewValue)} ms";
        }
    }

    private void OnOpacitySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityDisplay != null)
        {
            OpacityDisplay.Text = $"{Math.Round(e.NewValue * 100)}%";
        }
    }

    private void OnSaveApplyClick(object sender, RoutedEventArgs e)
    {
        if (_workingKeys.Count == 0)
        {
            _workingKeys.Add("P");
            _workingKeys.Add("4");
            _workingKeys.Add("Backspace");
        }

        _settings.QuickBarKeys = new List<string>(_workingKeys);
        _settings.CustomMappingRules = new List<KeyMappingRule>(_workingRules);
        _settings.LongPressThresholdMs = (int)Math.Round(ThresholdSlider.Value);

        _settings.MiniTrioOnly = RadioRescueBar.IsChecked == true;
        _settings.AlwaysOnTop = CheckAlwaysOnTop.IsChecked == true;
        _settings.StartWithWindows = CheckStartup.IsChecked == true;
        _settings.WindowOpacity = OpacitySlider.Value;

        _settings.Save();
        _onSaved.Invoke(_settings);

        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
