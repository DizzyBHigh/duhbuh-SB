// duhBuhUITheme - safe visual styling for the shared settings UI.
// IMPORTANT: Do not replace TabControl/TabItem templates or modify generated tab content.
// Streamer.bot's hosted WPF runtime can lose tab content when templates are replaced.

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
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;

        ResourceDictionary r = window.Resources;
        r[typeof(Button)] = CreateButtonStyle(light);
        r[typeof(TextBox)] = CreateTextBoxStyle(light);
        r[typeof(ComboBox)] = CreateComboBoxStyle(light);
        r[typeof(DatePicker)] = CreateDatePickerStyle(light);

        r["duhBuhSectionBackground"] = new SolidColorBrush(
            light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(37, 40, 46));
        r["duhBuhSectionBorder"] = new SolidColorBrush(
            light ? Color.FromRgb(220, 223, 229) : Color.FromRgb(58, 62, 70));
        r["duhBuhAccent"] = new SolidColorBrush(
            light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 140, 205));
        r["duhBuhSectionText"] = new SolidColorBrush(
            light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
        r["duhBuhDescriptionText"] = new SolidColorBrush(
            light ? Color.FromRgb(100, 106, 118) : Color.FromRgb(160, 167, 178));

        // Style only the already-created field groups. We never replace templates
        // and we never alter TabControl/TabItem content.
        window.AddHandler(
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnElementLoaded));
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        FrameworkElement element = e.OriginalSource as FrameworkElement;
        if (element == null) return;

        StackPanel panel = element as StackPanel;
        if (panel == null) return;

        // FieldBox() creates small StackPanels containing a label/description and
        // one or more controls. Category/root panels contain many children and are
        // intentionally left alone.
        if (panel.Children.Count >= 2 && panel.Children.Count <= 4 && IsFieldPanel(panel))
        {
            bool light = IsLightWindow(Window.GetWindow(panel));
            panel.Background = new SolidColorBrush(
                light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(37, 40, 46));
            panel.Margin = new Thickness(0, 0, 0, 10);
            panel.Padding = new Thickness(12, 10, 12, 10);
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
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 6, 12, 6)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        return style;
    }

    private static Style CreateTextBoxStyle(bool light)
    {
        Style style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty,
            new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty,
            new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty,
            new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty,
            new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style style = new Style(typeof(DatePicker));
        style.Setters.Add(new Setter(Control.BackgroundProperty,
            new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty,
            new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty,
            new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }
}
