using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// RtsUI custom time picker. The picker popup is anchored to the field and
// uses the reusable RtsUI dropdown rather than a WPF ComboBox.
public sealed class TimePicker : Control
{
    private TimeSpan? _selectedTime;
    private bool _focused;
    private bool _lightTheme;
    private Popup _popup;

    private static readonly Color PopupBackground = Color.FromRgb(28, 30, 35);
    private static readonly Color PanelBackground = Color.FromRgb(38, 41, 48);
    private static readonly Color BorderColor = Color.FromRgb(75, 80, 90);
    private static readonly Color TextColorStatic = Color.FromRgb(240, 242, 245);
    private static readonly Color AccentColor = Color.FromRgb(224, 166, 52);

    public event EventHandler SelectedTimeChanged;
    public TimeSpan? SelectedTime
    {
        get { return _selectedTime; }
        set
        {
            TimeSpan? normalized = value.HasValue ? new TimeSpan(value.Value.Hours, value.Value.Minutes, 0) : (TimeSpan?)null;
            if (_selectedTime == normalized) return;
            _selectedTime = normalized;
            RaiseChanged();
        }
    }

    public string Value
    {
        get { return _selectedTime.HasValue ? _selectedTime.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture) : ""; }
        set { SetTextValue(value); }
    }
    public string Text { get { return Value; } set { SetTextValue(value); } }

    public TimePicker()
    {
        Focusable = true; IsTabStop = true; Cursor = Cursors.Hand;
        Height = 34; MinHeight = 34; MinWidth = 180;
        Background = new SolidColorBrush(PanelBackground);
        Foreground = new SolidColorBrush(TextColorStatic);
        BorderBrush = new SolidColorBrush(BorderColor);
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 34, 5);
    }

    public void ApplyTheme(bool light)
    {
        _lightTheme = light;
        Background = new SolidColorBrush(light ? Color.FromRgb(255, 255, 255) : PanelBackground);
        Foreground = new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : TextColorStatic);
        BorderBrush = new SolidColorBrush(light ? Color.FromRgb(160, 164, 172) : BorderColor);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = BrushColor(Background, _lightTheme ? Colors.White : PanelBackground);
        Color fg = BrushColor(Foreground, _lightTheme ? Color.FromRgb(30, 32, 38) : TextColorStatic);
        Color edge = (_focused || (_popup != null && _popup.IsOpen)) ? AccentColor : BrushColor(BorderBrush, BorderColor);
        dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(edge), 1), new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 3, 3);
        string text = _selectedTime.HasValue ? _selectedTime.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture) : "Select time";
        FormattedText ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, new SolidColorBrush(fg), VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(ft, new Point(9, Math.Max(5, (ActualHeight - ft.Height) / 2)));
        double cx = ActualWidth - 17, cy = ActualHeight / 2;
        StreamGeometry arrow = new StreamGeometry();
        using (StreamGeometryContext c = arrow.Open()) { c.BeginFigure(new Point(cx - 5, cy - 2), true, true); c.LineTo(new Point(cx + 5, cy - 2), true, false); c.LineTo(new Point(cx, cy + 4), true, false); }
        dc.DrawGeometry(new SolidColorBrush(fg), null, arrow);
    }

    protected override void OnGotFocus(RoutedEventArgs e) { _focused = true; InvalidateVisual(); base.OnGotFocus(e); }
    protected override void OnLostFocus(RoutedEventArgs e) { _focused = false; InvalidateVisual(); base.OnLostFocus(e); }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { Focus(); if (_popup != null && _popup.IsOpen) ClosePopup(); else OpenTimePopup(); e.Handled = true; base.OnMouseLeftButtonDown(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.Key == Key.Escape) { ClosePopup(); e.Handled = true; return; } if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Down) { OpenTimePopup(); e.Handled = true; return; } base.OnKeyDown(e); }

    private void OpenTimePopup()
    {
        if (_popup != null && _popup.IsOpen) return;
        TimeSpan initial = _selectedTime ?? new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
        int selectedHour = initial.Hours, selectedMinute = initial.Minutes;

        StackPanel root = new StackPanel { Margin = new Thickness(14) };
        root.Children.Add(new TextBlock { Text = "Choose time", FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = Brush(TextColor()), Margin = new Thickness(0, 0, 0, 12) });

        RtsUICustomDropdown hours = MakeTimeDropdown();
        RtsUICustomDropdown minutes = MakeTimeDropdown();
        string[] hourOptions = new string[24], minuteOptions = new string[60];
        for (int i = 0; i < 24; i++) hourOptions[i] = i.ToString("00", CultureInfo.InvariantCulture);
        for (int i = 0; i < 60; i++) minuteOptions[i] = i.ToString("00", CultureInfo.InvariantCulture);
        hours.Options = hourOptions; minutes.Options = minuteOptions;
        hours.SelectedIndex = selectedHour; minutes.SelectedIndex = selectedMinute;

        Grid pickerGrid = new Grid();
        pickerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        pickerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        pickerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        StackPanel hourPanel = new StackPanel(); hourPanel.Children.Add(MakePickerLabel("Hour")); hourPanel.Children.Add(hours);
        StackPanel minutePanel = new StackPanel(); minutePanel.Children.Add(MakePickerLabel("Minute")); minutePanel.Children.Add(minutes);
        Grid.SetColumn(hourPanel, 0); Grid.SetColumn(minutePanel, 2); pickerGrid.Children.Add(hourPanel); pickerGrid.Children.Add(minutePanel);
        TextBlock colon = new TextBlock { Text = ":", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = Brush(TextColor()), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 4) };
        Grid.SetColumn(colon, 1); pickerGrid.Children.Add(colon); root.Children.Add(pickerGrid);

        TextBlock preview = new TextBlock { FontSize = 24, FontWeight = FontWeights.SemiBold, Foreground = Brush(AccentColor), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 14) };
        root.Children.Add(preview);

        StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        Button now = MakePopupButton("Now", 82), cancel = MakePopupButton("Cancel", 90), ok = MakePopupButton("OK", 68);
        now.Margin = new Thickness(0, 0, 8, 0); cancel.Margin = new Thickness(0, 0, 8, 0);
        now.Click += delegate { DateTime n = DateTime.Now; hours.SelectedIndex = n.Hour; minutes.SelectedIndex = n.Minute; };
        cancel.Click += delegate { ClosePopup(); };
        ok.Click += delegate { int h = hours.SelectedIndex < 0 ? 0 : hours.SelectedIndex; int m = minutes.SelectedIndex < 0 ? 0 : minutes.SelectedIndex; SelectedTime = new TimeSpan(h, m, 0); ClosePopup(); };
        buttons.Children.Add(now); buttons.Children.Add(cancel); buttons.Children.Add(ok); root.Children.Add(buttons);

        Action updatePreview = delegate { int h = hours.SelectedIndex < 0 ? 0 : hours.SelectedIndex; int m = minutes.SelectedIndex < 0 ? 0 : minutes.SelectedIndex; preview.Text = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", h, m); };
        hours.SelectionChanged += delegate { updatePreview(); }; minutes.SelectionChanged += delegate { updatePreview(); };
        updatePreview();

        Border surface = new Border { Background = Brush(PopupBackground), BorderBrush = Brush(BorderColor), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(7), Child = root };
        _popup = new Popup { PlacementTarget = this, Placement = PlacementMode.Bottom, VerticalOffset = 6, AllowsTransparency = true, StaysOpen = true, Focusable = false, Child = surface, Width = 300 };
        _popup.Closed += PopupClosed; _popup.IsOpen = true; InvalidateVisual();
    }

    private RtsUICustomDropdown MakeTimeDropdown()
    {
        RtsUICustomDropdown d = new RtsUICustomDropdown { MinWidth = 120, Height = 32 };
        d.ApplyTheme(_lightTheme); return d;
    }
    private TextBlock MakePickerLabel(string text) { return new TextBlock { Text = text, FontSize = 11, Foreground = Brush(SecondaryColor()), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 5) }; }
    private static Button MakePopupButton(string text, double width) { return new Button { Content = text, Width = width, Height = 38, FontSize = 13, Padding = new Thickness(8, 6, 8, 6), Background = new SolidColorBrush(PanelBackground), Foreground = new SolidColorBrush(TextColorStatic), BorderBrush = new SolidColorBrush(BorderColor), BorderThickness = new Thickness(1), Cursor = Cursors.Hand }; }
    private void ClosePopup() { if (_popup != null) _popup.IsOpen = false; }
    private void PopupClosed(object sender, EventArgs e) { if (_popup != null) { _popup.Closed -= PopupClosed; _popup = null; } InvalidateVisual(); }
    private void SetTextValue(string value) { TimeSpan parsed; if (TimeSpan.TryParseExact(value ?? "", new[] { @"hh\:mm", @"h\:mm", @"HH\:mm", @"H\:mm" }, CultureInfo.InvariantCulture, out parsed)) SelectedTime = new TimeSpan(parsed.Hours, parsed.Minutes, 0); }
    private void RaiseChanged() { EventHandler handler = SelectedTimeChanged; if (handler != null) handler(this, EventArgs.Empty); InvalidateMeasure(); InvalidateVisual(); }
    private Brush Brush(Color c) { return new SolidColorBrush(c); }
    private Color TextColor() { return _lightTheme ? Color.FromRgb(30, 32, 38) : TextColorStatic; }
    private Color SecondaryColor() { return _lightTheme ? Color.FromRgb(90, 94, 104) : Color.FromRgb(170, 176, 186); }
    private static Color BrushColor(Brush brush, Color fallback) { SolidColorBrush solid = brush as SolidColorBrush; return solid == null ? fallback : solid.Color; }
}
