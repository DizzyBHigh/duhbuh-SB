// duhBuhUITheme - visual styling for the shared settings UI.
// Loaded by DuhBuhUIBannerAssets so existing actions do not need another code change.

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
        resources["duhBuhComboBoxItemStyle"] = CreateComboBoxItemStyle(light);
        resources["duhBuhCheckBoxStyle"] = CreateCheckBoxStyle(light);
        resources["duhBuhSliderStyle"] = CreateSliderStyle(light);
        resources["duhBuhTabControlStyle"] = CreateTabControlStyle(light);
        resources["duhBuhTabItemStyle"] = CreateTabItemStyle(light);
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
                else if (control is ComboBoxItem) control.Style = (Style)FindResource(root, "duhBuhComboBoxItemStyle");
                else if (control is CheckBox) control.Style = (Style)FindResource(root, "duhBuhCheckBoxStyle");
                else if (control is Slider) control.Style = (Style)FindResource(root, "duhBuhSliderStyle");
                else if (control is DatePicker) control.Style = (Style)FindResource(root, "duhBuhDatePickerStyle");
                else if (control is TabControl) control.Style = (Style)FindResource(root, "duhBuhTabControlStyle");
                else if (control is TabItem) control.Style = (Style)FindResource(root, "duhBuhTabItemStyle");
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
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(15, 8, 15, 8)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
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
        outer.SetValue(Border.CornerRadiusProperty, new CornerRadius(9));
        outer.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        outer.SetValue(Border.BackgroundProperty, new SolidColorBrush(background));
        outer.SetValue(Border.BorderBrushProperty, new SolidColorBrush(border));
        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(ContentPresenter.MarginProperty, new Thickness(3, 1, 3, 1));
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
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 7, 10, 7)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Resources.Add(typeof(Border), RoundedBorderStyle(7));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(190, 197, 210) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 6, 10, 6)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 13.0));
        style.Resources.Add(typeof(ComboBoxItem), CreateComboBoxItemStyle(light));
        return style;
    }

    private static Style CreateComboBoxItemStyle(bool light)
    {
        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10, 7, 10, 7)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        return style;
    }

    private static Style CreateCheckBoxStyle(bool light)
    {
        Style style = new Style(typeof(CheckBox));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 14.0));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 3, 0, 2)));
        return style;
    }

    private static Style CreateSliderStyle(bool light)
    {
        Style style = new Style(typeof(Slider));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 6, 4, 3)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(90, 145, 215))));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 20.0));
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

    private static Style CreateTabControlStyle(bool light)
    {
        Style style = new Style(typeof(TabControl));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Color.FromRgb(246, 247, 249) : Color.FromRgb(30, 32, 37))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        return style;
    }

    private static Style CreateTabItemStyle(bool light)
    {
        Color selected = light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(43, 46, 53);
        Color normal = light ? Color.FromRgb(232, 234, 239) : Color.FromRgb(36, 39, 45);
        Color foreground = light ? Color.FromRgb(55, 58, 65) : Color.FromRgb(205, 210, 220);
        Color selectedForeground = light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(245, 247, 250);

        Style style = new Style(typeof(TabItem));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(foreground)));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(normal)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(15, 9, 15, 9)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(2, 2, 2, 0)));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 13.0));
        style.Setters.Add(new Setter(Control.TemplateProperty, RoundedTabTemplate(normal, selected, foreground, selectedForeground)));
        return style;
    }

    private static ControlTemplate RoundedTabTemplate(Color normal, Color selected, Color foreground, Color selectedForeground)
    {
        ControlTemplate template = new ControlTemplate(typeof(TabItem));
        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.Name = "TabBorder";
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(normal));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8, 8, 0, 0));
        border.SetValue(Border.PaddingProperty, new Thickness(14, 8, 14, 8));
        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(content);
        template.VisualTree = border;

        Trigger selectedTrigger = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
        selectedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(selected), "TabBorder"));
        selectedTrigger.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(selectedForeground)));
        template.Triggers.Add(selectedTrigger);

        Trigger disabled = new Trigger { Property = TabItem.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty, 0.45));
        template.Triggers.Add(disabled);
        return template;
    }

    private static Style RoundedBorderStyle(double radius)
    {
        Style style = new Style(typeof(Border));
        style.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(radius)));
        return style;
    }
}
