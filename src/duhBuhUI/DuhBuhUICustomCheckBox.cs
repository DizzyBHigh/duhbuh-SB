using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Shared custom checkbox host used by DuhBuhUI.
// Registered checkbox keys render as square checkboxes; normal toggle keys
// render as switches. This keeps the existing DuhBuhUI registration surface
// intact while giving the two controls genuinely different visuals.
public class CheckBox : Control
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        "IsChecked", typeof(bool?), typeof(CheckBox),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        "Content", typeof(object), typeof(CheckBox), new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public event RoutedEventHandler Checked;
    public event RoutedEventHandler Unchecked;
    private bool _focused;
    private bool _pressed;

    public bool? IsChecked { get { return (bool?)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }
    public object Content { get { return GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }
    public bool IsThreeState { get; set; }

    public CheckBox()
    {
        Focusable = true; IsTabStop = true; Cursor = Cursors.Hand;
        MinHeight = 30; Height = 30;
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        FontSize = 15;
    }

    private bool IsSquareCheckbox()
    {
        return DuhBuhUICheckBoxStyler.IsRegisteredCheckbox(Tag);
    }

    private static void OnIsCheckedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        CheckBox control = sender as CheckBox;
        if (control == null) return;
        control.InvalidateMeasure(); control.InvalidateVisual();
        bool oldValue = e.OldValue is bool && (bool)e.OldValue;
        bool newValue = e.NewValue is bool && (bool)e.NewValue;
        if (oldValue == newValue) return;
        RoutedEventHandler handler = newValue ? control.Checked : control.Unchecked;
        if (handler != null) handler(control, new RoutedEventArgs());
    }

    private static void OnVisualPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        CheckBox control = sender as CheckBox;
        if (control != null) { control.InvalidateMeasure(); control.InvalidateVisual(); }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        FormattedText formatted = Format(text);
        bool square = IsSquareCheckbox();
        double desiredWidth = (square ? 22 : 54) + 11 + formatted.Width;
        double desiredHeight = square ? Math.Max(26, formatted.Height + 4) : Math.Max(30, formatted.Height + 6);
        double width = double.IsPositiveInfinity(availableSize.Width) ? desiredWidth : Math.Min(desiredWidth, availableSize.Width);
        double height = double.IsPositiveInfinity(availableSize.Height) ? desiredHeight : Math.Min(desiredHeight, availableSize.Height);
        return new Size(Math.Max(square ? 22 : 54, width), Math.Max(square ? 26 : 30, height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (IsSquareCheckbox()) RenderCheckbox(dc);
        else RenderToggle(dc);
    }

    private void RenderCheckbox(DrawingContext dc)
    {
        bool isChecked = IsChecked == true;
        Color accent = Color.FromRgb(224, 166, 52);
        Color off = Color.FromRgb(52, 56, 64);
        Color edge = _focused || _pressed ? accent : Color.FromRgb(75, 80, 90);
        Color textColor = Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245);

        double top = Math.Max(1, (ActualHeight - 21) / 2);
        Rect box = new Rect(1, top, 21, 21);
        dc.DrawRoundedRectangle(new SolidColorBrush(isChecked ? accent : off), new Pen(new SolidColorBrush(edge), 1.5), box, 3, 3);

        if (isChecked)
        {
            Pen tickPen = new Pen(Brushes.White, 2.2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            StreamGeometry tick = new StreamGeometry();
            using (StreamGeometryContext ctx = tick.Open())
            {
                ctx.BeginFigure(new Point(5.5, top + 10.5), false, false);
                ctx.LineTo(new Point(9.2, top + 14.2), true, false);
                ctx.LineTo(new Point(16.8, top + 6.2), true, false);
            }
            tick.Freeze(); dc.DrawGeometry(null, tickPen, tick);
        }

        DrawText(dc, textColor, 32);
    }

    private void RenderToggle(DrawingContext dc)
    {
        bool isChecked = IsChecked == true;
        Color offTrack = Color.FromRgb(52, 56, 64);
        Color onTrack = Color.FromRgb(224, 166, 52);
        Color edge = _focused || _pressed ? Color.FromRgb(224, 166, 52) : Color.FromRgb(75, 80, 90);
        Color textColor = Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245);

        Rect track = new Rect(1, Math.Max(0, (ActualHeight - 30) / 2), 54, 30);
        dc.DrawRoundedRectangle(new SolidColorBrush(isChecked ? onTrack : offTrack), new Pen(new SolidColorBrush(edge), 1), track, 15, 15);
        Rect thumb = isChecked ? new Rect(track.Right - 25, track.Top + 4, 22, 22) : new Rect(track.Left + 3, track.Top + 4, 22, 22);
        dc.DrawEllipse(Brushes.White, null, new Point(thumb.Left + 11, thumb.Top + 11), 11, 11);
        DrawText(dc, textColor, 66);
    }

    private void DrawText(DrawingContext dc, Color color, double x)
    {
        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        if (string.IsNullOrEmpty(text)) return;
        FormattedText formatted = Format(text, color);
        dc.DrawText(formatted, new Point(x, Math.Max(0, (ActualHeight - formatted.Height) / 2)));
    }

    private FormattedText Format(string value) { return Format(value, Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245)); }
    private FormattedText Format(string value, Color color)
    {
        return new FormattedText(value ?? "", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), FontSize, new SolidColorBrush(color), VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    protected override void OnGotFocus(RoutedEventArgs e) { _focused = true; InvalidateVisual(); base.OnGotFocus(e); }
    protected override void OnLostFocus(RoutedEventArgs e) { _focused = false; _pressed = false; InvalidateVisual(); base.OnLostFocus(e); }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { Focus(); _pressed = true; CaptureMouse(); InvalidateVisual(); e.Handled = true; }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        bool wasPressed = _pressed; _pressed = false; ReleaseMouseCapture();
        if (wasPressed && IsMouseOver) Toggle();
        InvalidateVisual(); e.Handled = true;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter) { Toggle(); e.Handled = true; return; }
        base.OnKeyDown(e);
    }
    private void Toggle()
    {
        if (IsThreeState)
        {
            if (IsChecked == false) IsChecked = null;
            else if (IsChecked == null) IsChecked = true;
            else IsChecked = false;
        }
        else IsChecked = !(IsChecked == true);
    }
}
