using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public static class DuhBuhUISliderStyler
{
    private static readonly object Sync = new object();
    private static bool _initialized;

    public static void Initialize()
    {
        lock (Sync)
        {
            if (_initialized) return;
            _initialized = true;
        }
    }

    public static void Apply(Slider slider)
    {
        if (slider == null) return;

        slider.Height = 30;
        slider.MinHeight = 30;
        slider.Margin = new Thickness(3, 5, 3, 5);
        slider.VerticalAlignment = VerticalAlignment.Center;
        slider.Background = new SolidColorBrush(Color.FromRgb(55, 59, 67));
        slider.Foreground = new SolidColorBrush(Color.FromRgb(224, 166, 52));
        slider.BorderThickness = new Thickness(0);
        slider.Cursor = Cursors.Hand;

        // Keep WPF's native Slider interaction/state handling, but draw our own
        // visual track/thumb over it. This avoids relying on theme-specific Track
        // dependency properties that are not public in all .NET Framework builds.
        Grid host = new Grid
        {
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            Height = 24
        };

        Border inactive = new Border
        {
            Height = 5,
            Background = new SolidColorBrush(Color.FromRgb(55, 59, 67)),
            CornerRadius = new CornerRadius(3),
            VerticalAlignment = VerticalAlignment.Center
        };

        Border active = new Border
        {
            Height = 5,
            Background = new SolidColorBrush(Color.FromRgb(224, 166, 52)),
            CornerRadius = new CornerRadius(3, 0, 0, 3),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 0
        };

        Border thumb = new Border
        {
            Width = 16,
            Height = 24,
            Background = new SolidColorBrush(Color.FromRgb(224, 166, 52)),
            BorderBrush = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        host.Children.Add(inactive);
        host.Children.Add(active);
        host.Children.Add(thumb);

        // A native Slider cannot host an arbitrary overlay directly, so place the
        // visual layer in an Adorner-like popup window and keep it synchronized.
        // The overlay is non-interactive; the Slider remains the input surface.
        slider.Loaded += delegate
        {
            Window window = Window.GetWindow(slider);
            if (window == null) return;
            Panel parent = slider.Parent as Panel;
            if (parent == null) return;

            int index = parent.Children.IndexOf(slider);
            if (index < 0) return;
            parent.Children.Remove(slider);

            Grid wrapper = new Grid
            {
                Margin = slider.Margin,
                Height = slider.Height,
                VerticalAlignment = slider.VerticalAlignment
            };
            slider.Margin = new Thickness(0);
            wrapper.Children.Add(host);
            wrapper.Children.Add(slider);
            parent.Children.Insert(index, wrapper);

            Action update = delegate
            {
                double range = slider.Maximum - slider.Minimum;
                double ratio = range <= 0 ? 0 : (slider.Value - slider.Minimum) / range;
                ratio = Math.Max(0, Math.Min(1, ratio));
                double width = wrapper.ActualWidth;
                double thumbTravel = Math.Max(0, width - thumb.Width);
                double x = thumbTravel * ratio;
                thumb.Margin = new Thickness(x, 0, 0, 0);
                active.Width = Math.Max(0, x + thumb.Width / 2.0);
            };

            slider.SizeChanged += delegate { update(); };
            slider.ValueChanged += delegate { update(); };
            wrapper.SizeChanged += delegate { update(); };
            update();
        };
    }
}
