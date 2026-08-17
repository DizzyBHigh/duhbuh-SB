using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom combined date/time picker. The control owns the field and
// anchored popup; WPF is used only as the window/input foundation.
public sealed class DateTimePicker : Control
{
    private DateTime? _selected;
    private bool _focused;
    private bool _lightTheme;
    private Popup _popup;
    private Window _ownerWindow;

    private static readonly Color DarkPopup = Color.FromRgb(28, 30, 35);
    private static readonly Color DarkPanel = Color.FromRgb(38, 41, 48);
    private static readonly Color DarkBorder = Color.FromRgb(75, 80, 90);
    private static readonly Color DarkText = Color.FromRgb(240, 242, 245);
    private static readonly Color DarkSecondary = Color.FromRgb(170, 176, 186);
    private static readonly Color DarkHover = Color.FromRgb(58, 62, 72);
    private static readonly Color Accent = Color.FromRgb(224, 166, 52);

    public event EventHandler SelectedDateTimeChanged;

    public DateTime? SelectedDateTime
    {
        get { return _selected; }
        set
        {
            DateTime? normalized = value.HasValue ? new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, value.Value.Hour, value.Value.Minute, 0) : (DateTime?)null;
            if (_selected == normalized) return;
            _selected = normalized;
            RaiseChanged();
        }
    }

    public string Value
    {
        get { return _selected.HasValue ? _selected.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : ""; }
        set { SetTextValue(value); }
    }

    public DateTimePicker()
    {
        Focusable = true;
        IsTabStop = true;
        Cursor = Cursors.Hand;
        Height = 34;
        MinHeight = 34;
        MinWidth = 220;
        Background = new SolidColorBrush(DarkPanel);
        Foreground = new SolidColorBrush(DarkText);
        BorderBrush = new SolidColorBrush(DarkBorder);
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 34, 5);
    }

    public void ApplyTheme(bool light)
    {
        _lightTheme = light;
        if (light)
        {
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Foreground = new SolidColorBrush(Color.FromRgb(30, 32, 38));
            BorderBrush = new SolidColorBrush(Color.FromRgb(160, 164, 172));
        }
        else
        {
            Background = new SolidColorBrush(DarkPanel);
            Foreground = new SolidColorBrush(DarkText);
            BorderBrush = new SolidColorBrush(DarkBorder);
        }
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = BrushColor(Background, _lightTheme ? Colors.White : DarkPanel);
        Color fg = BrushColor(Foreground, _lightTheme ? Color.FromRgb(30, 32, 38) : DarkText);
        Color edge = (_focused || (_popup != null && _popup.IsOpen)) ? Accent : BrushColor(BorderBrush, _lightTheme ? Color.FromRgb(160, 164, 172) : DarkBorder);
        dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(edge), 1), new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 3, 3);
        string text = _selected.HasValue ? _selected.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "Select date & time";
        FormattedText ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, new SolidColorBrush(fg), VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(ft, new Point(9, Math.Max(5, (ActualHeight - ft.Height) / 2)));
        double cx = ActualWidth - 17;
        double cy = ActualHeight / 2;
        StreamGeometry arrow = new StreamGeometry();
        using (StreamGeometryContext c = arrow.Open())
        {
            c.BeginFigure(new Point(cx - 5, cy - 2), true, true);
            c.LineTo(new Point(cx + 5, cy - 2), true, false);
            c.LineTo(new Point(cx, cy + 4), true, false);
        }
        dc.DrawGeometry(new SolidColorBrush(fg), null, arrow);
    }

    protected override void OnGotFocus(RoutedEventArgs e) { _focused = true; InvalidateVisual(); base.OnGotFocus(e); }
    protected override void OnLostFocus(RoutedEventArgs e) { _focused = false; InvalidateVisual(); base.OnLostFocus(e); }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        if (_popup != null && _popup.IsOpen) ClosePopup(); else OpenPopup();
        e.Handled = true;
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { ClosePopup(); e.Handled = true; return; }
        if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Down) { OpenPopup(); e.Handled = true; return; }
        base.OnKeyDown(e);
    }

    private void OpenPopup()
    {
        if (_popup != null && _popup.IsOpen) return;
        DateTime initial = _selected ?? DateTime.Now;
        DateTime displayMonth = new DateTime(initial.Year, initial.Month, 1);
        int selectedHour = initial.Hour;
        int selectedMinute = initial.Minute;

        StackPanel root = new StackPanel { Margin = new Thickness(14) };
        root.Children.Add(new TextBlock { Text = "Choose date & time", FontSize = 16, FontWeight = FontWeights.SemiBold, Foreground = Brush(TextColor()), Margin = new Thickness(0, 0, 0, 14) });

        Border calendarBorder = new Border { Background = Brush(PanelColor()), BorderBrush = Brush(BorderColor()), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(5), Padding = new Thickness(10) };
        StackPanel calendarPanel = new StackPanel();
        calendarBorder.Child = calendarPanel;
        root.Children.Add(calendarBorder);

        Action rebuildCalendar = null;
        DuhBuhUICustomDropdown hours = MakeTimeDropdown();
        DuhBuhUICustomDropdown minutes = MakeTimeDropdown();
        string[] hourOptions = new string[24];
        string[] minuteOptions = new string[60];
        for (int i = 0; i < 24; i++) hourOptions[i] = i.ToString("00", CultureInfo.InvariantCulture);
        for (int i = 0; i < 60; i++) minuteOptions[i] = i.ToString("00", CultureInfo.InvariantCulture);
        hours.Options = hourOptions;
        minutes.Options = minuteOptions;
        hours.SelectedIndex = selectedHour;
        minutes.SelectedIndex = selectedMinute;

        Grid timeGrid = new Grid { Margin = new Thickness(0, 12, 0, 0) };
        timeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        timeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        timeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        StackPanel hourPanel = new StackPanel();
        hourPanel.Children.Add(new TextBlock { Text = "Hour", FontSize = 11, Foreground = Brush(SecondaryColor()), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 5) });
        hourPanel.Children.Add(hours);
        StackPanel minutePanel = new StackPanel();
        minutePanel.Children.Add(new TextBlock { Text = "Minute", FontSize = 11, Foreground = Brush(SecondaryColor()), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 5) });
        minutePanel.Children.Add(minutes);
        Grid.SetColumn(hourPanel, 0); Grid.SetColumn(minutePanel, 2);
        timeGrid.Children.Add(hourPanel); timeGrid.Children.Add(minutePanel);
        TextBlock colon = new TextBlock { Text = ":", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = Brush(TextColor()), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 4) };
        Grid.SetColumn(colon, 1); timeGrid.Children.Add(colon);
        root.Children.Add(timeGrid);

        TextBlock preview = new TextBlock { FontSize = 22, FontWeight = FontWeights.SemiBold, Foreground = Brush(Accent), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 14, 0, 12) };
        root.Children.Add(preview);

        StackPanel buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        Button today = MakePopupButton("Today");
        Button now = MakePopupButton("Now");
        Button cancel = MakePopupButton("Cancel");
        Button ok = MakePopupButton("OK");
        today.Margin = new Thickness(0, 0, 8, 0); now.Margin = new Thickness(0, 0, 8, 0); cancel.Margin = new Thickness(0, 0, 8, 0);
        today.Click += delegate { DateTime t = DateTime.Today; initial = t; displayMonth = new DateTime(t.Year, t.Month, 1); rebuildCalendar(); };
        now.Click += delegate { DateTime t = DateTime.Now; initial = t; displayMonth = new DateTime(t.Year, t.Month, 1); hours.SelectedIndex = t.Hour; minutes.SelectedIndex = t.Minute; rebuildCalendar(); };
        cancel.Click += delegate { ClosePopup(); };
        ok.Click += delegate
        {
            DateTime selectedDate = SelectedCalendarDate(initial, displayMonth);
            int h = hours.SelectedIndex < 0 ? initial.Hour : hours.SelectedIndex;
            int m = minutes.SelectedIndex < 0 ? initial.Minute : minutes.SelectedIndex;
            SelectedDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, h, m, 0);
            ClosePopup();
        };
        buttons.Children.Add(today); buttons.Children.Add(now); buttons.Children.Add(cancel); buttons.Children.Add(ok);
        root.Children.Add(buttons);

        rebuildCalendar = delegate
        {
            calendarPanel.Children.Clear();
            Grid header = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            Button previous = MakeNavButton("‹"); Button next = MakeNavButton("›");
            TextBlock monthText = new TextBlock { Text = displayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture), FontSize = 15, FontWeight = FontWeights.SemiBold, Foreground = Brush(TextColor()), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(previous, 0); Grid.SetColumn(monthText, 1); Grid.SetColumn(next, 2); header.Children.Add(previous); header.Children.Add(monthText); header.Children.Add(next);
            previous.Click += delegate { displayMonth = displayMonth.AddMonths(-1); rebuildCalendar(); };
            next.Click += delegate { displayMonth = displayMonth.AddMonths(1); rebuildCalendar(); };
            calendarPanel.Children.Add(header);

            Grid names = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            for (int i = 0; i < 7; i++) names.ColumnDefinitions.Add(new ColumnDefinition());
            string[] dayNames = new[] { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su" };
            for (int i = 0; i < 7; i++) { TextBlock n = new TextBlock { Text = dayNames[i], FontSize = 11, Foreground = Brush(SecondaryColor()), HorizontalAlignment = HorizontalAlignment.Center }; Grid.SetColumn(n, i); names.Children.Add(n); }
            calendarPanel.Children.Add(names);

            Grid days = new Grid();
            for (int i = 0; i < 7; i++) days.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 6; i++) days.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            int offset = ((int)displayMonth.DayOfWeek + 6) % 7;
            int count = DateTime.DaysInMonth(displayMonth.Year, displayMonth.Month);
            DateTime currentSelected = initial.Date;
            if (_selected.HasValue && displayMonth.Year == _selected.Value.Year && displayMonth.Month == _selected.Value.Month) currentSelected = _selected.Value.Date;
            for (int index = 0; index < 42; index++)
            {
                int dayNumber = index - offset + 1;
                if (dayNumber < 1 || dayNumber > count) continue;
                DateTime date = new DateTime(displayMonth.Year, displayMonth.Month, dayNumber);
                bool selected = date.Date == currentSelected.Date;
                bool isToday = date.Date == DateTime.Today;
                Border day = new Border { Margin = new Thickness(1), Background = Brush(selected ? Accent : Colors.Transparent), BorderBrush = Brush(isToday ? Accent : Colors.Transparent), BorderThickness = new Thickness(isToday ? 1 : 0), CornerRadius = new CornerRadius(2), Cursor = Cursors.Hand, Tag = date };
                day.Child = new TextBlock { Text = dayNumber.ToString(CultureInfo.InvariantCulture), FontSize = 12, FontWeight = selected ? FontWeights.SemiBold : FontWeights.Normal, Foreground = Brush(selected ? Color.FromRgb(25, 27, 31) : TextColor()), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center };
                day.MouseEnter += delegate { if (!(bool)(selected)) day.Background = Brush(DarkHover); };
                day.MouseLeave += delegate { day.Background = Brush(date.Date == currentSelected.Date ? Accent : Colors.Transparent); };
                day.MouseLeftButtonDown += delegate { initial = date; displayMonth = new DateTime(date.Year, date.Month, 1); rebuildCalendar(); };
                Grid.SetColumn(day, index % 7); Grid.SetRow(day, index / 7); days.Children.Add(day);
            }
            calendarPanel.Children.Add(days);
            preview.Text = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd} {1:00}:{2:00}", initial, hours.SelectedIndex < 0 ? initial.Hour : hours.SelectedIndex, minutes.SelectedIndex < 0 ? initial.Minute : minutes.SelectedIndex);
        };

        EventHandler updatePreview = delegate { preview.Text = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd} {1:00}:{2:00}", initial, hours.SelectedIndex < 0 ? initial.Hour : hours.SelectedIndex, minutes.SelectedIndex < 0 ? initial.Minute : minutes.SelectedIndex); };
        hours.SelectionChanged += updatePreview; minutes.SelectionChanged += updatePreview;
        rebuildCalendar();

        Border surface = new Border { Background = Brush(PopupBackground()), BorderBrush = Brush(BorderColor()), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(7), Child = root };
        _ownerWindow = Window.GetWindow(this);
        if (_ownerWindow != null) _ownerWindow.PreviewMouseDown += OwnerPreviewMouseDown;
        _popup = new Popup { PlacementTarget = this, Placement = PlacementMode.Bottom, HorizontalOffset = 0, VerticalOffset = 6, AllowsTransparency = true, StaysOpen = true, Focusable = false, Child = surface, Width = 390 };
        _popup.Closed += PopupClosed; _popup.IsOpen = true; InvalidateVisual();
    }

    private DateTime SelectedCalendarDate(DateTime initial, DateTime displayMonth) { if (initial.Year == displayMonth.Year && initial.Month == displayMonth.Month) return initial.Date; return new DateTime(displayMonth.Year, displayMonth.Month, Math.Min(initial.Day, DateTime.DaysInMonth(displayMonth.Year, displayMonth.Month))); }
    private DuhBuhUICustomDropdown MakeTimeDropdown()
    {
        DuhBuhUICustomDropdown d = new DuhBuhUICustomDropdown { MinWidth = 120, Height = 32 };
        d.ApplyTheme(_lightTheme); return d;
    }
    private static Button MakeNavButton(string text) { return new Button { Content = text, FontSize = 20, Padding = new Thickness(0, 0, 0, 2), BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent), Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245)), Cursor = Cursors.Hand }; }
    private Button MakePopupButton(string text) { return new Button { Content = text, FontSize = 13, Padding = new Thickness(12, 6, 12, 6), Background = Brush(PanelColor()), Foreground = Brush(TextColor()), BorderBrush = Brush(BorderColor()), BorderThickness = new Thickness(1), Cursor = Cursors.Hand }; }

    private void OwnerPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_popup == null || !_popup.IsOpen) return;
        DependencyObject source = e.OriginalSource as DependencyObject;
        if (source != null && IsDescendantOf(source)) return;
        ClosePopup();
    }
    private bool IsDescendantOf(DependencyObject source) { DependencyObject current = source; while (current != null) { if (ReferenceEquals(current, this)) return true; current = VisualTreeHelper.GetParent(current); } return false; }
    private void ClosePopup() { if (_popup != null) _popup.IsOpen = false; }
    private void PopupClosed(object sender, EventArgs e)
    {
        if (_ownerWindow != null) { _ownerWindow.PreviewMouseDown -= OwnerPreviewMouseDown; _ownerWindow = null; }
        if (_popup != null) { _popup.Closed -= PopupClosed; _popup = null; }
        InvalidateVisual();
    }

    private void SetTextValue(string value) { DateTime parsed; if (DateTime.TryParseExact(value ?? "", new[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-ddTHH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)) SelectedDateTime = new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, 0); }
    private void RaiseChanged() { EventHandler handler = SelectedDateTimeChanged; if (handler != null) handler(this, EventArgs.Empty); InvalidateMeasure(); InvalidateVisual(); }
    private Brush Brush(Color c) { return new SolidColorBrush(c); }
    private Color TextColor() { return _lightTheme ? Color.FromRgb(30, 32, 38) : DarkText; }
    private Color SecondaryColor() { return _lightTheme ? Color.FromRgb(90, 94, 104) : DarkSecondary; }
    private Color PanelColor() { return _lightTheme ? Color.FromRgb(255, 255, 255) : DarkPanel; }
    private Color PopupBackground() { return _lightTheme ? Color.FromRgb(246, 247, 249) : DarkPopup; }
    private Color BorderColor() { return _lightTheme ? Color.FromRgb(160, 164, 172) : DarkBorder; }
    private static Color BrushColor(Brush brush, Color fallback) { SolidColorBrush solid = brush as SolidColorBrush; return solid == null ? fallback : solid.Color; }
}