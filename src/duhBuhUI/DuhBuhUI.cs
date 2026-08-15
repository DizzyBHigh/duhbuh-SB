// duhBuhUI - self-contained settings UI framework for Streamer.bot C# actions.
// UI controls are created on a dedicated STA thread.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    private const string DefaultDarkHeader = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Dark%20Banner.png";
    private const string DefaultLightHeader = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Light%20Banner.png";

    private sealed class ControlDefinition
    {
        public string Type; public string Title; public string Description; public string Category; public string Key; public object DefaultValue; public int Minimum; public int Maximum; public bool Multiline; public string[] Options;
    }
    private sealed class TitleDefinition { public string Title; public string Category; }
    private sealed class ButtonDefinition { public string Title; public string Description; public string ButtonText; public string Color; public string Category; public Action Callback; }

    public DuhBuhUI(string extensionName, string extensionVersion, Func<string, bool, bool?> getBool, Func<string, bool, int?> getInt, Func<string, bool, string> getString, Func<string, bool, object> getObject, Action<string, object, bool> setGlobal, Action<string> logInfo)
    {
        _extensionName = extensionName ?? "duhBuh"; _extensionVersion = extensionVersion ?? "0.1.0"; _getBool = getBool; _getInt = getInt; _getString = getString; _getObject = getObject; _setGlobal = setGlobal; _logInfo = logInfo;
        _defaults["__duhbuh_headerDarkImage"] = DefaultDarkHeader;
        _defaults["__duhbuh_headerLightImage"] = DefaultLightHeader;
    }

    public void AddHeader(string imageUrl) { AddHeader(imageUrl, imageUrl); }
    public void AddHeader(string darkImageUrl, string lightImageUrl) { RegisterCategory("__header"); _defaults["__duhbuh_headerDarkImage"] = darkImageUrl ?? ""; _defaults["__duhbuh_headerLightImage"] = lightImageUrl ?? ""; }
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
        Window window = new Window { Title = _extensionName + " - Settings", Width = 760, Height = 920, MinWidth = 600, MinHeight = 650, WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = ThemeBrush(theme, "WindowBackground") };
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
        string key = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? "__duhbuh_headerLightImage" : "__duhbuh_headerDarkImage";
        object headerValue = null;
        string headerImage = "";
        if (_defaults.TryGetValue(key, out headerValue) && headerValue != null)
            headerImage = Convert.ToString(headerValue, CultureInfo.InvariantCulture);
        _logInfo("[duhBuhUI] Header image key=" + key + " value=" + headerImage);
        StackPanel panel = new StackPanel { Margin = new Thickness(8, 8, 8, 4) };
        if (!string.IsNullOrWhiteSpace(headerImage))
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit(); bitmap.UriSource = new Uri(headerImage, UriKind.Absolute); bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.DecodePixelWidth = 720; bitmap.EndInit();
                Image image = new Image { Source = bitmap, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 720, MaxHeight = 180, Margin = new Thickness(0, 0, 0, 6) };
                panel.Children.Add(image);
                _logInfo("[duhBuhUI] RTS settings banner loaded: " + headerImage);
            }
            catch (Exception ex)
            {
                _logInfo("[duhBuhUI] Unable to load RTS settings banner: " + headerImage + " | " + ex.Message);
                panel.Children.Add(new TextBlock { Text = "The Road to Somewhere", FontSize = 26, FontWeight = FontWeights.Bold, Foreground = ThemeBrush(theme, "AccentText"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(16, 14, 16, 8) });
            }
        }
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

    private void BuildColorControl(StackPanel panel, ControlDefinition d, string theme) { StackPanel box = FieldBox(theme, d.Description); box.Children.Insert(0, new TextBlock { Text = d.Title, FontSize = 14, Foreground = ThemeBrush(theme, "PrimaryText") }); string current = NormalizeColor(Read(d.Key, (string)d.DefaultValue)) ?? "#FFFFFFFF"; TextBox text = new TextBox { Text = current, Tag = d.Key, MinWidth = 160 }; Button pick = new Button { Content = "Pick…", Margin = new Thickness(8, 0, 0, 0), Padding = new Thickness(10, 4, 10, 4) }; StackPanel row = new StackPanel { Orientation = Orientation.Horizontal }; row.Children.Add(text); row.Children.Add(pick); pick.Click += delegate { text.Text = current; }; box.Children.Insert(1, row); panel.Children.Add(box); }
    private StackPanel FieldBox(string theme, string description) { StackPanel box = new StackPanel { Margin = new Thickness(0, 0, 0, 12) }; if (!string.IsNullOrWhiteSpace(description)) box.Children.Add(new TextBlock { Text = description, FontSize = 11, Foreground = ThemeBrush(theme, "SecondaryText"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 5) }); return box; }
    private void RegisterCategory(string category) { if (string.IsNullOrEmpty(category) || _categories.Contains(category)) return; _categories.Add(category); }
    private object ReadObject(string key, object fallback) { try { object value = _getObject(key, true); return value ?? fallback; } catch { return fallback; } }
    private bool Read(string key, bool fallback) { try { bool? value = _getBool(key, true); return value.HasValue ? value.Value : fallback; } catch { return fallback; } }
    private int Read(string key, int fallback) { try { int? value = _getInt(key, true); return value.HasValue ? value.Value : fallback; } catch { return fallback; } }
    private string Read(string key, string fallback) { try { string value = _getString(key, true); return value ?? fallback; } catch { return fallback; } }
    private void Save(DockPanel root) { foreach (KeyValuePair<string, object> item in _defaults) { if (item.Key.StartsWith("__duhbuh_", StringComparison.Ordinal)) continue; SaveTagged(root, item.Key); } _logInfo("[duhBuhUI] Saved " + _defaults.Count + " settings for " + _extensionName + " v" + _extensionVersion + "."); }
    private void SaveTagged(DependencyObject parent, string key) { foreach (object childObject in LogicalTreeHelper.GetChildren(parent)) { DependencyObject dep = childObject as DependencyObject; if (dep == null) continue; FrameworkElement fe = dep as FrameworkElement; if (fe != null && Equals(fe.Tag, key)) { CheckBox cb = fe as CheckBox; if (cb != null) _setGlobal(key, cb.IsChecked == true, true); Slider sl = fe as Slider; if (sl != null) _setGlobal(key, (int)Math.Round(sl.Value), true); TextBox tb = fe as TextBox; if (tb != null) _setGlobal(key, tb.Text, true); ComboBox co = fe as ComboBox; if (co != null) _setGlobal(key, Convert.ToString(co.SelectedItem, CultureInfo.InvariantCulture), true); DatePicker dp = fe as DatePicker; if (dp != null) _setGlobal(key, dp.SelectedDate.HasValue ? dp.SelectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "", true); } SaveTagged(dep, key); } }
    private string NormalizeColor(string value) { if (string.IsNullOrWhiteSpace(value)) return null; string v = value.Trim(); if (!v.StartsWith("#", StringComparison.Ordinal)) v = "#" + v; if (v.Length == 7) v = "#FF" + v.Substring(1); if (v.Length != 9) return null; return v.ToUpperInvariant(); }
    private DateTime? ParseDate(string value) { DateTime result; return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : (DateTime?)null; }
    private string ExtractTime(string value) { if (string.IsNullOrWhiteSpace(value)) return ""; int p = value.IndexOf(' '); return p >= 0 && p + 1 < value.Length ? value.Substring(p + 1) : value; }
    private Brush ThemeBrush(string theme, string name) { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase) ? (name == "WindowBackground" ? "#FFF4F4F4" : name == "PanelBackground" ? "#FFFFFFFF" : name == "PrimaryText" ? "#FF202020" : name == "SecondaryText" ? "#FF666666" : "#FF0066CC") : (name == "WindowBackground" ? "#FF181818" : name == "PanelBackground" ? "#FF242424" : name == "PrimaryText" ? "#FFF0F0F0" : name == "SecondaryText" ? "#FFAAAAAA" : "#FF66AAFF")); }
}
