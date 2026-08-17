using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom date picker. The control owns the field rendering and
// interaction; WPF is used only as the windowing/input foundation.
public sealed class DatePicker : Control
{
    private DateTime? _selectedDate;
    private bool _focused;

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
        Background = new SolidColorBrush(Color.FromRgb(38, 41, 48));
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        BorderBrush = new SolidColorBrush(Color.FromRgb(75, 80, 90));
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 34, 5);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = BrushColor(Background, Color.FromRgb(38, 41, 48));
        Color fg = BrushColor(Foreground, Color.FromRgb(240, 242, 245));
        Color edge = _focused ? Color.FromRgb(224, 166, 52) : Color.FromRgb(75, 80, 90);
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
        dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(240, 242, 245)), null, arrow);
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
        OpenCalendar();
        e.Handled = true;
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
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
        Window owner = Window.GetWindow(this);
        Window popup = new Window
        {
            Title = "Choose Date",
            Width = 300,
            Height = 320,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Owner = owner,
            Background = new SolidColorBrush(Color.FromRgb(28, 30, 35))
        };

        StackPanel root = new StackPanel { Margin = new Thickness(12) };
        root.Children.Add(new TextBlock
        {
            Text = "Choose date",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245)),
            Margin = new Thickness(0, 0, 0, 8)
        });

        System.Windows.Controls.Calendar calendar = new System.Windows.Controls.Calendar
        {
            SelectedDate = _selectedDate ?? DateTime.Today,
            DisplayDate = _selectedDate ?? DateTime.Today,
            Background = new SolidColorBrush(Color.FromRgb(38, 41, 48)),
            Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(75, 80, 90))
        };
        root.Children.Add(calendar);

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };
        Button today = new Button { Content = "Today", Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 0, 8, 0) };
        Button cancel = new Button { Content = "Cancel", Padding = new Thickness(10, 5, 10, 5) };
        today.Click += delegate { SelectedDate = DateTime.Today; popup.Close(); };
        cancel.Click += delegate { popup.Close(); };
        calendar.SelectedDatesChanged += delegate
        {
            if (calendar.SelectedDate.HasValue)
            {
                SelectedDate = calendar.SelectedDate.Value;
                popup.Close();
            }
        };
        buttons.Children.Add(today);
        buttons.Children.Add(cancel);
        root.Children.Add(buttons);
        popup.Content = root;
        popup.ShowDialog();
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
