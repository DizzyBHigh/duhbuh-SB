using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

// Reusable checkbox/toggle visuals. Native CheckBox supplies input/state;
// the template is entirely built from basic WPF primitives so the visual
// appearance does not depend on the stock WPF checkbox theme.
public static class DuhBuhUICheckBoxStyler
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(CheckBox), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnCheckBoxLoaded));
    }

    public static void Apply(CheckBox checkBox)
    {
        if (checkBox == null) return;
        Window window = Window.GetWindow(checkBox);
        bool light = IsLightWindow(window);
        SolidColorBrush accent = Brush(Color.FromRgb(224, 166, 52));
        SolidColorBrush border = Brush(light ? Color.FromRgb(145, 150, 160) : Color.FromRgb(75, 80, 90));
        SolidColorBrush off = Brush(light ? Color.FromRgb(238, 239, 242) : Color.FromRgb(52, 56, 64));
        SolidColorBrush text = Brush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245));

        checkBox.Foreground = text;
        checkBox.Focusable = true;
        checkBox.Padding = new Thickness(0);
        checkBox.Margin = new Thickness(0, 5, 0, 2);
        checkBox.FontSize = 15;

        if (checkBox.Tag != null)
            checkBox.Template = CreateToggleTemplate(accent, border, off, text);
        else
            checkBox.Template = CreateCheckBoxTemplate(accent, border, off, text);
    }

    private static void OnCheckBoxLoaded(object sender, RoutedEventArgs e)
    {
        Apply(sender as CheckBox);
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Binding TemplatedBinding(string path)
    {
        return new Binding(path)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
            Mode = BindingMode.OneWay
        };
    }

    private static ControlTemplate CreateCheckBoxTemplate(SolidColorBrush accent, SolidColorBrush border, SolidColorBrush off, SolidColorBrush text)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory box = new FrameworkElementFactory(typeof(Border));
        box.SetValue(FrameworkElement.NameProperty, "box");
        box.SetValue(Border.WidthProperty, 21.0);
        box.SetValue(Border.HeightProperty, 21.0);
        box.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        box.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        box.SetValue(Border.BorderBrushProperty, border);
        box.SetValue(Border.BackgroundProperty, off);
        box.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 10, 0));
        box.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
        check.SetValue(FrameworkElement.NameProperty, "check");
        check.SetValue(TextBlock.TextProperty, "✓");
        check.SetValue(TextBlock.FontSizeProperty, 15.0);
        check.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        check.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        check.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        check.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        check.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
        box.AppendChild(check);
        panel.AppendChild(box);

        FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, TemplatedBinding("Content"));
        label.SetBinding(TextBlock.ForegroundProperty, TemplatedBinding("Foreground"));
        label.SetValue(TextBlock.FontSizeProperty, 15.0);
        label.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        panel.AppendChild(label);

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

    private static ControlTemplate CreateToggleTemplate(SolidColorBrush accent, SolidColorBrush border, SolidColorBrush off, SolidColorBrush text)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory track = new FrameworkElementFactory(typeof(Border));
        track.SetValue(FrameworkElement.NameProperty, "track");
        track.SetValue(Border.WidthProperty, 54.0);
        track.SetValue(Border.HeightProperty, 30.0);
        track.SetValue(Border.CornerRadiusProperty, new CornerRadius(15));
        track.SetValue(Border.BackgroundProperty, off);
        track.SetValue(Border.BorderBrushProperty, border);
        track.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        track.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 11, 0));
        track.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory thumb = new FrameworkElementFactory(typeof(Border));
        thumb.SetValue(FrameworkElement.NameProperty, "thumb");
        thumb.SetValue(Border.WidthProperty, 22.0);
        thumb.SetValue(Border.HeightProperty, 22.0);
        thumb.SetValue(Border.CornerRadiusProperty, new CornerRadius(11));
        thumb.SetValue(Border.BackgroundProperty, Brushes.White);
        thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(3));
        track.AppendChild(thumb);
        panel.AppendChild(track);

        FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, TemplatedBinding("Content"));
        label.SetBinding(TextBlock.ForegroundProperty, TemplatedBinding("Foreground"));
        label.SetValue(TextBlock.FontSizeProperty, 15.0);
        label.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        panel.AppendChild(label);

        template.VisualTree = panel;

        Trigger checkedTrigger = new Trigger { Property = ToggleButton.IsCheckedProperty, Value = true };
        checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, accent, "track"));
        checkedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, accent, "track"));
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

    private static bool IsLightWindow(Window window)
    {
        if (window == null) return false;
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color color = brush.Color;
        return color.R > 180 && color.G > 180 && color.B > 180;
    }
}
