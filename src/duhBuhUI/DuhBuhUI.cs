// duhBuhUI - self-contained settings UI framework for Streamer.bot C# actions.
// UI controls are created only on a dedicated STA thread. Streamer.bot ExecuteCode
// actions are not guaranteed to run on an STA thread.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public sealed class DuhBuhUI
{
    private readonly Func<string, bool, bool?> _getBool;
    private readonly Func<string, bool, int?> _getInt;
    private readonly Func<string, bool, string> _getString;
    private readonly Func<string, bool, object> _getObject;
    private readonly Action<string, object, bool> _setGlobal;
    private readonly Action<string> _logInfo;
    private readonly string _extensionName;
    private readonly string _extensionVersion;

    private readonly List<ControlDefinition> _controls = new List<ControlDefinition>();
    private readonly List<TitleDefinition> _titles = new List<TitleDefinition>();
    private readonly List<ButtonDefinition> _buttons = new List<ButtonDefinition>();
    private readonly List<string> _categories = new List<string>();
    private readonly Dictionary<string, object> _defaults = new Dictionary<string, object>();

    private sealed class ControlDefinition
    {
        public string Type;
        public string Title;
        public string Description;
        public string Category;
        public string Key;
        public object DefaultValue;
        public int Minimum;
        public int Maximum;
        public bool Multiline;
    }

    private sealed class TitleDefinition
    {
        public string Title;
        public string Category;
    }

    private sealed class ButtonDefinition
    {
        public string Title;
        public string Description;
        public string ButtonText;
        public string Color;
        public string Category;
        public Action Callback;
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
        // Header rendering will be added after the core STA-safe UI is validated.
    }

    private void RegisterCategory(string category)
    {
        if (category == null) category = "General";
        if (!_categories.Contains(category)) _categories.Add(category);
    }

    public void AddTitle(string title, string category)
    {
        RegisterCategory(category);
        _titles.Add(new TitleDefinition { Title = title, Category = category });
    }

    public void AddToggleSwitch(string title, string description, string category, string variableName, bool defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue;
        _controls.Add(new ControlDefinition
        {
            Type = "toggle", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = defaultValue
        });
    }

    public void AddSlider(string title, string description, string category, string variableName, int minimum, int maximum, int defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue;
        _controls.Add(new ControlDefinition
        {
            Type = "slider", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = defaultValue, Minimum = minimum, Maximum = maximum
        });
    }

    public void AddTextbox(string title, string description, string category, string variableName, string defaultValue, bool multiline)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue;
        _controls.Add(new ControlDefinition
        {
            Type = "textbox", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = defaultValue, Multiline = multiline
        });
    }

    public void AddClickableButton(string title, string description, string buttonText, string color, string category, Action callback)
    {
        RegisterCategory(category);
        _buttons.Add(new ButtonDefinition
        {
            Title = title, Description = description, ButtonText = buttonText,
            Color = color, Category = category, Callback = callback
        });
    }

    public void LogExistingSettings()
    {
        foreach (KeyValuePair<string, object> item in _defaults)
        {
            object value = ReadObject(item.Key, item.Value);
            _logInfo("[duhBuhUI] " + item.Key + " = " + Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }

    public void ShowUI()
    {
        Exception threadError = null;
        Thread uiThread = new Thread(() =>
        {
            try
            {
                Window window = BuildWindow();
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                threadError = ex;
            }
        });

        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.IsBackground = false;
        uiThread.Start();
        uiThread.Join();

        if (threadError != null) throw threadError;
    }

    private Window BuildWindow()
    {
        Window window = new Window
        {
            Title = _extensionName + " - Settings",
            Width = 720,
            Height = 760,
            MinWidth = 560,
            MinHeight = 500,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        DockPanel root = new DockPanel();
        StackPanel footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12)
        };

        Button save = new Button { Content = "Save", Padding = new Thickness(18, 7, 18, 7), Margin = new Thickness(0, 0, 8, 0) };
        Button close = new Button { Content = "Save & Exit", Padding = new Thickness(18, 7, 18, 7) };
        save.Click += delegate { Save(root); };
        close.Click += delegate { Save(root); window.Close(); };
        footer.Children.Add(save);
        footer.Children.Add(close);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        TabControl tabs = new TabControl { Margin = new Thickness(8) };
        for (int i = 0; i < _categories.Count; i++)
        {
            string category = _categories[i];
            StackPanel panel = new StackPanel { Margin = new Thickness(18) };
            BuildCategory(panel, category);
            tabs.Items.Add(new TabItem
            {
                Header = category,
                Content = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = panel
                }
            });
        }

        root.Children.Add(tabs);
        window.Content = root;
        return window;
    }

    private void BuildCategory(StackPanel panel, string category)
    {
        for (int i = 0; i < _titles.Count; i++)
        {
            if (_titles[i].Category == category)
                panel.Children.Add(new TextBlock
                {
                    Text = _titles[i].Title,
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 12)
                });
        }

        for (int i = 0; i < _controls.Count; i++)
        {
            ControlDefinition d = _controls[i];
            if (d.Category != category) continue;

            if (d.Type == "toggle")
            {
                CheckBox check = new CheckBox
                {
                    Content = d.Title,
                    IsChecked = Read(d.Key, (bool)d.DefaultValue),
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 2),
                    Tag = d.Key
                };
                StackPanel box = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
                box.Children.Add(check);
                box.Children.Add(new TextBlock { Text = d.Description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(24, 0, 0, 0) });
                panel.Children.Add(box);
            }
            else if (d.Type == "slider")
            {
                int value = Read(d.Key, (int)d.DefaultValue);
                StackPanel box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
                TextBlock label = new TextBlock { Text = d.Title + ": " + value, FontSize = 14 };
                Slider slider = new Slider
                {
                    Minimum = d.Minimum, Maximum = d.Maximum, Value = Math.Max(d.Minimum, Math.Min(d.Maximum, value)),
                    TickFrequency = 1, IsSnapToTickEnabled = true, Tag = d.Key
                };
                slider.ValueChanged += delegate(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
                {
                    label.Text = d.Title + ": " + ((int)Math.Round(e.NewValue));
                };
                box.Children.Add(label);
                box.Children.Add(slider);
                box.Children.Add(new TextBlock { Text = d.Description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
                panel.Children.Add(box);
            }
            else if (d.Type == "textbox")
            {
                StackPanel box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
                box.Children.Add(new TextBlock { Text = d.Title, FontSize = 14 });
                TextBox text = new TextBox
                {
                    Text = Read(d.Key, (string)d.DefaultValue), AcceptsReturn = d.Multiline,
                    TextWrapping = TextWrapping.Wrap, MinHeight = d.Multiline ? 65 : 30, Tag = d.Key
                };
                box.Children.Add(text);
                box.Children.Add(new TextBlock { Text = d.Description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
                panel.Children.Add(box);
            }
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            ButtonDefinition d = _buttons[i];
            if (d.Category != category) continue;
            StackPanel box = new StackPanel { Margin = new Thickness(0, 5, 0, 14) };
            box.Children.Add(new TextBlock { Text = d.Title, FontSize = 14 });
            Button button = new Button { Content = d.ButtonText, Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
            Action callback = d.Callback;
            button.Click += delegate { if (callback != null) callback(); };
            box.Children.Add(button);
            box.Children.Add(new TextBlock { Text = d.Description, TextWrapping = TextWrapping.Wrap, Opacity = .72, Margin = new Thickness(0, 3, 0, 0) });
            panel.Children.Add(box);
        }
    }

    public void AddPopupWindow(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private object ReadObject(string key, object fallback)
    {
        try { object value = _getObject(key, true); return value ?? fallback; }
        catch { return fallback; }
    }

    private bool Read(string key, bool fallback)
    {
        try { bool? value = _getBool(key, true); return value.HasValue ? value.Value : fallback; }
        catch { return fallback; }
    }

    private int Read(string key, int fallback)
    {
        try { int? value = _getInt(key, true); return value.HasValue ? value.Value : fallback; }
        catch { return fallback; }
    }

    private string Read(string key, string fallback)
    {
        try { string value = _getString(key, true); return value ?? fallback; }
        catch { return fallback; }
    }

    private void Save(DockPanel root)
    {
        foreach (KeyValuePair<string, object> item in _defaults)
            SaveTagged(root, item.Key);

        _logInfo("[duhBuhUI] Saved " + _defaults.Count + " settings for " + _extensionName + " v" + _extensionVersion + ".");
    }

    private void SaveTagged(DependencyObject parent, string key)
    {
        foreach (object childObject in LogicalTreeHelper.GetChildren(parent))
        {
            DependencyObject dep = childObject as DependencyObject;
            if (dep == null) continue;
            FrameworkElement fe = dep as FrameworkElement;
            if (fe != null && Equals(fe.Tag, key))
            {
                CheckBox cb = fe as CheckBox;
                if (cb != null) _setGlobal(key, cb.IsChecked == true, true);
                Slider sl = fe as Slider;
                if (sl != null) _setGlobal(key, (int)Math.Round(sl.Value), true);
                TextBox tb = fe as TextBox;
                if (tb != null) _setGlobal(key, tb.Text, true);
            }
            SaveTagged(dep, key);
        }
    }
}
