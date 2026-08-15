// duhBuhUITheme - safe visual styling for the shared settings UI.
// This version deliberately avoids replacing TabControl/TabItem templates because
// WPF template replacement can hide TabItem content in Streamer.bot's hosted runtime.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DuhBuhUITheme
{
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered) return;
        _registered = true;
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;

        bool light = IsLightWindow(window);
        ResourceDictionary resources = window.Resources;
        resources["duhBuhButtonStyle"] = CreateButtonStyle(light);
        resources["duhBuhTextBoxStyle"] = CreateTextBoxStyle(light);
        resources["duhBuhComboBoxStyle"] = CreateComboBoxStyle(light);
        resources["duhBuhDatePickerStyle"] = CreateDatePickerStyle(light);

        ApplyStyleToTree(window, light);
    }

    private static bool IsLightWindow(Window window)
    {
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color c = brush.Color;
        return c.R > 180 && c.G > 180 && c.B > 180;
    }

    private static void ApplyStyleToTree(DependencyObject root, bool light)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            Control control = child as Control;
            if (control != null)
            {
                if (control is Button) control.Style = (Style)FindResource(root, "duhBuhButtonStyle");
                else if (control is TextBox) control.Style = (Style)FindResource(root, "duhBuhTextBoxStyle");
                else if (control is ComboBox) control.Style = (Style)FindResource(root, "duhBuhComboBoxStyle");
                else if (control is DatePicker) control.Style = (Style)FindResource(root, "duhBuhDatePickerStyle");
            }
            ApplyStyleToTree(child, light);
        }
    }

    private static object FindResource(DependencyObject root, object key)
    {
        FrameworkElement element = root as FrameworkElement;
        if (element != null)
        {
            object value = element.TryFindResource(key);
            if (value != null) return value;
        }
        return null;
    }

    private static Style CreateButtonStyle(bool light)
    {
        Color background = light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 110, 180);
        Color hover = light ? Color.FromRgb(58, 110, 185) : Color.FromRgb(78, 135, 210);
        Color pressed = light ? Color.FromRgb(35, 72, 128) : Color.FromRgb(45, 88, 145);
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
        style.Setters.Add(new Setter(Control.TemplateProperty, RoundedButtonTemplate(background, hover, pressed, border)));
        return style;
    }

    private static ControlTemplate RoundedButtonTemplate(Color background, Color hover, Color pressed, Color border)
    {
        ControlTemplate template = new ControlTemplate(typeof(Button));
        FrameworkElementFactory outer = new FrameworkElementFactory(typeof(Border));
        outer.Name = "ButtonBorder";
        outer.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        outer.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        outer.SetValue(Border.BackgroundProperty, new SolidColorBrush(background));
        outer.SetValue(Border.BorderBrushProperty, new SolidColorBrush(border));

        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(ContentPresenter.MarginProperty, new Thickness(2));
        content.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        outer.AppendChild(content);
        template.VisualTree = outer;

        Trigger over = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
        over.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hover), "ButtonBorder"));
        template.Triggers.Add(over);
        Trigger down = new Trigger { Property = Button.IsPressedProperty, Value = true };
        down.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(pressed), "ButtonBorder"));
        template.Triggers.Add(down);
        Trigger disabled = new Trigger { Property = Button.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty, 0.45));
        template.Triggers.Add(disabled);
        return template;
    }

    private static Style CreateTextBoxStyle(bool light)
    {
        Style style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 6, 9, 6)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Resources.Add(typeof(Border), RoundedBorderStyle(7));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Resources.Add(typeof(Border), RoundedBorderStyle(7));
        return style;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style style = new Style(typeof(DatePicker));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Resources.Add(typeof(Border), RoundedBorderStyle(7));
        return style;
    }

    private static Style RoundedBorderStyle(double radius)
    {
        Style style = new Style(typeof(Border));
        style.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(radius)));
        return style;
    }
}
