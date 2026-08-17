using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

public static class DuhBuhUISliderStyler
{
    private static readonly object Sync = new object();
    private static bool _initialized;

    [ModuleInitializer]
    public static void Initialize()
    {
        lock (Sync)
        {
            if (_initialized) return;
            _initialized = true;
            EventManager.RegisterClassHandler(typeof(Slider), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnSliderLoaded));
        }
    }

    private static void OnSliderLoaded(object sender, RoutedEventArgs e)
    {
        Slider slider = sender as Slider;
        if (slider != null) Apply(slider);
    }

    public static void Apply(Slider slider)
    {
        if (slider == null) return;
        slider.Height = 28;
        slider.MinHeight = 28;
        slider.Margin = new Thickness(3, 5, 3, 5);
        slider.VerticalAlignment = VerticalAlignment.Center;
        slider.Template = CreateTemplate();
    }

    private static ControlTemplate CreateTemplate()
    {
        Color trackColor = Color.FromRgb(55, 59, 67);
        Color accent = Color.FromRgb(224, 166, 52);
        Color thumbBorder = Color.FromRgb(255, 255, 255);

        ControlTemplate template = new ControlTemplate(typeof(Slider));
        FrameworkElementFactory root = new FrameworkElementFactory(typeof(Grid));

        FrameworkElementFactory track = new FrameworkElementFactory(typeof(Track));
        track.SetValue(FrameworkElement.NameProperty, "PART_Track");
        track.SetValue(Track.OrientationProperty, Orientation.Horizontal);
        track.SetValue(Track.IsDirectionReversedProperty, false);
        track.SetBinding(Track.MinimumProperty, new Binding("Minimum") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        track.SetBinding(Track.MaximumProperty, new Binding("Maximum") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        track.SetBinding(Track.ValueProperty, new Binding("Value") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Mode = BindingMode.TwoWay });
        track.SetBinding(Track.ViewportSizeProperty, new Binding("ViewportSize") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });

        FrameworkElementFactory decrease = new FrameworkElementFactory(typeof(RepeatButton));
        decrease.SetValue(ButtonBase.CommandProperty, Slider.DecreaseLarge);
        decrease.SetValue(Control.BackgroundProperty, Brush(accent));
        decrease.SetValue(Control.BorderThicknessProperty, new Thickness(0));
        decrease.SetValue(Control.HeightProperty, 5.0);
        decrease.SetValue(Control.TemplateProperty, SimpleBarTemplate(accent, new CornerRadius(3, 0, 0, 3)));
        track.SetValue(Track.DecreaseRepeatButtonProperty, null);
        track.AppendChild(decrease);

        FrameworkElementFactory increase = new FrameworkElementFactory(typeof(RepeatButton));
        increase.SetValue(ButtonBase.CommandProperty, Slider.IncreaseLarge);
        increase.SetValue(Control.BackgroundProperty, Brush(trackColor));
        increase.SetValue(Control.BorderThicknessProperty, new Thickness(0));
        increase.SetValue(Control.HeightProperty, 5.0);
        increase.SetValue(Control.TemplateProperty, SimpleBarTemplate(trackColor, new CornerRadius(0, 3, 3, 0)));
        track.AppendChild(increase);

        FrameworkElementFactory thumb = new FrameworkElementFactory(typeof(Thumb));
        thumb.SetValue(Control.WidthProperty, 14.0);
        thumb.SetValue(Control.HeightProperty, 22.0);
        thumb.SetValue(Control.BackgroundProperty, Brush(accent));
        thumb.SetValue(Control.BorderBrushProperty, Brush(thumbBorder));
        thumb.SetValue(Control.BorderThicknessProperty, new Thickness(1));
        thumb.SetValue(Control.CursorProperty, System.Windows.Input.Cursors.Hand);
        thumb.SetValue(Control.TemplateProperty, SimpleThumbTemplate(accent, thumbBorder));
        track.SetValue(Track.ThumbProperty, null);
        track.AppendChild(thumb);

        root.AppendChild(track);
        template.VisualTree = root;
        return template;
    }

    private static ControlTemplate SimpleBarTemplate(Color color, CornerRadius radius)
    {
        ControlTemplate t = new ControlTemplate(typeof(RepeatButton));
        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.BackgroundProperty, Brush(color));
        border.SetValue(Border.CornerRadiusProperty, radius);
        t.VisualTree = border;
        return t;
    }

    private static ControlTemplate SimpleThumbTemplate(Color background, Color borderColor)
    {
        ControlTemplate t = new ControlTemplate(typeof(Thumb));
        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.BackgroundProperty, Brush(background));
        border.SetValue(Border.BorderBrushProperty, Brush(borderColor));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        t.VisualTree = border;
        return t;
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
