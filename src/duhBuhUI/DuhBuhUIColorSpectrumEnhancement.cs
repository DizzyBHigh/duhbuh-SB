using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

public static class DuhBuhUIColorSpectrumEnhancement
{
    private static readonly FieldInfo PopupField = typeof(DuhBuhUICustomColorPicker).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly HashSet<DuhBuhUICustomColorPicker> Attached = new HashSet<DuhBuhUICustomColorPicker>();

    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(DuhBuhUICustomColorPicker), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded), true);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        DuhBuhUICustomColorPicker picker = sender as DuhBuhUICustomColorPicker;
        if (picker == null || Attached.Contains(picker)) return;
        Attached.Add(picker);
        picker.PreviewMouseLeftButtonDown += delegate { ScheduleAttach(picker); };
        picker.PreviewKeyDown += delegate { ScheduleAttach(picker); };
    }

    private static void ScheduleAttach(DuhBuhUICustomColorPicker picker)
    {
        // The picker creates its Popup from its mouse/key handler, which runs after
        // PreviewMouseLeftButtonDown/PreviewKeyDown. The old Input-priority callback
        // could therefore run before the Popup existed. Poll briefly at ContextIdle
        // so the enhancement attaches after the Popup has actually been created.
        int attempts = 0;
        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromMilliseconds(25)
        };
        timer.Tick += delegate
        {
            attempts++;
            if (Attach(picker) || attempts >= 20)
                timer.Stop();
        };
        timer.Start();
    }

    private static bool Attach(DuhBuhUICustomColorPicker picker)
    {
        if (PopupField == null) return false;
        Popup popup = PopupField.GetValue(picker) as Popup;
        Border surface = popup == null ? null : popup.Child as Border;
        StackPanel root = surface == null ? null : surface.Child as StackPanel;
        if (popup == null || !popup.IsOpen || root == null) return false;
        if (root.Tag as string == "spectrum") return true;
        root.Tag = "spectrum";

        Color c; if (!TryColor(picker.SelectedColor, out c)) c = Colors.White;
        double h, s, v; RgbHsv(c, out h, out s, out v);
        Grid grid = new Grid { Height = 142, Margin = new Thickness(0, 0, 0, 10) };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        Spectrum spectrum = new Spectrum { Hue = h, Saturation = s, Value = v, Margin = new Thickness(0, 0, 7, 0) };
        HueBar hue = new HueBar { Hue = h };
        Grid.SetColumn(hue, 1);
        grid.Children.Add(spectrum);
        grid.Children.Add(hue);
        root.Children.Insert(2, new TextBlock { Text = "Custom colour", FontSize = 11, Foreground = picker.Foreground, Margin = new Thickness(0, 0, 0, 5) });
        root.Children.Insert(3, grid);

        Action apply = delegate
        {
            Color chosen = HsvRgb(hue.Hue, spectrum.Saturation, spectrum.Value, c.A);
            picker.SelectedColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", chosen.A, chosen.R, chosen.G, chosen.B);
            spectrum.Hue = hue.Hue;
            spectrum.InvalidateVisual();
        };
        spectrum.Changed += delegate(double ss, double vv) { spectrum.Saturation = ss; spectrum.Value = vv; apply(); };
        hue.Changed += delegate(double hh) { hue.Hue = hh; apply(); };
        return true;
    }

    private static bool TryColor(string text, out Color color)
    {
        color = Colors.White;
        try
        {
            object value = ColorConverter.ConvertFromString(text);
            if (value is Color) { color = (Color)value; return true; }
        }
        catch { }
        return false;
    }

    private static void RgbHsv(Color c, out double h, out double s, out double v)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
        v = max;
        s = max == 0 ? 0 : d / max;
        if (d == 0) { h = 0; return; }
        if (max == r) h = 60 * ((g - b) / d % 6);
        else if (max == g) h = 60 * ((b - r) / d + 2);
        else h = 60 * ((r - g) / d + 4);
        if (h < 0) h += 360;
    }

    private static Color HsvRgb(double h, double s, double v, byte a)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Max(0, Math.Min(1, s));
        v = Math.Max(0, Math.Min(1, v));
        double c = v * s, x = c * (1 - Math.Abs(h / 60 % 2 - 1)), m = v - c;
        double r = 0, g = 0, b = 0;
        if (h < 60) { r = c; g = x; }
        else if (h < 120) { r = x; g = c; }
        else if (h < 180) { g = c; b = x; }
        else if (h < 240) { g = x; b = c; }
        else if (h < 300) { r = x; b = c; }
        else { r = c; b = x; }
        return Color.FromArgb(a, B((r + m) * 255), B((g + m) * 255), B((b + m) * 255));
    }

    private static byte B(double x) { return (byte)Math.Max(0, Math.Min(255, Math.Round(x))); }

    private sealed class Spectrum : FrameworkElement
    {
        public double Hue, Saturation, Value;
        public event Action<double, double> Changed;
        private bool drag;

        protected override void OnRender(DrawingContext d)
        {
            double w = Math.Max(1, ActualWidth), h = Math.Max(1, ActualHeight);
            Color hc = HsvRgb(Hue, 1, 1, 255);
            d.DrawRectangle(new SolidColorBrush(hc), null, new Rect(0, 0, w, h));
            d.DrawRectangle(new LinearGradientBrush(Colors.White, Color.FromArgb(0, 255, 255, 255), 0), null, new Rect(0, 0, w, h));
            d.DrawRectangle(new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), Colors.Black, 90), null, new Rect(0, 0, w, h));
            double x = Saturation * w, y = (1 - Value) * h;
            d.DrawEllipse(null, new Pen(Brushes.Black, 3), new Point(x, y), 7, 7);
            d.DrawEllipse(null, new Pen(Brushes.White, 2), new Point(x, y), 5, 5);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { drag = true; CaptureMouse(); Pick(e.GetPosition(this)); e.Handled = true; }
        protected override void OnMouseMove(MouseEventArgs e) { if (drag) Pick(e.GetPosition(this)); }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { if (drag) { Pick(e.GetPosition(this)); drag = false; ReleaseMouseCapture(); e.Handled = true; } }

        private void Pick(Point p)
        {
            Saturation = Math.Max(0, Math.Min(1, p.X / Math.Max(1, ActualWidth)));
            Value = 1 - Math.Max(0, Math.Min(1, p.Y / Math.Max(1, ActualHeight)));
            InvalidateVisual();
            if (Changed != null) Changed(Saturation, Value);
        }
    }

    private sealed class HueBar : FrameworkElement
    {
        public double Hue;
        public event Action<double> Changed;
        private bool drag;

        protected override void OnRender(DrawingContext d)
        {
            LinearGradientBrush b = new LinearGradientBrush();
            b.StartPoint = new Point(.5, 0);
            b.EndPoint = new Point(.5, 1);
            b.GradientStops.Add(new GradientStop(Colors.Red, 0));
            b.GradientStops.Add(new GradientStop(Colors.Magenta, 1.0 / 6));
            b.GradientStops.Add(new GradientStop(Colors.Blue, 2.0 / 6));
            b.GradientStops.Add(new GradientStop(Colors.Cyan, 3.0 / 6));
            b.GradientStops.Add(new GradientStop(Colors.Lime, 4.0 / 6));
            b.GradientStops.Add(new GradientStop(Colors.Yellow, 5.0 / 6));
            b.GradientStops.Add(new GradientStop(Colors.Red, 1));
            d.DrawRectangle(b, null, new Rect(0, 0, ActualWidth, ActualHeight));
            double y = Hue / 360 * Math.Max(1, ActualHeight);
            d.DrawLine(new Pen(Brushes.Black, 3), new Point(0, y), new Point(ActualWidth, y));
            d.DrawLine(new Pen(Brushes.White, 1), new Point(0, y), new Point(ActualWidth, y));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { drag = true; CaptureMouse(); Pick(e.GetPosition(this)); e.Handled = true; }
        protected override void OnMouseMove(MouseEventArgs e) { if (drag) Pick(e.GetPosition(this)); }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { if (drag) { Pick(e.GetPosition(this)); drag = false; ReleaseMouseCapture(); e.Handled = true; } }

        private void Pick(Point p)
        {
            Hue = Math.Max(0, Math.Min(359.999, p.Y / Math.Max(1, ActualHeight) * 360));
            InvalidateVisual();
            if (Changed != null) Changed(Hue);
        }
    }
}
