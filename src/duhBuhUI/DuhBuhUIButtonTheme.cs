using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

public static class DuhBuhUIButtonTheme
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;
        window.Resources[typeof(Button)] = CreateButtonStyle(light);
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        Apply(window, IsLightWindow(window));
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Style CreateButtonStyle(bool light)
    {
        Color normal = light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 110, 180);
        Color hover = light ? Color.FromRgb(58, 108, 184) : Color.FromRgb(72, 128, 198);
        Color pressed = light ? Color.FromRgb(34, 72, 132) : Color.FromRgb(44, 88, 150);
        Color disabled = light ? Color.FromRgb(190, 195, 204) : Color.FromRgb(70, 74, 82);
        Color normalBorder = light ? Color.FromRgb(32, 68, 125) : Color.FromRgb(90, 140, 205);
        Color accentBorder = Color.FromRgb(224, 166, 52);

        Style style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normal)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(normalBorder)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16, 7, 16, 7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.WidthProperty, 104.0));
        style.Setters.Add(new Setter(Control.HeightProperty, 38.0));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateButtonTemplate()));

        Trigger mouseOver = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        mouseOver.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hover)));
        mouseOver.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accentBorder)));
        style.Triggers.Add(mouseOver);

        Trigger pressedTrigger = new Trigger { Property = ButtonBase.IsPressedProperty, Value = true };
        pressedTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brush(pressed)));
        pressedTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accentBorder)));
        style.Triggers.Add(pressedTrigger);

        Trigger disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabledTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brush(disabled)));
        disabledTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Color.FromRgb(150, 154, 162))));
        disabledTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(disabled)));
        style.Triggers.Add(disabledTrigger);

        return style;
    }

    private static ControlTemplate CreateButtonTemplate()
    {
        ControlTemplate template = new ControlTemplate(typeof(Button));
        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, TemplatedBinding("Background"));
        border.SetBinding(Border.BorderBrushProperty, TemplatedBinding("BorderBrush"));
        border.SetBinding(Border.BorderThicknessProperty, TemplatedBinding("BorderThickness"));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(7));
        border.SetValue(Border.SnapsToDevicePixelsProperty, true);

        FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.ContentProperty, TemplatedBinding("Content"));
        presenter.SetBinding(ContentPresenter.ContentTemplateProperty, TemplatedBinding("ContentTemplate"));
        presenter.SetBinding(ContentPresenter.ContentStringFormatProperty, TemplatedBinding("ContentStringFormat"));
        presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, TemplatedBinding("HorizontalContentAlignment"));
        presenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, TemplatedBinding("VerticalContentAlignment"));
        presenter.SetBinding(ContentPresenter.MarginProperty, TemplatedBinding("Padding"));
        presenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        border.AppendChild(presenter);

        template.VisualTree = border;
        return template;
    }

    private static Binding TemplatedBinding(string path)
    {
        return new Binding(path)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
        };
    }

    private static bool IsLightWindow(Window window)
    {
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color color = brush.Color;
        return color.R > 180 && color.G > 180 && color.B > 180;
    }
}
