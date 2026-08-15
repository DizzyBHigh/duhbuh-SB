// duhBuhUI - self-contained settings UI framework for Streamer.bot C# actions.
// UI controls are created on a dedicated STA thread.

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
        public string Type; public string Title; public string Description; public string Category; public string Key; public object DefaultValue; public int Minimum; public int Maximum; public bool Multiline; public string[] Options;
    }
    private sealed class TitleDefinition { public string Title; public string Category; }
    private sealed class ButtonDefinition { public string Title; public string Description; public string ButtonText; public string Color; public string Category; public Action Callback; }

    public DuhBuhUI(string extensionName, string extensionVersion, Func<string, bool, bool?> getBool, Func<string, bool, int?> getInt, Func<string, bool, string> getString, Func<string, bool, object> getObject, Action<string, object, bool> setGlobal, Action<string> logInfo)
    {
        _extensionName = extensionName ?? "duhBuh"; _extensionVersion = extensionVersion ?? "0.1.0"; _getBool = getBool; _getInt = getInt; _getString = getString; _getObject = getObject; _setGlobal = setGlobal; _logInfo = logInfo;
    }
    public void AddHeader(string imageUrl) { RegisterCategory("__header"); _defaults["__duhbuh_headerImage"] = imageUrl ?? ""; }
    public void AddThemeSelector(string title, string description, string category, string variableName, string defaultValue) { AddDropdown(title, description, category, variableName, new[] { "Dark", "Light", "System" }, defaultValue); }
    public void AddRadioGroup(string title, string description, string category, string variableName, string[] options, string defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue ?? (options != null && options.Length > 0 ? options[0] : ""); _controls.Add(new ControlDefinition { Type = "radio", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName], Options = options ?? new string[0] }); }
    public void AddDropdown(string title, string description, string category, string variableName, string[] options, string defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue ?? (options != null && options.Length > 0 ? options[0] : ""); _controls.Add(new ControlDefinition { Type = "dropdown", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName], Options = options ?? new string[0] }); }
    public void AddColorPicker(string title, string description, string category, string variableName, string defaultValue) { RegisterCategory(category); _defaults[variableName] = NormalizeColor(defaultValue) ?? "#FFFFFFFF"; _controls.Add(new ControlDefinition { Type = "color", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName] }); }
    public void AddDatePicker(string title, string description, string category, string variableName, string defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); _controls.Add(new ControlDefinition { Type = "date", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName] }); }
    public void AddTimePicker(string title, string description, string category, string variableName, string defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture); _controls.Add(new ControlDefinition { Type = "time", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName] }); }
    public void AddDateTimePicker(string title, string description, string category, string variableName, string defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture); _controls.Add(new ControlDefinition { Type = "datetime", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = _defaults[variableName] }); }
    public void AddTitle(string title, string category) { RegisterCategory(category); _titles.Add(new TitleDefinition { Title = title, Category = category }); }
    public void AddToggleSwitch(string title, string description, string category, string variableName, bool defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue; _controls.Add(new ControlDefinition { Type = "toggle", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = defaultValue }); }
    public void AddSlider(string title, string description, string category, string variableName, int minimum, int maximum, int defaultValue) { RegisterCategory(category); _defaults[variableName] = defaultValue; _controls.Add(new ControlDefinition { Type = "slider", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = defaultValue, Minimum = minimum, Maximum = maximum }); }
    public void AddTextbox(string title, string description, string category, string variableName, string defaultValue, bool multiline) { RegisterCategory(category); _defaults[variableName] = defaultValue; _controls.Add(new ControlDefinition { Type = "textbox", Title = title, Description = description, Category = category, Key = variableName, DefaultValue = defaultValue, Multiline = multiline }); }
    public void AddClickableButton(string title, string description, string buttonText, string color, string category, Action callback) { RegisterCategory(category); _buttons.Add(new ButtonDefinition { Title = title, Description = description, ButtonText = buttonText, Color = color, Category = category, Callback = callback }); }
    public void AddPopupWindow(string title, string message) { MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information); }
    public void LogExistingSettings() { foreach (KeyValuePair<string, object> item in _defaults) { if (item.Key.StartsWith("__duhbuh_", StringComparison.Ordinal)) continue; object value = ReadObject(item.Key, item.Value); _logInfo("[duhBuhUI] " + item.Key + " = " + Convert.ToString(value, CultureInfo.InvariantCulture)); } }

    public void ShowUI()
    {
        Exception threadError = null;
        Thread uiThread = new Thread((ThreadStart)delegate { try { Window window = BuildWindow(); window.ShowDialog(); } catch (Exception ex) { threadError = ex; } });
        uiThread.SetApartmentState(ApartmentState.STA); uiThread.IsBackground = false; uiThread.Start(); uiThread.Join(); if (threadError != null) throw threadError;
    }

    private Window BuildWindow()
    {
        string theme = Read("duhbuh_ui_theme", "Dark"); if (string.Equals(theme, "System", StringComparison.OrdinalIgnoreCase)) theme = "Dark";
        Window window = new Window { Title = _extensionName + " - Settings", Width = 760, Height = 820, MinWidth = 600, MinHeight = 560, WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = ThemeBrush(theme, "WindowBackground") };
        DockPanel root = new DockPanel(); StackPanel header = BuildHeader(theme); if (header != null) { DockPanel.SetDock(header, Dock.Top); root.Children.Add(header); }
        StackPanel footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(12) };
        Button save = new Button { Content = "Save", Padding = new Thickness(18, 7, 18, 7), Margin = new Thickness(0, 0, 8, 0) }; Button close = new Button { Content = "Save & Exit", Padding = new Thickness(18, 7, 18, 7) };
        save.Click += delegate { Save(root); }; close.Click += delegate { Save(root); window.Close(); }; footer.Children.Add(save); footer.Children.Add(close); DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        TabControl tabs = new TabControl { Margin = new Thickness(8), Background = ThemeBrush(theme, "PanelBackground") };
        for (int i = 0; i < _categories.Count; i++) { string category = _categories[i]; if (category == "__header") continue; StackPanel panel = new StackPanel { Margin = new Thickness(18) }; BuildCategory(panel, category, theme); tabs.Items.Add(new TabItem { Header = category, Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = panel } }); }
        root.Children.Add(tabs); window.Content = root; return window;
    }

    private StackPanel BuildHeader(string theme)
    {
        string headerImage = Read("__duhbuh_headerImage", ""); StackPanel panel = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };
        if (!string.IsNullOrWhiteSpace(headerImage)) panel.Children.Add(new TextBlock { Text = "The Road to Somewhere", FontSize = 26, FontWeight = FontWeights.Bold, Foreground = ThemeBrush(theme, "AccentText"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(16, 14, 16, 8) });
        panel.Children.Add(new TextBlock { Text = _extensionName + "  •  v" + _extensionVersion, FontSize = 12, Foreground = ThemeBrush(theme, "SecondaryText"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 6) }); return panel;
    }

    private void BuildCategory(StackPanel panel, string category, string theme)
    {
        for (int i = 0; i < _titles.Count; i++) if (_titles[i].Category == category) panel.Children.Add(new TextBlock { Text = _titles[i].Title, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = ThemeBrush(theme, "PrimaryText"), Margin = new Thickness(0, 10, 0, 12) });
        for (int i = 0; i < _controls.Count; i++)
        {
            ControlDefinition d = _controls[i]; if (d.Category != category) continue;
            if (d.Type == "toggle") { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new CheckBox { Content = d.Title, IsChecked = Read(d.Key, (bool)d.DefaultValue), FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText"), Margin = new Thickness(0, 5, 0, 2), Tag = d.Key }); panel.Children.Add(box); }
            else if (d.Type == "slider") { StackPanel box = FieldBox(theme, d.Description); int value = Read(d.Key, (int)d.DefaultValue); TextBlock label = new TextBlock { Text = d.Title + ": " + value, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }; Slider slider = new Slider { Minimum = d.Minimum, Maximum = d.Maximum, Value = Math.Max(d.Minimum, Math.Min(d.Maximum, value)), TickFrequency = 1, IsSnapToTickEnabled = true, Tag = d.Key }; slider.ValueChanged += delegate(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) { label.Text = d.Title + ": " + ((int)Math.Round(e.NewValue)); }; box.Children.Insert(0, label); box.Children.Insert(1, slider); panel.Children.Add(box); }
            else if (d.Type == "textbox") { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); box.Children.Insert(1, new TextBox { Text = Read(d.Key, (string)d.DefaultValue), AcceptsReturn = d.Multiline, TextWrapping = TextWrapping.Wrap, MinHeight = d.Multiline ? 65 : 30, Tag = d.Key }); panel.Children.Add(box); }
            else if (d.Type == "dropdown") { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); ComboBox combo = new ComboBox { Tag = d.Key, MinWidth = 180, Margin = new Thickness(0, 4, 0, 0) }; for (int j = 0; j < d.Options.Length; j++) combo.Items.Add(d.Options[j]); string current = Read(d.Key, (string)d.DefaultValue); combo.SelectedItem = current; if (combo.SelectedIndex < 0 && combo.Items.Count > 0) combo.SelectedIndex = 0; box.Children.Insert(1, combo); panel.Children.Add(box); }
            else if (d.Type == "radio") { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); StackPanel group = new StackPanel { Margin = new Thickness(0, 5, 0, 0), Tag = d.Key }; string current = Read(d.Key, (string)d.DefaultValue); for (int j = 0; j < d.Options.Length; j++) group.Children.Add(new RadioButton { Content = d.Options[j], IsChecked = string.Equals(current, d.Options[j], StringComparison.Ordinal), GroupName = d.Key, Margin = new Thickness(0, 2, 0, 2) }); box.Children.Insert(1, group); panel.Children.Add(box); }
            else if (d.Type == "color") BuildColorControl(panel, d, theme);
            else if (d.Type == "date" || d.Type == "time" || d.Type == "datetime") { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); string current = Read(d.Key, (string)d.DefaultValue); if (d.Type == "date" || d.Type == "datetime") box.Children.Insert(1, new DatePicker { Tag = d.Key, SelectedDate = ParseDate(current) }); if (d.Type == "time" || d.Type == "datetime") box.Children.Insert(2, new TextBox { Tag = d.Type == "datetime" ? d.Key + "::time" : d.Key, Text = ExtractTime(current), MinWidth = 100, Margin = new Thickness(0, 4, 0, 0) }); panel.Children.Add(box); }
        }
        for (int i = 0; i < _buttons.Count; i++) { ButtonDefinition d = _buttons[i]; if (d.Category != category) continue; StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); Button button = new Button { Content = d.ButtonText, Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left }; Action callback = d.Callback; button.Click += delegate { if (callback != null) callback(); }; box.Children.Insert(1, button); panel.Children.Add(box); }
    }

    private void BuildColorControl(StackPanel panel, ControlDefinition d, string theme)
    {
        StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") });
        string current = NormalizeColor(Read(d.Key, (string)d.DefaultValue)); if (current == "") current = "#FFFFFFFF";
        StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0), Tag = d.Key };
        Button swatch = new Button { Width = 42, Height = 30, Margin = new Thickness(0, 0, 8, 0), Tag = d.Key, ToolTip = "Click to choose a colour" };
        TextBox hex = new TextBox { Text = current, Width = 120, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Tag = d.Key };
        Button choose = new Button { Content = "Choose…", Padding = new Thickness(10, 4, 10, 4), Tag = d.Key }; SetSwatch(swatch, current);
        swatch.Click += delegate { OpenColorPicker(hex, swatch, theme); }; choose.Click += delegate { OpenColorPicker(hex, swatch, theme); }; hex.LostFocus += delegate { string c = NormalizeColor(hex.Text); if (c != "") { hex.Text = c; SetSwatch(swatch, c); } };
        row.Children.Add(swatch); row.Children.Add(hex); row.Children.Add(choose); box.Children.Insert(1, row); box.Children.Insert(2, new TextBlock { Text = "Use the swatch or enter #RRGGBB / #AARRGGBB", Foreground = ThemeBrush(theme, "SecondaryText"), Margin = new Thickness(0, 2, 0, 0) }); panel.Children.Add(box);
    }

    private StackPanel FieldBox(string theme, string description)
    {
        StackPanel box = new StackPanel { Margin = new Thickness(0, 4, 0, 14) }; if (!string.IsNullOrWhiteSpace(description)) box.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Foreground = ThemeBrush(theme, "SecondaryText"), Margin = new Thickness(0, 3, 0, 0) }); return box;
    }

    private void OpenColorPicker(TextBox target, Button swatch, string theme)
    {
        string initial = NormalizeColor(target.Text); if (initial == "") initial = "#FFFFFFFF";
        Window picker = new Window { Title = "Choose Colour", Width = 520, Height = 480, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = ThemeBrush(theme, "WindowBackground") };
        StackPanel root = new StackPanel { Margin = new Thickness(18) };
        root.Children.Add(new TextBlock { Text = "Choose a notification colour", FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = ThemeBrush(theme, "PrimaryText"), Margin = new Thickness(0, 0, 0, 10) });
        Border preview = new Border { Height = 46, CornerRadius = new System.Windows.CornerRadius(4), Margin = new Thickness(0, 0, 0, 12), BorderBrush = ThemeBrush(theme, "SecondaryText"), BorderThickness = new Thickness(1) };
        TextBlock previewText = new TextBlock { Text = initial, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White, FontWeight = FontWeights.Bold }; preview.Child = previewText; SetPreview(preview, previewText, initial); root.Children.Add(preview);

        TextBox custom = null;
        WrapPanel palette = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 12) };
        string[] colours = new[] { "#FFFFFFFF", "#FFF2F2F2", "#FFBFBFBF", "#FF808080", "#FF404040", "#FF000000", "#FFFF0000", "#FFFF8000", "#FFFFFF00", "#FF80FF00", "#FF00FF00", "#FF00FFFF", "#FF0080FF", "#FF0000FF", "#FF8000FF", "#FFFF00FF", "#FFFF80C0", "#FF804000", "#FF800000", "#FF808000", "#FF008000", "#FF008080", "#FF000080", "#FF800080", "#FF00AEEF", "#FF0077B6", "#FF3A86FF", "#FF8338EC", "#FFFF006E", "#FFFB5607", "#FFFFBE0B", "#FF2A9D8F", "#FF06D6A0", "#FF118AB2", "#FFEF476F", "#FF6C757D" };
        for (int i = 0; i < colours.Length; i++)
        {
            string colour = colours[i]; Button swatchButton = new Button { Width = 38, Height = 32, Margin = new Thickness(3), Tag = colour, ToolTip = colour, Padding = new Thickness(0) }; swatchButton.Background = BrushFromHex(colour); swatchButton.BorderBrush = ThemeBrush(theme, "SecondaryText");
            swatchButton.Click += delegate(object sender, RoutedEventArgs e) { string selected = (string)((Button)sender).Tag; custom.Text = selected; SetPreview(preview, previewText, selected); };
            palette.Children.Add(swatchButton);
        }
        root.Children.Add(palette);

        StackPanel customRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        TextBlock customLabel = new TextBlock { Text = "Custom:", Foreground = ThemeBrush(theme, "PrimaryText"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
        custom = new TextBox { Text = initial, Width = 150, VerticalContentAlignment = VerticalAlignment.Center };
        Button applyCustom = new Button { Content = "Apply", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(8, 0, 0, 0) };
        customRow.Children.Add(customLabel); customRow.Children.Add(custom); customRow.Children.Add(applyCustom); root.Children.Add(customRow);
        custom.TextChanged += delegate { string c = NormalizeColor(custom.Text); if (c != "") SetPreview(preview, previewText, c); };
        applyCustom.Click += delegate { string c = NormalizeColor(custom.Text); if (c != "") SetPreview(preview, previewText, c); };

        StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        Button ok = new Button { Content = "OK", Padding = new Thickness(16, 6, 16, 6), Margin = new Thickness(0, 0, 8, 0) }; Button cancel = new Button { Content = "Cancel", Padding = new Thickness(16, 6, 16, 6) };
        ok.Click += delegate { string c = NormalizeColor(custom.Text); if (c == "") c = initial; target.Text = c; SetSwatch(swatch, c); picker.Close(); }; cancel.Click += delegate { picker.Close(); };
        buttons.Children.Add(ok); buttons.Children.Add(cancel); root.Children.Add(buttons); picker.Content = root; picker.ShowDialog();
    }

    private void SetPreview(Border preview, TextBlock text, string colour)
    {
        preview.Background = BrushFromHex(colour); text.Text = colour; Color c; try { c = (Color)ColorConverter.ConvertFromString(colour); } catch { c = Colors.White; } double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0; text.Foreground = luminance > 0.6 ? Brushes.Black : Brushes.White;
    }
    private void SetSwatch(Button button, string value) { button.Background = BrushFromHex(value); button.BorderBrush = new SolidColorBrush(Color.FromArgb(180, 120, 120, 120)); }
    private Brush BrushFromHex(string value) { try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(NormalizeColor(value))); } catch { return Brushes.Transparent; } }
    private string NormalizeColor(string value) { if (string.IsNullOrWhiteSpace(value)) return ""; string v = value.Trim(); if (!v.StartsWith("#", StringComparison.Ordinal)) v = "#" + v; if (v.Length == 7) return "#FF" + v.Substring(1).ToUpperInvariant(); if (v.Length == 9) return "#" + v.Substring(1).ToUpperInvariant(); return ""; }
    private void RegisterCategory(string category) { if (string.IsNullOrEmpty(category)) category = "General"; if (!_categories.Contains(category)) _categories.Add(category); }

    private Brush ThemeBrush(string theme, string role)
    {
        if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase)) { if (role == "WindowBackground") return new SolidColorBrush(Color.FromRgb(246, 247, 249)); if (role == "PanelBackground") return new SolidColorBrush(Color.FromRgb(255, 255, 255)); if (role == "PrimaryText") return new SolidColorBrush(Color.FromRgb(30, 32, 38)); if (role == "SecondaryText") return new SolidColorBrush(Color.FromRgb(90, 94, 104)); if (role == "AccentText") return new SolidColorBrush(Color.FromRgb(44, 90, 160)); }
        if (role == "WindowBackground") return new SolidColorBrush(Color.FromRgb(28, 30, 34)); if (role == "PanelBackground") return new SolidColorBrush(Color.FromRgb(36, 39, 45)); if (role == "PrimaryText") return new SolidColorBrush(Color.FromRgb(240, 242, 245)); if (role == "SecondaryText") return new SolidColorBrush(Color.FromRgb(170, 175, 185)); if (role == "AccentText") return new SolidColorBrush(Color.FromRgb(115, 170, 255)); return Brushes.White;
    }
    private DateTime? ParseDate(string value) { DateTime parsed; return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ? (DateTime?)parsed.Date : null; }
    private string ExtractTime(string value) { DateTime parsed; if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)) return parsed.ToString("HH:mm", CultureInfo.InvariantCulture); return value; }
    private object ReadObject(string key, object fallback) { try { object value = _getObject(key, true); return value ?? fallback; } catch { return fallback; } }
    private bool Read(string key, bool fallback) { try { bool? value = _getBool(key, true); return value.HasValue ? value.Value : fallback; } catch { return fallback; } }
    private int Read(string key, int fallback) { try { int? value = _getInt(key, true); return value.HasValue ? value.Value : fallback; } catch { return fallback; } }
    private string Read(string key, string fallback) { try { string value = _getString(key, true); return value ?? fallback; } catch { return fallback; } }

    private void Save(DockPanel root)
    {
        foreach (KeyValuePair<string, object> item in _defaults) { if (item.Key.StartsWith("__duhbuh_", StringComparison.Ordinal)) continue; SaveTagged(root, item.Key); }
        _logInfo("[duhBuhUI] Saved " + _defaults.Count + " settings for " + _extensionName + " v" + _extensionVersion + ".");
    }
    private void SaveTagged(DependencyObject parent, string key)
    {
        foreach (object childObject in LogicalTreeHelper.GetChildren(parent))
        {
            DependencyObject dep = childObject as DependencyObject; if (dep == null) continue; FrameworkElement fe = dep as FrameworkElement;
            if (fe != null && Equals(fe.Tag, key))
            {
                CheckBox cb = fe as CheckBox; if (cb != null) _setGlobal(key, cb.IsChecked == true, true);
                Slider sl = fe as Slider; if (sl != null) _setGlobal(key, (int)Math.Round(sl.Value), true);
                TextBox tb = fe as TextBox; if (tb != null) _setGlobal(key, tb.Text, true);
                ComboBox combo = fe as ComboBox; if (combo != null) _setGlobal(key, combo.SelectedItem == null ? "" : combo.SelectedItem.ToString(), true);
                StackPanel group = fe as StackPanel;
                if (group != null && group.Tag != null) for (int i = 0; i < group.Children.Count; i++) { RadioButton radio = group.Children[i] as RadioButton; if (radio != null && radio.IsChecked == true) _setGlobal(key, radio.Content == null ? "" : radio.Content.ToString(), true); }
                DatePicker date = fe as DatePicker;
                if (date != null) { string current = Read(key, ""); string time = ExtractTime(current); string dateText = date.SelectedDate.HasValue ? date.SelectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : ""; if (!string.IsNullOrWhiteSpace(time) && time != current && current.IndexOf(':') >= 0) _setGlobal(key, dateText + " " + time, true); else _setGlobal(key, dateText, true); }
                if (group != null && group.Tag != null) for (int i = 0; i < group.Children.Count; i++) { TextBox colorBox = group.Children[i] as TextBox; if (colorBox != null) { string color = NormalizeColor(colorBox.Text); if (color != "") { _setGlobal(key, color, true); break; } } }
            }
            SaveTagged(dep, key);
        }
    }
}
