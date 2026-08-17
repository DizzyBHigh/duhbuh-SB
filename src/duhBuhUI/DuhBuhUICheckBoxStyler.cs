using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Markup;

public static class DuhBuhUICheckBoxStyler
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(CheckBox), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnCheckBoxLoaded));
    }

    private static void OnCheckBoxLoaded(object sender, RoutedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        if (checkBox == null) return;

        Window window = Window.GetWindow(checkBox);
        if (window == null) return;

        SolidColorBrush accent = GetBrush(window, "duhBuhAccent", Color.FromRgb(224, 166, 52));
        SolidColorBrush border = GetBrush(window, "duhBuhSectionBorder", Color.FromRgb(75, 80, 90));
        bool isToggle = checkBox.Tag != null;

        if (isToggle)
        {
            checkBox.Template = CreateToggleTemplate(accent, border);
            checkBox.Padding = new Thickness(0);
            checkBox.Margin = new Thickness(0, 5, 0, 2);
        }
        else
        {
            checkBox.Template = CreateCheckBoxTemplate(accent, border);
        }
    }

    private static SolidColorBrush GetBrush(FrameworkElement element, string key, Color fallback)
    {
        object value = null;
        if (element != null && element.Resources != null && element.Resources.Contains(key))
            value = element.Resources[key];

        Window window = element as Window;
        if (value == null && window != null && window.Resources != null && window.Resources.Contains(key))
            value = window.Resources[key];

        SolidColorBrush brush = value as SolidColorBrush;
        if (brush != null) return brush;
        return Brush(fallback);
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static ControlTemplate CreateCheckBoxTemplate(SolidColorBrush accent, SolidColorBrush border)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory box = new FrameworkElementFactory(typeof(Border));
        box.SetValue(Border.WidthProperty, 16.0);
        box.SetValue(Border.HeightProperty, 16.0);
        box.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        box.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        box.SetValue(Border.BorderBrushProperty, border);
        box.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        box.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 8, 0));
        box.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
        check.SetValue(TextBlock.TextProperty, "✓");
        check.SetValue(TextBlock.FontSizeProperty, 12.0);
        check.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        check.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        check.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        check.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        check.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
        box.AppendChild(check);
        panel.AppendChild(box);

        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetBinding(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
        content.SetBinding(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
        content.SetBinding(ContentPresenter.ContentStringFormatProperty, new TemplateBindingExtension(ContentControl.ContentStringFormatProperty));
        content.SetBinding(ContentPresenter.ForegroundProperty, new TemplateBindingExtension(Control.ForegroundProperty));
        content.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
        content.SetBinding(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Control.VerticalContentAlignmentProperty));
        panel.AppendChild(content);

        template.VisualTree = panel;

        Trigger checkedTrigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
        checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, accent, "box"));
        checkedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, accent, "box"));
        checkedTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "check"));
        template.Triggers.Add(checkedTrigger);

        Trigger hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, accent, "box"));
        template.Triggers.Add(hoverTrigger);

        Trigger disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.55));
        template.Triggers.Add(disabledTrigger);

        return template;
    }

    private static ControlTemplate CreateToggleTemplate(SolidColorBrush accent, SolidColorBrush border)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory track = new FrameworkElementFactory(typeof(Border));
        track.SetValue(Border.WidthProperty, 42.0);
        track.SetValue(Border.HeightProperty, 22.0);
        track.SetValue(Border.CornerRadiusProperty, new CornerRadius(11));
        track.SetValue(Border.BackgroundProperty, Brush(Color.FromRgb(68, 73, 82)));
        track.SetValue(Border.BorderBrushProperty, border);
        track.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        track.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 9, 0));
        track.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory thumb = new FrameworkElementFactory(typeof(Border));
        thumb.SetValue(Border.WidthProperty, 16.0);
        thumb.SetValue(Border.HeightProperty, 16.0);
        thumb.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        thumb.SetValue(Border.BackgroundProperty, Brushes.White);
        thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
        track.AppendChild(thumb);
        panel.AppendChild(track);

        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetBinding(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
        content.SetBinding(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
        content.SetBinding(ContentPresenter.ContentStringFormatProperty, new TemplateBindingExtension(ContentControl.ContentStringFormatProperty));
        content.SetBinding(ContentPresenter.ForegroundProperty, new TemplateBindingExtension(Control.ForegroundProperty));
        content.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
        content.SetBinding(ContentPresenter.VerticalAlignmentProperty, new TemplateBindingExtension(Control.VerticalContentAlignmentProperty));
        panel.AppendChild(content);

        template.VisualTree = panel;

        Trigger checkedTrigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
        checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, accent, "track"));
        checkedTrigger.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right, "thumb"));
        template.Triggers.Add(checkedTrigger);

        Trigger hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, accent, "track"));
        template.Triggers.Add(hoverTrigger);

        Trigger disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabledTrigger.Setters.Add(new Setter(UIElement.OpacityProperty, 0.55));
        template.Triggers.Add(disabledTrigger);

        return template;
    }
}
