using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom date picker. The control owns the field and popup
// presentation; WPF is used only as the window/input foundation.
public sealed class DatePicker : Control
{
    private DateTime? _selectedDate;
    private bool _focused;
    private Popup _popup;
    private Window _ownerWindow;

    private static readonly Color PopupBackground = Color.FromRgb(28, 30, 35);
    private static readonly Color PanelBackground = Color.FromRgb(38, 41, 48);
    private static readonly Color BorderColor = Color.FromRgb(75, 80, 90);
    private static readonly Color TextColor = Color.FromRgb(240, 242, 245);
    private static readonly Color SecondaryTextColor = Color.FromRgb(170, 176, 186);
    private static readonly Color AccentColor = Color.FromRgb(224, 166, 52);
    private static readonly Color HoverColor = Color.FromRgb(58, 62, 72);

    public event EventHandler SelectedDateChanged;

    public DateTime? SelectedDate
    {
        get { return _selectedDate; }
        set
        {
            DateTime? normalized = value.HasValue ? value.Value.Date : (DateTime?)null;
            if (_selectedDate == normalized) return;
            _selectedDate = normalized;
            RaiseChanged();
        }
    }

    public DatePicker()
    {
        Focusable = true;
        IsTabStop = true;
        Cursor = Cursors.Hand;
        Height = 34;
        MinHeight = 34;
        MinWidth = 180;
        Background = new SolidColorBrush(PanelBackground);
        Foreground = new SolidColorBrush(TextColor);
        BorderBrush = new SolidColorBrush(BorderColor);
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 34, 5);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = BrushColor(Background, PanelBackground);
        Color fg = BrushColor(Foreground, TextColor);
        Color edge = (_focused || (_popup != null && _popup.IsOpen)) ? AccentColor : BorderColor;
        dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(edge), 1),
            new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 3, 3);

        string text = _selectedDate.HasValue
            ? _selectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "Select date";
        FormattedText ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 14, new SolidColorBrush(fg), VisualTreeHelper.GetDpi(this).PixelsPerDip);
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
        dc.DrawGeometry(new SolidColorBrush(TextColor), null, arrow);
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
        InvalidateVisual();
        base.OnLostFocus(e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        if (_popup != null && _popup.IsOpen) ClosePopup();
        else OpenCalendar();
        e.Handled = true;
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClosePopup();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Down)
        {
            OpenCalendar();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OpenCalendar()
    {
        if (_popup != null && _popup.IsOpen) return;
        DuhBuhUIPickerPopupManager.Activate(this, ClosePopup);

        DateTime displayMonth = new DateTime(
            (_selectedDate ?? DateTime.Today).Year,
            (_selectedDate ?? DateTime.Today).Month,
            1);

        StackPanel root = new StackPanel { Margin = new Thickness(14) };
        root.Children.Add(new TextBlock
        {
            Text = "Choose date",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(TextColor),
            Margin = new Thickness(0, 0, 0, 14)
        });

        Border calendarBorder = new Border
        {
            Background = new SolidColorBrush(PanelBackground),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10)
        };

        StackPanel calendarPanel = new StackPanel();
        calendarBorder.Child = calendarPanel;
        root.Children.Add(calendarBorder);

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0)
        };
        Button today = MakePopupButton("Today");
        Button cancel = MakePopupButton("Cancel");
        today.Margin = new Thickness(0, 0, 8, 0);
        today.Click += delegate { SelectedDate = DateTime.Today; ClosePopup(); };
        cancel.Click += delegate { ClosePopup(); };
        buttons.Children.Add(today);
        buttons.Children.Add(cancel);
        root.Children.Add(buttons);

        Action rebuild = null;
        rebuild = delegate
        {
            calendarPanel.Children.Clear();

            Grid header = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

            Button previous = MakeNavButton("‹");
            Button next = MakeNavButton("›");
            TextBlock monthText = new TextBlock
            {
                Text = displayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(TextColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(previous, 0);
            Grid.SetColumn(monthText, 1);
            Grid.SetColumn(next, 2);
            header.Children.Add(previous);
            header.Children.Add(monthText);
            header.Children.Add(next);
            previous.Click += delegate { displayMonth = displayMonth.AddMonths(-1); rebuild(); };
            next.Click += delegate { displayMonth = displayMonth.AddMonths(1); rebuild(); };
            calendarPanel.Children.Add(header);

            Grid dayNames = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            for (int i = 0; i < 7; i++) dayNames.ColumnDefinitions.Add(new ColumnDefinition());
            string[] names = new[] { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su" };
            for (int i = 0; i < 7; i++)
            {
                TextBlock name = new TextBlock
                {
                    Text = names[i],
                    FontSize = 11,
                    Foreground = new SolidColorBrush(SecondaryTextColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(name, i);
                dayNames.Children.Add(name);
            }
            calendarPanel.Children.Add(dayNames);

            Grid days = new Grid();
            for (int i = 0; i < 7; i++) days.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 6; i++) days.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });

            DateTime first = displayMonth;
            int offset = ((int)first.DayOfWeek + 6) % 7;
            int daysInMonth = DateTime.DaysInMonth(displayMonth.Year, displayMonth.Month);

            for (int index = 0; index < 42; index++)
            {
                int dayNumber = index - offset + 1;
                if (dayNumber < 1 || dayNumber > daysInMonth) continue;

                DateTime date = new DateTime(displayMonth.Year, displayMonth.Month, dayNumber);
                bool selected = _selectedDate.HasValue && _selectedDate.Value.Date == date.Date;
                bool todayDate = date.Date == DateTime.Today;

                Border day = new Border
                {
                    Margin = new Thickness(1),
                    Background = new SolidColorBrush(selected ? AccentColor : Colors.Transparent),
                    BorderBrush = new SolidColorBrush(todayDate ? AccentColor : Colors.Transparent),
                    BorderThickness = new Thickness(todayDate ? 1 : 0),
                    CornerRadius = new CornerRadius(2),
                    Cursor = Cursors.Hand,
                    Tag = date
                };

                TextBlock dayText = new TextBlock
                {
                    Text = dayNumber.ToString(CultureInfo.InvariantCulture),
                    FontSize = 12,
                    FontWeight = selected ? FontWeights.SemiBold : FontWeights.Normal,
                    Foreground = new SolidColorBrush(selected ? Color.FromRgb(25, 27, 31) : TextColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
                day.Child = dayText;

                day.MouseEnter += delegate
                {
                    bool isSelected = _selectedDate.HasValue && _selectedDate.Value.Date == date.Date;
                    if (!isSelected) day.Background = new SolidColorBrush(HoverColor);
                };
                day.MouseLeave += delegate
                {
                    bool isSelected = _selectedDate.HasValue && _selectedDate.Value.Date == date.Date;
                    day.Background = new SolidColorBrush(isSelected ? AccentColor : Colors.Transparent);
                };
                day.MouseLeftButtonDown += delegate
                {
                    SelectedDate = (DateTime)day.Tag;
                    ClosePopup();
                };

                Grid.SetColumn(day, index % 7);
                Grid.SetRow(day, index / 7);
                days.Children.Add(day);
            }

            calendarPanel.Children.Add(days);
        };

        rebuild();

        Border surface = new Border
        {
            Background = new SolidColorBrush(PopupBackground),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Child = root
        };

        _ownerWindow = Window.GetWindow(this);
        if (_ownerWindow != null)
            _ownerWindow.PreviewMouseDown += OwnerPreviewMouseDown;

        _popup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 0,
            VerticalOffset = 6,
            AllowsTransparency = true,
            StaysOpen = true,
            Focusable = false,
            Child = surface,
            Width = 340
        };
        _popup.Closed += PopupClosed;
        _popup.IsOpen = true;
        InvalidateVisual();
    }

    private void OwnerPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_popup == null || !_popup.IsOpen) return;
        DependencyObject source = e.OriginalSource as DependencyObject;
        if (source != null && IsDescendantOf(source)) return;
        ClosePopup();
    }

    private bool IsDescendantOf(DependencyObject source)
    {
        DependencyObject current = source;
        while (current != null)
        {
            if (ReferenceEquals(current, this)) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private void ClosePopup()
    {
        if (_popup == null) return;
        _popup.IsOpen = false;
    }

    private void PopupClosed(object sender, EventArgs e)
    {
        if (_ownerWindow != null)
        {
            _ownerWindow.PreviewMouseDown -= OwnerPreviewMouseDown;
            _ownerWindow = null;
        }
        if (_popup != null)
        {
            _popup.Closed -= PopupClosed;
            _popup = null;
        }
        DuhBuhUIPickerPopupManager.Deactivate(this);
        InvalidateVisual();
    }

    private static Button MakeNavButton(string text)
    {
        return new Button
        {
            Content = text,
            FontSize = 20,
            Padding = new Thickness(0, 0, 0, 2),
            BorderThickness = new Thickness(0),
            Background = new SolidColorBrush(Colors.Transparent),
            Foreground = new SolidColorBrush(TextColor),
            Cursor = Cursors.Hand
        };
    }

    private static Button MakePopupButton(string text)
    {
        return new Button
        {
            Content = text,
            FontSize = 13,
            Padding = new Thickness(12, 6, 12, 6),
            Background = new SolidColorBrush(PanelBackground),
            Foreground = new SolidColorBrush(TextColor),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand
        };
    }

    private void RaiseChanged()
    {
        EventHandler handler = SelectedDateChanged;
        if (handler != null) handler(this, EventArgs.Empty);
        InvalidateMeasure();
        InvalidateVisual();
    }

    private static Color BrushColor(Brush brush, Color fallback)
    {
        SolidColorBrush solid = brush as SolidColorBrush;
        return solid == null ? fallback : solid.Color;
    }
}
