// duhBuhUI - self-contained settings UI framework for Streamer.bot C# actions.
//
// This is intentionally source-first: Streamer.bot C# actions can paste/include this
// class without requiring a separate DLL. The persistence layer uses Streamer.bot's
// persisted global variables, so settings survive restarts.
//
// Supported controls in v0.1:
//   AddHeader, AddTitle, AddToggleSwitch, AddSlider, AddTextbox,
//   AddClickableButton, LogExistingSettings, ShowUI, AddPopupWindow.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public sealed class DuhBuhUI
{
    // Delegates keep this framework independent of Streamer.bot's concrete CPH type
    // and, importantly, avoid requiring the C# compiler's dynamic binder.
    private readonly Func<string, bool, bool?> _getBool;
    private readonly Func<string, bool, int?> _getInt;
    private readonly Func<string, bool, string> _getString;
    private readonly Func<string, bool, object> _getObject;
    private readonly Action<string, object, bool> _setGlobal;
    private readonly Action<string> _logInfo;

    private readonly string _extensionName;
    private readonly string _extensionVersion;
    private readonly Dictionary<string, object> _defaults = new Dictionary<string, object>();
    private readonly Dictionary<string, string> _labels = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _descriptions = new Dictionary<string, string>();
    private readonly List<TabDefinition> _tabs = new List<TabDefinition>();

    private sealed class TabDefinition
    {
        public string Name;
        public StackPanel Panel;
    }

    public DuhBuhUI(
        string extensionName,
        string extensionVersion,
        Func<string, bool, bool?> getBool,
        Func<string, bool, int?> getInt,
        Func<string, bool, string> getString,
        Func<string, bool, object> getObject,
        Action<string, object, bool> setGlobal,
        Action<string> logInfo)
    {
        _extensionName = extensionName ?? "duhBuh";
        _extensionVersion = extensionVersion ?? "0.1.0";
        _getBool = getBool;
        _getInt = getInt;
        _getString = getString;
        _getObject = getObject;
        _setGlobal = setGlobal;
        _logInfo = logInfo;
    }

    public void AddHeader(string imageUrl)
    {
        // Reserved for the visual header implementation in the next UI pass.
    }

    private StackPanel GetPanel(string category)
    {
        for (int i = 0; i < _tabs.Count; i++)
            if (_tabs[i].Name == category) return _tabs[i].Panel;

        var panel = new StackPanel { Margin = new Thickness(18) };
        _tabs.Add(new TabDefinition { Name = category, Panel = panel });
        return panel;
    }

    public void AddTitle(string title, string category)
    {
        GetPanel(category).Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 12)
        });
    }

    public void AddToggleSwitch(string title, string description, string category, string variableName, bool defaultValue)
    {
        Register(variableName, title, description, defaultValue);
        var panel = GetPanel(category);
        var existing = Read(variableName, defaultValue);
        var check = new CheckBox
        {
            Content = title,
            IsChecked = existing,
            FontSize = 14,
            Margin = new Thickness(0, 5, 0, 2),
            Tag = variableName
        };
        var box = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        box.Children.Add(check);
        box.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = .72,
            Margin = new Thickness(24, 0, 0, 0)
        });
        panel.Children.Add(box);
    }

    public void AddSlider(string title, string description, string category, string variableName, int minimum, int maximum, int defaultValue)
    {
        Register(variableName, title, description, defaultValue);
        var panel = GetPanel(category);
        var value = Read(variableName, defaultValue);
        var box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
        var label = new TextBlock { Text = title + ": " + value, FontSize = 14 };
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Max(minimum, Math.Min(maximum, value)),
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            Tag = variableName
        };
        slider.ValueChanged += (s, e) => label.Text = title + ": " + ((int)Math.Round(e.NewValue));
        box.Children.Add(label);
        box.Children.Add(slider);
        box.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = .72,
            Margin = new Thickness(0, 3, 0, 0)
        });
        panel.Children.Add(box);
    }

    public void AddTextbox(string title, string description, string category, string variableName, string defaultValue, bool multiline)
    {
        Register(variableName, title, description, defaultValue);
        var panel = GetPanel(category);
        var box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
        box.Children.Add(new TextBlock { Text = title, FontSize = 14 });
        var text = new TextBox
        {
            Text = Read(variableName, defaultValue),
            AcceptsReturn = multiline,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = multiline ? 65 : 30,
            Tag = variableName
        };
        box.Children.Add(text);
        box.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = .72,
            Margin = new Thickness(0, 3, 0, 0)
        });
        panel.Children.Add(box);
    }

    public void AddClickableButton(string title, string description, string buttonText, string color, string category, Action callback)
    {
        var panel = GetPanel(category);
        var box = new StackPanel { Margin = new Thickness(0, 5, 0, 14) };
        var button = new Button
        {
            Content = buttonText,
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.Click += (s, e) => { if (callback != null) callback(); };
        box.Children.Add(new TextBlock { Text = title, FontSize = 14 });
        box.Children.Add(button);
        box.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = .72,
            Margin = new Thickness(0, 3, 0, 0)
        });
        panel.Children.Add(box);
    }

    public void LogExistingSettings()
    {
        for (int i = 0; i < _defaults.Count; i++)
        {
            foreach (var item in _defaults)
            {
                object value = ReadObject(item.Key, item.Value);
                _logInfo("[duhBuhUI] " + item.Key + " = " + Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            break;
        }
    }

    public void ShowUI()
    {
        var window = new Window
        {
            Title = _extensionName + " - Settings",
            Width = 720,
            Height = 760,
            MinWidth = 560,
            MinHeight = 500,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        var root = new DockPanel();
        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12)
        };
        var save = new Button
        {
            Content = "Save",
            Padding = new Thickness(18, 7, 18, 7),
            Margin = new Thickness(0, 0, 8, 0)
        };
        var close = new Button
        {
            Content = "Save & Exit",
            Padding = new Thickness(18, 7, 18, 7)
        };
        save.Click += (s, e) => Save(root);
        close.Click += (s, e) => { Save(root); window.Close(); };
        footer.Children.Add(save);
        footer.Children.Add(close);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var tabs = new TabControl { Margin = new Thickness(8) };
        for (int i = 0; i < _tabs.Count; i++)
        {
            TabDefinition item = _tabs[i];
            tabs.Items.Add(new TabItem
            {
                Header = item.Name,
                Content = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = item.Panel
                }
            });
        }
        root.Children.Add(tabs);
        window.Content = root;
        window.ShowDialog();
    }

    public void AddPopupWindow(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Register(string key, string label, string description, object value)
    {
        _defaults[key] = value;
        _labels[key] = label;
        _descriptions[key] = description;
    }

    private object ReadObject(string key, object fallback)
    {
        try
        {
            object value = _getObject(key, true);
            return value ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private bool Read(string key, bool fallback)
    {
        try
        {
            bool? value = _getBool(key, true);
            return value.HasValue ? value.Value : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private int Read(string key, int fallback)
    {
        try
        {
            int? value = _getInt(key, true);
            return value.HasValue ? value.Value : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private string Read(string key, string fallback)
    {
        try
        {
            string value = _getString(key, true);
            return value ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private void Save(DockPanel root)
    {
        foreach (var item in _defaults)
            SaveTagged(root, item.Key);

        _logInfo("[duhBuhUI] Saved " + _defaults.Count + " settings for " + _extensionName + " v" + _extensionVersion + ".");
    }

    private void SaveTagged(DependencyObject parent, string key)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(parent))
        {
            var dep = child as DependencyObject;
            if (dep == null) continue;

            var fe = dep as FrameworkElement;
            if (fe != null && Equals(fe.Tag, key))
            {
                var cb = fe as CheckBox;
                if (cb != null)
                    _setGlobal(key, cb.IsChecked == true, true);

                var sl = fe as Slider;
                if (sl != null)
                    _setGlobal(key, (int)Math.Round(sl.Value), true);

                var tb = fe as TextBox;
                if (tb != null)
                    _setGlobal(key, tb.Text, true);
            }

            SaveTagged(dep, key);
        }
    }
}
