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
    private readonly dynamic _cph;
    private readonly string _extensionName;
    private readonly string _extensionVersion;
    private readonly Dictionary<string, object> _defaults = new Dictionary<string, object>();
    private readonly Dictionary<string, string> _labels = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _descriptions = new Dictionary<string, string>();
    private readonly List<TabDefinition> _tabs = new List<TabDefinition>();
    private string _currentCategory;
    private string _headerUrl;

    private sealed class TabDefinition
    {
        public string Name;
        public StackPanel Panel;
        public TabItem Tab;
    }

    public DuhBuhUI(dynamic cph, string extensionName, string extensionVersion)
    {
        _cph = cph;
        _extensionName = extensionName ?? "duhBuh";
        _extensionVersion = extensionVersion ?? "0.1.0";
    }

    public void AddHeader(string imageUrl) => _headerUrl = imageUrl;

    private StackPanel GetPanel(string category)
    {
        foreach (var tab in _tabs)
            if (tab.Name == category) return tab.Panel;

        var panel = new StackPanel { Margin = new Thickness(18) };
        _tabs.Add(new TabDefinition { Name = category, Panel = panel });
        return panel;
    }

    public void AddTitle(string title, string category)
    {
        _currentCategory = category;
        GetPanel(category).Children.Add(new TextBlock {
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
        var check = new CheckBox {
            Content = title,
            IsChecked = Read(variableName, defaultValue),
            FontSize = 14,
            Margin = new Thickness(0, 5, 0, 2)
        };
        var box = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        box.Children.Add(check);
        box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(24, 0, 0, 0) });
        panel.Children.Add(box);
        check.Tag = variableName;
    }

    public void AddSlider(string title, string description, string category, string variableName, int minimum, int maximum, int defaultValue)
    {
        Register(variableName, title, description, defaultValue);
        var panel = GetPanel(category);
        var value = Read(variableName, defaultValue);
        var box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
        var label = new TextBlock { Text = title + ": " + value, FontSize = 14 };
        var slider = new Slider { Minimum = minimum, Maximum = maximum, Value = Math.Max(minimum, Math.Min(maximum, value)), TickFrequency = 1, IsSnapToTickEnabled = true };
        slider.ValueChanged += (s, e) => label.Text = title + ": " + ((int)Math.Round(e.NewValue));
        box.Children.Add(label);
        box.Children.Add(slider);
        box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
        panel.Children.Add(box);
        slider.Tag = variableName;
    }

    public void AddTextbox(string title, string description, string category, string variableName, string defaultValue, bool multiline)
    {
        Register(variableName, title, description, defaultValue);
        var panel = GetPanel(category);
        var box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
        box.Children.Add(new TextBlock { Text = title, FontSize = 14 });
        var text = new TextBox { Text = Read(variableName, defaultValue), AcceptsReturn = multiline, TextWrapping = TextWrapping.Wrap, MinHeight = multiline ? 65 : 30 };
        text.Tag = variableName;
        box.Children.Add(text);
        box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
        panel.Children.Add(box);
    }

    public void AddClickableButton(string title, string description, string buttonText, string color, string category, Action callback)
    {
        var panel = GetPanel(category);
        var box = new StackPanel { Margin = new Thickness(0, 5, 0, 14) };
        var button = new Button { Content = buttonText, Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += (s, e) => callback?.Invoke();
        box.Children.Add(new TextBlock { Text = title, FontSize = 14 });
        box.Children.Add(button);
        box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
        panel.Children.Add(box);
    }

    public void LogExistingSettings()
    {
        foreach (var item in _defaults)
            _cph.LogInfo("[duhBuhUI] " + item.Key + " = " + Convert.ToString(ReadObject(item.Key, item.Value), CultureInfo.InvariantCulture));
    }

    public void ShowUI()
    {
        var window = new Window {
            Title = _extensionName + " - Settings",
            Width = 720,
            Height = 760,
            MinWidth = 560,
            MinHeight = 500,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        var root = new DockPanel();
        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(12) };
        var save = new Button { Content = "Save", Padding = new Thickness(18, 7, 18, 7), Margin = new Thickness(0, 0, 8, 0) };
        var close = new Button { Content = "Save & Exit", Padding = new Thickness(18, 7, 18, 7) };
        save.Click += (s, e) => Save(root);
        close.Click += (s, e) => { Save(root); window.Close(); };
        footer.Children.Add(save); footer.Children.Add(close);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var tabs = new TabControl { Margin = new Thickness(8) };
        foreach (var item in _tabs)
            tabs.Items.Add(new TabItem { Header = item.Name, Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = item.Panel } });
        root.Children.Add(tabs);
        window.Content = root;
        window.ShowDialog();
    }

    public void AddPopupWindow(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    private void Register(string key, string label, string description, object value)
    {
        _defaults[key] = value; _labels[key] = label; _descriptions[key] = description;
    }

    private object ReadObject(string key, object fallback)
    {
        try { return _cph.GetGlobalVar<object>(key, true) ?? fallback; }
        catch { return fallback; }
    }

    private bool Read(string key, bool fallback) { try { return _cph.GetGlobalVar<bool?>(key, true) ?? fallback; } catch { return fallback; } }
    private int Read(string key, int fallback) { try { return _cph.GetGlobalVar<int?>(key, true) ?? fallback; } catch { return fallback; } }
    private string Read(string key, string fallback) { try { return _cph.GetGlobalVar<string>(key, true) ?? fallback; } catch { return fallback; } }

    private void Save(DockPanel root)
    {
        foreach (var item in _defaults)
        {
            // Controls are located by their Tag, so settings remain keyed by the
            // extension's stable variable name rather than by UI position.
            SaveTagged(root, item.Key);
        }
        _cph.LogInfo("[duhBuhUI] Saved " + _defaults.Count + " settings for " + _extensionName + ".");
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
                if (fe is CheckBox cb) _cph.SetGlobalVar(key, cb.IsChecked == true, true);
                else if (fe is Slider sl) _cph.SetGlobalVar(key, (int)Math.Round(sl.Value), true);
                else if (fe is TextBox tb) _cph.SetGlobalVar(key, tb.Text, true);
            }
            SaveTagged(dep, key);
        }
    }
}
