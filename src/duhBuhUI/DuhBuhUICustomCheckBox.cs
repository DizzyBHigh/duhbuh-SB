using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
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
    private bool _hover;
    private bool _lightTheme;
    private bool _themeSynced;

    public bool? IsChecked { get { return (bool?)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }
    public object Content { get { return GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }
    public bool IsThreeState { get; set; }

    public CheckBox()
    {
        Focusable = true; IsTabStop = true; Cursor = Cursors.Hand;
        MinHeight = 32; Height = 32;
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        FontSize = 15;
        Loaded += delegate { SyncThemeFromWindow(); ApplyAccessibility(); InvalidateMeasure(); InvalidateVisual(); };
        MouseEnter += delegate { _hover = true; InvalidateVisual(); };
        MouseLeave += delegate { _hover = false; _pressed = false; InvalidateVisual(); };
        IsEnabledChanged += delegate { ApplyAccessibility(); InvalidateVisual(); };
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
        control.ApplyAccessibility();
        bool oldValue = e.OldValue is bool && (bool)e.OldValue;
        bool newValue = e.NewValue is bool && (bool)e.NewValue;
        if (oldValue == newValue) return;
        RoutedEventHandler handler = newValue ? control.Checked : control.Unchecked;
        if (handler != null) handler(control, new RoutedEventArgs());
    }

    private static void OnVisualPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        CheckBox control = sender as CheckBox;
        if (control != null) { control.InvalidateMeasure(); control.InvalidateVisual(); control.ApplyAccessibility(); }
    }

    private void SyncThemeFromWindow()
    {
        if (_themeSynced) return;
        _themeSynced = true;
        Window window = Window.GetWindow(this);
        SolidColorBrush brush = window == null ? null : window.Background as SolidColorBrush;
        if (brush == null)
        {
            Color system = SystemColors.WindowColor;
            _lightTheme = system.R > 180 && system.G > 180 && system.B > 180;
            return;
        }

        Color color = brush.Color;
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        _lightTheme = luminance >= 0.62;
    }

    private void ApplyAccessibility()
    {
        string name = Convert.ToString(Content, CultureInfo.CurrentUICulture);
        if (string.IsNullOrWhiteSpace(name)) name = "Toggle switch";
        AutomationProperties.SetName(this, name);
        AutomationProperties.SetHelpText(this, "Press Space or Enter to toggle.");
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        string text = Convert.ToString(Content, CultureInfo.CurrentUICulture) ?? "";
        FormattedText formatted = Format(text);
        bool square = IsSquareCheckbox();
        double desiredWidth = (square ? 22 : 56) + 11 + formatted.Width;
        double desiredHeight = square ? Math.Max(26, formatted.Height + 4) : Math.Max(32, formatted.Height + 8);
        double width = double.IsPositiveInfinity(availableSize.Width) ? desiredWidth : Math.Min(desiredWidth, availableSize.Width);
        double height = double.IsPositiveInfinity(availableSize.Height) ? desiredHeight : Math.Min(desiredHeight, availableSize.Height);
        return new Size(Math.Max(square ? 22 : 56, width), Math.Max(square ? 26 : 32, height));
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
        Color offTrack = _lightTheme ? Color.FromRgb(218, 221, 226) : Color.FromRgb(52, 56, 64);
        Color onTrack = Color.FromRgb(224, 166, 52);
        Color offHover = _lightTheme ? Color.FromRgb(202, 206, 213) : Color.FromRgb(62, 67, 76);
        Color onHover = Color.FromRgb(235, 178, 62);
        Color offPressed = _lightTheme ? Color.FromRgb(190, 195, 203) : Color.FromRgb(43, 47, 54);
        Color onPressed = Color.FromRgb(194, 137, 32);
        Color border = _lightTheme ? Color.FromRgb(160, 166, 176) : Color.FromRgb(75, 80, 90);
        Color disabledTrack = _lightTheme ? Color.FromRgb(232, 234, 237) : Color.FromRgb(43, 46, 52);
        Color disabledThumb = _lightTheme ? Color.FromRgb(196, 199, 204) : Color.FromRgb(105, 109, 117);
        Color disabledText = _lightTheme ? Color.FromRgb(145, 149, 158) : Color.FromRgb(110, 114, 122);

        Color trackColor;
        if (!IsEnabled) trackColor = disabledTrack;
        else if (_pressed) trackColor = isChecked ? onPressed : offPressed;
        else if (_hover) trackColor = isChecked ? onHover : offHover;
        else trackColor = isChecked ? onTrack : offTrack;

        Color edge = _focused ? onTrack : border;
        Rect track = new Rect(1, Math.Max(1, (ActualHeight - 28) / 2), 52, 28);
        dc.DrawRoundedRectangle(new SolidColorBrush(trackColor), new Pen(new SolidColorBrush(edge), _focused ? 1.5 : 1), track, 14, 14);

        Rect thumb = isChecked ? new Rect(track.Right - 24, track.Top + 3, 22, 22) : new Rect(track.Left + 3, track.Top + 3, 22, 22);
        dc.DrawEllipse(IsEnabled ? Brushes.White : new SolidColorBrush(disabledThumb), null,
            new Point(thumb.Left + 11, thumb.Top + 11), 11, 11);

        Color textColor = !IsEnabled ? disabledText : (Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : (_lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245)));
        DrawText(dc, textColor, 64);
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
        if (wasPressed && IsMouseOver && IsEnabled) Toggle();
        InvalidateVisual(); e.Handled = true;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if ((e.Key == Key.Space || e.Key == Key.Enter) && IsEnabled) { Toggle(); e.Handled = true; return; }
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
