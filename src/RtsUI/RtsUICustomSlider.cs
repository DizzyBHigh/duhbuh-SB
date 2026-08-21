using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public sealed class RtsUICustomSlider : FrameworkElement
{
    private double _minimum;
    private double _maximum = 100;
    private double _value;
    private bool _dragging;

    public event EventHandler ValueChanged;

    public double Minimum
    {
        get { return _minimum; }
        set { _minimum = value; if (_maximum < _minimum) _maximum = _minimum; SetValueInternal(_value); }
    }

    public double Maximum
    {
        get { return _maximum; }
        set { _maximum = value < _minimum ? _minimum : value; SetValueInternal(_value); }
    }

    public double Value
    {
        get { return _value; }
        set { SetValueInternal(value); }
    }

    public double TickFrequency { get; set; } = 1;

    public RtsUICustomSlider()
    {
        Height = 30;
        MinHeight = 30;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        Focusable = true;
        Cursor = Cursors.Hand;
        UseLayoutRounding = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 240 : availableSize.Width;
        return new Size(Math.Max(120, width), 30);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double width = ActualWidth;
        double height = ActualHeight;
        double trackHeight = 5;
        double thumbWidth = 16;
        double thumbHeight = 24;
        double trackY = (height - trackHeight) / 2.0;
        double thumbY = (height - thumbHeight) / 2.0;
        double travel = Math.Max(0, width - thumbWidth);
        double ratio = GetRatio();
        double thumbX = travel * ratio;
        double activeWidth = Math.Max(0, thumbX + thumbWidth / 2.0);

        Color trackColor = Color.FromRgb(220, 220, 220);
        Color accentColor = Color.FromRgb(224, 166, 52);
        Pen thumbBorder = new Pen(new SolidColorBrush(Colors.White), 1);

        // Fully custom rendering: no native WPF Slider track, thumb, border,
        // focus rectangle, or template is used.
        dc.DrawRoundedRectangle(new SolidColorBrush(trackColor), null, new Rect(0, trackY, width, trackHeight), 3, 3);
        if (activeWidth > 0)
            dc.DrawRoundedRectangle(new SolidColorBrush(accentColor), null, new Rect(0, trackY, activeWidth, trackHeight), 3, 3);

        Rect thumbRect = new Rect(thumbX, thumbY, thumbWidth, thumbHeight);
        dc.DrawRoundedRectangle(new SolidColorBrush(accentColor), thumbBorder, thumbRect, 3, 3);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        _dragging = true;
        CaptureMouse();
        SetFromMouse(e.GetPosition(this).X);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging) SetFromMouse(e.GetPosition(this).X);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_dragging)
        {
            _dragging = false;
            ReleaseMouseCapture();
            SetFromMouse(e.GetPosition(this).X);
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        double step = TickFrequency > 0 ? TickFrequency : Math.Max(1, (_maximum - _minimum) / 100.0);
        if (e.Key == Key.Left || e.Key == Key.Down) { SetValueInternal(_value - step); e.Handled = true; }
        else if (e.Key == Key.Right || e.Key == Key.Up) { SetValueInternal(_value + step); e.Handled = true; }
        else if (e.Key == Key.Home) { SetValueInternal(_minimum); e.Handled = true; }
        else if (e.Key == Key.End) { SetValueInternal(_maximum); e.Handled = true; }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    private void SetFromMouse(double x)
    {
        double travel = Math.Max(1, ActualWidth - 16);
        double ratio = Math.Max(0, Math.Min(1, (x - 8) / travel));
        SetValueInternal(_minimum + ((_maximum - _minimum) * ratio));
    }

    private void SetValueInternal(double value)
    {
        double old = _value;
        if (double.IsNaN(value) || double.IsInfinity(value)) value = _minimum;
        value = Math.Max(_minimum, Math.Min(_maximum, value));
        if (TickFrequency > 0)
            value = _minimum + Math.Round((value - _minimum) / TickFrequency) * TickFrequency;
        value = Math.Max(_minimum, Math.Min(_maximum, value));
        _value = value;
        InvalidateVisual();
        if (Math.Abs(old - _value) > 0.000001 && ValueChanged != null) ValueChanged(this, EventArgs.Empty);
    }

    private double GetRatio()
    {
        double range = _maximum - _minimum;
        return range <= 0 ? 0 : Math.Max(0, Math.Min(1, (_value - _minimum) / range));
    }
}
