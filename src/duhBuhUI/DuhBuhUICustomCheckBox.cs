using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Custom checkbox control used by duhBuhUI.
// This is intentionally a checkbox, not a toggle switch: it renders a compact
// square box with a check mark and keeps the label immediately to its right.
public sealed class DuhBuhUICustomCheckBox : Control
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        "IsChecked", typeof(bool?), typeof(DuhBuhUICustomCheckBox),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        "Content", typeof(object), typeof(DuhBuhUICustomCheckBox), new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public event RoutedEventHandler Checked;
    public event RoutedEventHandler Unchecked;
    private bool _focused;
    private bool _pressed;

    public bool? IsChecked { get { return (bool?)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }
    public object Content { get { return GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }
    public bool IsThreeState { get; set; }

    public DuhBuhUICustomCheckBox()
    {
        Focusable = true; IsTabStop = true; Cursor = Cursors.Hand;
        MinHeight = 26; Height = 26;
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        Background = new SolidColorBrush(Color.FromRgb(52, 56, 64));
        BorderBrush = new SolidColorBrush(Color.FromRgb(75, 80, 90));
        BorderThickness = new Thickness(1); FontSize = 15;
    }

    private static void OnIsCheckedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DuhBuhUICustomCheckBox checkBox = sender as DuhBuhUICustomCheckBox;
        if (checkBox == null) return;
        checkBox.InvalidateMeasure(); checkBox.InvalidateVisual();
        bool oldValue = e.OldValue is bool && (bool)e.OldValue;
        bool newValue = e.NewValue is bool && (bool)e.NewValue;
        if (oldValue == newValue) return;
        RoutedEventHandler handler = newValue ? checkBox.Checked : checkBox.Unchecked;
        if (handler != null) handler(checkBox, new RoutedEventArgs());
    }

    private static void OnVisualPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DuhBuhUICustomCheckBox checkBox = sender as DuhBuhUICustomCheckBox;
        if (checkBox != null) { checkBox.InvalidateMeasure(); checkBox.InvalidateVisual(); }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        FormattedText formatted = Format(text);
        double desiredWidth = 22 + 10 + formatted.Width;
        double desiredHeight = Math.Max(24, formatted.Height + 4);
        double width = double.IsPositiveInfinity(availableSize.Width) ? desiredWidth : Math.Min(desiredWidth, availableSize.Width);
        double height = double.IsPositiveInfinity(availableSize.Height) ? desiredHeight : Math.Min(desiredHeight, availableSize.Height);
        return new Size(Math.Max(22, width), Math.Max(24, height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        bool isChecked = IsChecked == true;
        Color accent = Color.FromRgb(224, 166, 52);
        Color off = Color.FromRgb(52, 56, 64);
        Color border = _focused || _pressed ? accent : Color.FromRgb(75, 80, 90);
        Color textColor = Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245);

        double top = Math.Max(1, (ActualHeight - 21) / 2);
        Rect box = new Rect(1, top, 21, 21);
        dc.DrawRoundedRectangle(new SolidColorBrush(isChecked ? accent : off), new Pen(new SolidColorBrush(border), 1.5), box, 3, 3);

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

        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        if (!string.IsNullOrEmpty(text))
        {
            FormattedText formatted = Format(text, textColor);
            dc.DrawText(formatted, new Point(32, Math.Max(0, (ActualHeight - formatted.Height) / 2)));
        }
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
