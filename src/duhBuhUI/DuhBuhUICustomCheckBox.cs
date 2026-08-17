using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Custom toggle control used by duhBuhUI.
// WPF supplies the Control host; the toggle geometry, state rendering and
// interaction are owned by duhBuhUI.
//
// The global CheckBox name is intentional for this migration step: existing
// DuhBuhUI code already uses CheckBox, so the control can replace the native
// WPF checkbox without changing the public settings API.
public class CheckBox : Control
{
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        "IsChecked", typeof(bool?), typeof(CheckBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        "Content", typeof(object), typeof(CheckBox), new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public event RoutedEventHandler Checked;
    public event RoutedEventHandler Unchecked;

    private bool _focused;
    private bool _pressed;

    public bool? IsChecked
    {
        get { return (bool?)GetValue(IsCheckedProperty); }
        set { SetValue(IsCheckedProperty, value); }
    }

    public object Content
    {
        get { return GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    public bool IsThreeState { get; set; }

    public CheckBox()
    {
        Focusable = true;
        IsTabStop = true;
        Cursor = Cursors.Hand;
        MinHeight = 30;
        Height = 30;
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        Background = new SolidColorBrush(Color.FromRgb(224, 166, 52));
        BorderBrush = new SolidColorBrush(Color.FromRgb(75, 80, 90));
        BorderThickness = new Thickness(1);
        FontSize = 15;
    }

    private static void OnIsCheckedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        if (checkBox == null) return;
        checkBox.InvalidateMeasure();
        checkBox.InvalidateVisual();

        bool oldValue = e.OldValue is bool && (bool)e.OldValue;
        bool newValue = e.NewValue is bool && (bool)e.NewValue;
        if (oldValue == newValue) return;

        RoutedEventHandler handler = newValue ? checkBox.Checked : checkBox.Unchecked;
        if (handler != null) handler(checkBox, new RoutedEventArgs());
    }

    private static void OnVisualPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        if (checkBox != null)
        {
            checkBox.InvalidateMeasure();
            checkBox.InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        FormattedText formatted = Format(text);
        double desiredWidth = 54 + 11 + formatted.Width;
        double desiredHeight = Math.Max(30, formatted.Height + 6);
        double width = double.IsPositiveInfinity(availableSize.Width) ? desiredWidth : Math.Min(desiredWidth, availableSize.Width);
        double height = double.IsPositiveInfinity(availableSize.Height) ? desiredHeight : Math.Min(desiredHeight, availableSize.Height);
        return new Size(Math.Max(54, width), Math.Max(30, height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        bool isChecked = IsChecked == true;
        Color offTrack = Color.FromRgb(52, 56, 64);
        Color onTrack = Color.FromRgb(224, 166, 52);
        Color edge = _focused || _pressed ? Color.FromRgb(224, 166, 52) : Color.FromRgb(75, 80, 90);
        Color textColor = Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245);

        Rect track = new Rect(1, Math.Max(0, (ActualHeight - 30) / 2), 54, 30);
        dc.DrawRoundedRectangle(new SolidColorBrush(isChecked ? onTrack : offTrack), new Pen(new SolidColorBrush(edge), 1), track, 15, 15);

        Rect thumb = isChecked
            ? new Rect(track.Right - 25, track.Top + 4, 22, 22)
            : new Rect(track.Left + 3, track.Top + 4, 22, 22);
        dc.DrawEllipse(Brushes.White, null, new Point(thumb.Left + 11, thumb.Top + 11), 11, 11);

        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        if (!string.IsNullOrEmpty(text))
        {
            FormattedText formatted = Format(text, textColor);
            dc.DrawText(formatted, new Point(66, Math.Max(0, (ActualHeight - formatted.Height) / 2)));
        }
    }

    private FormattedText Format(string value)
    {
        return Format(value, Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245));
    }

    private FormattedText Format(string value, Color color)
    {
        return new FormattedText(value ?? "", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), FontSize, new SolidColorBrush(color), VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        _focused = true;
        InvalidateVisual();
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        _focused = false;
        _pressed = false;
        InvalidateVisual();
        base.OnLostFocus(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        _pressed = true;
        CaptureMouse();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        bool wasPressed = _pressed;
        _pressed = false;
        ReleaseMouseCapture();
        if (wasPressed && IsMouseOver) Toggle();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            Toggle();
            e.Handled = true;
            return;
        }
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
        else
        {
            IsChecked = !(IsChecked == true);
        }
    }
}
