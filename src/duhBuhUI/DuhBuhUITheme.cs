// duhBuhUITheme - safe visual styling for the shared settings UI.
// IMPORTANT: Do not replace TabControl/TabItem templates or modify generated tab content.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DuhBuhUITheme
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded));
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        if (window.Title == null || window.Title.IndexOf("Settings", StringComparison.OrdinalIgnoreCase) < 0) return;
        Apply(window, IsLightWindow(window));
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;

        ResourceDictionary r = window.Resources;
        r[typeof(Button)] = CreateButtonStyle(light);
        r[typeof(TextBox)] = CreateTextBoxStyle(light);
        r[typeof(ComboBox)] = CreateComboBoxStyle(light);
        r[typeof(DatePicker)] = CreateDatePickerStyle(light);
        r[typeof(CheckBox)] = CreateCheckBoxStyle(light);
        r[typeof(RadioButton)] = CreateRadioButtonStyle(light);

        r["duhBuhSectionBackground"] = new SolidColorBrush(light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(32, 35, 41));
        r["duhBuhSectionBorder"] = new SolidColorBrush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74));
        r["duhBuhAccent"] = new SolidColorBrush(light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 140, 205));
        r["duhBuhSectionText"] = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
        r["duhBuhDescriptionText"] = new SolidColorBrush(light ? Color.FromRgb(100, 106, 118) : Color.FromRgb(160, 167, 178));

        window.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnElementLoaded));
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        FrameworkElement element = e.OriginalSource as FrameworkElement;
        if (element == null) return;
        Window window = Window.GetWindow(element);
        if (window == null) return;
        bool light = IsLightWindow(window);

        TextBlock heading = element as TextBlock;
        if (heading != null && heading.FontSize >= 17 && heading.FontWeight == FontWeights.SemiBold)
        {
            heading.Foreground = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
            heading.Background = new SolidColorBrush(light ? Color.FromRgb(238, 241, 246) : Color.FromRgb(43, 47, 54));
            heading.Padding = new Thickness(10, 6, 10, 6);
            heading.Margin = new Thickness(0, 12, 0, 8);
            heading.HorizontalAlignment = HorizontalAlignment.Stretch;
            return;
        }

        StackPanel panel = element as StackPanel;
        if (panel == null) return;
        if (panel.Children.Count >= 2 && panel.Children.Count <= 4 && IsFieldPanel(panel))
        {
            panel.Background = new SolidColorBrush(light ? Color.FromRgb(250, 251, 253) : Color.FromRgb(39, 42, 48));
            panel.Margin = new Thickness(0, 2, 0, 10);
        }
    }

    private static bool IsFieldPanel(StackPanel panel)
    {
        bool hasText = false;
        bool hasControl = false;
        for (int i = 0; i < panel.Children.Count; i++)
        {
            UIElement child = panel.Children[i];
            if (child is TextBlock) hasText = true;
            if (child is Control || child is StackPanel) hasControl = true;
        }
        return hasText && hasControl;
    }

    private static bool IsLightWindow(Window window)
    {
        if (window == null) return false;
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color c = brush.Color;
        return c.R > 180 && c.G > 180 && c.B > 180;
    }

    private static Style CreateButtonStyle(bool light)
    {
        Color background = light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 110, 180);
        Color border = light ? Color.FromRgb(32, 68, 125) : Color.FromRgb(90, 140, 205);
        Style style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(background)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14, 7, 14, 7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        return style;
    }

    private static Style CreateTextBoxStyle(bool light)
    {
        Style style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style style = new Style(typeof(DatePicker));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateCheckBoxStyle(bool light)
    {
        Style style = new Style(typeof(CheckBox));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 4, 0, 4)));
        return style;
    }

    private static Style CreateRadioButtonStyle(bool light)
    {
        Style style = new Style(typeof(RadioButton));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 3, 0, 3)));
        return style;
    }
}
