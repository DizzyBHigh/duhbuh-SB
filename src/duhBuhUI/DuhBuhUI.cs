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
        public string[] Options;
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

    // The image is rendered as a top banner when supplied. A later DLL version can
    // also expose a theme asset manager without changing extension settings code.
    public void AddHeader(string imageUrl)
    {
        RegisterCategory("__header");
        _defaults["__duhbuh_headerImage"] = imageUrl ?? "";
    }

    public void AddThemeSelector(string title, string description, string category, string variableName, string defaultValue)
    {
        AddDropdown(title, description, category, variableName, new[] { "Dark", "Light", "System" }, defaultValue);
    }

    public void AddRadioGroup(string title, string description, string category, string variableName, string[] options, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? (options != null && options.Length > 0 ? options[0] : "");
        _controls.Add(new ControlDefinition
        {
            Type = "radio", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName], Options = options ?? new string[0]
        });
    }

    public void AddDropdown(string title, string description, string category, string variableName, string[] options, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? (options != null && options.Length > 0 ? options[0] : "");
        _controls.Add(new ControlDefinition
        {
            Type = "dropdown", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName], Options = options ?? new string[0]
        });
    }

    public void AddColorPicker(string title, string description, string category, string variableName, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? "#FFFFFFFF";
        _controls.Add(new ControlDefinition
        {
            Type = "color", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName]
        });
    }

    public void AddDatePicker(string title, string description, string category, string variableName, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _controls.Add(new ControlDefinition
        {
            Type = "date", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName]
        });
    }

    public void AddTimePicker(string title, string description, string category, string variableName, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
        _controls.Add(new ControlDefinition
        {
            Type = "time", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName]
        });
    }

    public void AddDateTimePicker(string title, string description, string category, string variableName, string defaultValue)
    {
        RegisterCategory(category);
        _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        _controls.Add(new ControlDefinition
        {
            Type = "datetime", Title = title, Description = description, Category = category,
            Key = variableName, DefaultValue = _defaults[variableName]
        });
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
            if (item.Key.StartsWith("__duhbuh_", StringComparison.Ordinal)) continue;
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
        string theme = Read("duhbuh_ui_theme", "Dark");
        if (string.Equals(theme, "System", StringComparison.OrdinalIgnoreCase)) theme = "Dark";

        Window window = new Window
        {
            Title = _extensionName + " - Settings",
            Width = 760,
            Height = 820,
            MinWidth = 600,
            MinHeight = 560,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = ThemeBrush(theme, "WindowBackground")
        };

        DockPanel root = new DockPanel();
        StackPanel header = BuildHeader(theme);
        if (header != null)
        {
            DockPanel.SetDock(header, Dock.Top);
            root.Children.Add(header);
        }

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

        TabControl tabs = new TabControl { Margin = new Thickness(8), Background = ThemeBrush(theme, "PanelBackground") };
        for (int i = 0; i < _categories.Count; i++)
        {
            string category = _categories[i];
            if (category == "__header") continue;
            StackPanel panel = new StackPanel { Margin = new Thickness(18) };
            BuildCategory(panel, category, theme);
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

    private StackPanel BuildHeader(string theme)
    {
        string headerImage = Read("__duhbuh_headerImage", "");
        StackPanel panel = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };
        if (!string.IsNullOrWhiteSpace(headerImage))
        {
            TextBlock banner = new TextBlock
            {
                Text = "The Road to Somewhere",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = ThemeBrush(theme, "AccentText"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(16, 14, 16, 8)
            };
            panel.Children.Add(banner);
        }
        TextBlock subtitle = new TextBlock
        {
            Text = _extensionName + "  •  v" + _extensionVersion,
            FontSize = 12,
            Foreground = ThemeBrush(theme, "SecondaryText"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6)
        };
        panel.Children.Add(subtitle);
        return panel;
    }

    private void BuildCategory(StackPanel panel, string category, string theme)
    {
        for (int i = 0; i < _titles.Count; i++)
        {
            if (_titles[i].Category == category)
                panel.Children.Add(new TextBlock
                {
                    Text = _titles[i].Title,
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush(theme, "PrimaryText"),
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
                    Foreground = ThemeBrush(theme, "PrimaryText"),
                    Margin = new Thickness(0, 5, 0, 2),
                    Tag = d.Key
                };
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, check);
                panel.Children.Add(box);
            }
            else if (d.Type == "slider")
            {
                int value = Read(d.Key, (int)d.DefaultValue);
                StackPanel box = FieldBox(theme, d.Description);
                TextBlock label = new TextBlock { Text = d.Title + ": " + value, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") };
                Slider slider = new Slider
                {
                    Minimum = d.Minimum, Maximum = d.Maximum, Value = Math.Max(d.Minimum, Math.Min(d.Maximum, value)),
                    TickFrequency = 1, IsSnapToTickEnabled = true, Tag = d.Key
                };
                slider.ValueChanged += delegate(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
                {
                    label.Text = d.Title + ": " + ((int)Math.Round(e.NewValue));
                };
                box.Children.Insert(0, label);
                box.Children.Insert(1, slider);
                panel.Children.Add(box);
            }
            else if (d.Type == "textbox")
            {
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
                TextBox text = new TextBox
                {
                    Text = Read(d.Key, (string)d.DefaultValue), AcceptsReturn = d.Multiline,
                    TextWrapping = TextWrapping.Wrap, MinHeight = d.Multiline ? 65 : 30, Tag = d.Key
                };
                box.Children.Insert(1, text);
                panel.Children.Add(box);
            }
            else if (d.Type == "dropdown")
            {
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
                ComboBox combo = new ComboBox { Tag = d.Key, MinWidth = 180, Margin = new Thickness(0, 4, 0, 0) };
                for (int j = 0; j < d.Options.Length; j++) combo.Items.Add(d.Options[j]);
                string current = Read(d.Key, (string)d.DefaultValue);
                combo.SelectedItem = current;
                if (combo.SelectedIndex < 0 && combo.Items.Count > 0) combo.SelectedIndex = 0;
                box.Children.Insert(1, combo);
                panel.Children.Add(box);
            }
            else if (d.Type == "radio")
            {
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
                string current = Read(d.Key, (string)d.DefaultValue);
                StackPanel group = new StackPanel { Margin = new Thickness(0, 5, 0, 0), Tag = d.Key };
                for (int j = 0; j < d.Options.Length; j++)
                {
                    RadioButton radio = new RadioButton
                    {
                        Content = d.Options[j],
                        IsChecked = string.Equals(current, d.Options[j], StringComparison.Ordinal),
                        GroupName = d.Key,
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    group.Children.Add(radio);
                }
                box.Children.Insert(1, group);
                panel.Children.Add(box);
            }
            else if (d.Type == "color")
            {
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
                TextBox color = new TextBox { Text = Read(d.Key, (string)d.DefaultValue), Tag = d.Key, MinWidth = 160 };
                box.Children.Insert(1, color);
                box.Children.Insert(2, new TextBlock { Text = "Use #RRGGBB or #AARRGGBB", Foreground = ThemeBrush(theme, "SecondaryText"), Margin = new Thickness(0, 2, 0, 0) });
                panel.Children.Add(box);
            }
            else if (d.Type == "date" || d.Type == "time" || d.Type == "datetime")
            {
                StackPanel box = FieldBox(theme, d.Description);
                box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
                DatePicker date = null;
                TextBox time = null;
                if (d.Type == "date" || d.Type == "datetime")
                {
                    date = new DatePicker { Tag = d.Key, SelectedDate = ParseDate(Read(d.Key, (string)d.DefaultValue)) };
                    box.Insert(1, date);
                }
                if (d.Type == "time" || d.Type == "datetime")
                {
                    time = new TextBox { Tag = d.Type == "datetime" ? d.Key + "::time" : d.Key, Text = ExtractTime(Read(d.Key, (string)d.DefaultValue)), MinWidth = 100, Margin = new Thickness(0, 4, 0, 0) };
                    box.Insert(2, time);
                }
                panel.Children.Add(box);
            }
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            ButtonDefinition d = _buttons[i];
            if (d.Category != category) continue;
            StackPanel box = FieldBox(theme, d.Description);
            box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
            Button button = new Button { Content = d.ButtonText, Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
            Action callback = d.Callback;
            button.Click += delegate { if (callback != null) callback(); };
            box.Children.Insert(1, button);
            panel.Children.Add(box);
        }
    }

    private StackPanel FieldBox(string theme, string description)
    {
        StackPanel box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) };
        if (!string.IsNullOrWhiteSpace(description))
            box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Foreground = ThemeBrush(theme, "SecondaryText"), Margin = new Thickness(0, 3, 0, 0) });
        return box;
    }

    public void AddPopupWindow(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private Brush ThemeBrush(string theme, string role)
    {
        if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
        {
            if (role == "WindowBackground") return new SolidColorBrush(Color.FromRgb(246, 247, 249));
            if (role == "PanelBackground") return new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (role == "PrimaryText") return new SolidColorBrush(Color.FromRgb(30, 32, 38));
            if (role == "SecondaryText") return new SolidColorBrush(Color.FromRgb(90, 94, 104));
            if (role == "AccentText") return new SolidColorBrush(Color.FromRgb(44, 90, 160));
        }
        if (role == "WindowBackground") return new SolidColorBrush(Color.FromRgb(28, 30, 34));
        if (role == "PanelBackground") return new SolidColorBrush(Color.FromRgb(36, 39, 45));
        if (role == "PrimaryText") return new SolidColorBrush(Color.FromRgb(240, 242, 245));
        if (role == "SecondaryText") return new SolidColorBrush(Color.FromRgb(170, 175, 185));
        if (role == "AccentText") return new SolidColorBrush(Color.FromRgb(115, 170, 255));
        return Brushes.White;
    }

    private DateTime? ParseDate(string value)
    {
        DateTime parsed;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ? (DateTime?)parsed.Date : null;
    }

    private string ExtractTime(string value)
    {
        DateTime parsed;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            return parsed.ToString("HH:mm", CultureInfo.InvariantCulture);
        return value;
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
        {
            if (item.Key.StartsWith("__duhbuh_", StringComparison.Ordinal)) continue;
            SaveTagged(root, item.Key);
        }

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
                ComboBox combo = fe as ComboBox;
                if (combo != null) _setGlobal(key, combo.SelectedItem == null ? "" : combo.SelectedItem.ToString(), true);
                StackPanel group = fe as StackPanel;
                if (group != null && group.Tag != null)
                {
                    foreach (object radioObject in group.Children)
                    {
                        RadioButton radio = radioObject as RadioButton;
                        if (radio != null && radio.IsChecked == true) _setGlobal(key, radio.Content == null ? "" : radio.Content.ToString(), true);
                    }
                }
                DatePicker date = fe as DatePicker;
                if (date != null)
                {
                    string current = Read(key, "");
                    string time = ExtractTime(current);
                    string dateText = date.SelectedDate.HasValue ? date.SelectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
                    if (!string.IsNullOrWhiteSpace(time) && time != current && current.IndexOf(':') >= 0)
                        _setGlobal(key, dateText + " " + time, true);
                    else
                        _setGlobal(key, dateText, true);
                }
            }
            SaveTagged(dep, key);
        }
    }
}
