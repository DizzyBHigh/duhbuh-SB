using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom dropdown. This deliberately does not use WPF ComboBox for
// the visible control or its popup. WPF is only the window/input foundation.
public sealed class DuhBuhUICustomDropdown : Control
{
    private string[] _options = new string[0];
    private int _selectedIndex = -1;
    private bool _focused;

    private static readonly Color PopupBackground = Color.FromRgb(28, 30, 35);
    private static readonly Color PanelBackground = Color.FromRgb(38, 41, 48);
    private static readonly Color BorderColor = Color.FromRgb(75, 80, 90);
    private static readonly Color TextColor = Color.FromRgb(240, 242, 245);
    private static readonly Color AccentColor = Color.FromRgb(224, 166, 52);
    private static readonly Color HoverColor = Color.FromRgb(58, 66, 76);

    public event EventHandler SelectionChanged;

    public string[] Options
    {
        get { return _options; }
        set
        {
            _options = value ?? new string[0];
            if (_options.Length == 0) SelectedIndex = -1;
            else if (_selectedIndex < 0 || _selectedIndex >= _options.Length) SelectedIndex = 0;
            else InvalidateVisual();
        }
    }

    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set
        {
            int normalized = value;
            if (normalized < -1) normalized = -1;
            if (normalized >= _options.Length) normalized = _options.Length - 1;
            if (_selectedIndex == normalized) { InvalidateVisual(); return; }
            _selectedIndex = normalized;
            InvalidateVisual();
            EventHandler handler = SelectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

    public string SelectedItem
    {
        get { return _selectedIndex >= 0 && _selectedIndex < _options.Length ? _options[_selectedIndex] : ""; }
        set
        {
            if (value == null) { SelectedIndex = -1; return; }
            for (int i = 0; i < _options.Length; i++)
                if (string.Equals(_options[i], value, StringComparison.Ordinal)) { SelectedIndex = i; return; }
            SelectedIndex = -1;
        }
    }

    public string SelectedValue
    {
        get { return SelectedItem; }
        set { SelectedItem = value; }
    }

    public DuhBuhUICustomDropdown()
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
        Color edge = _focused ? AccentColor : BrushColor(BorderBrush, BorderColor);
        dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(edge), 1),
            new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 3, 3);

        string text = SelectedItem;
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
        dc.DrawGeometry(new SolidColorBrush(fg), null, arrow);
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
        OpenPopup();
        e.Handled = true;
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Down)
        {
            OpenPopup();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Up && _options.Length > 0)
        {
            SelectedIndex = _selectedIndex <= 0 ? _options.Length - 1 : _selectedIndex - 1;
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OpenPopup()
    {
        if (_options.Length == 0) return;
        Window owner = Window.GetWindow(this);
        Window popup = new Window
        {
            Title = "Select",
            Width = Math.Max(180, ActualWidth + 4),
            SizeToContent = SizeToContent.Height,
            MaxHeight = 360,
            MinHeight = 34,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Owner = owner,
            ShowInTaskbar = false,
            Background = new SolidColorBrush(PopupBackground),
            Foreground = new SolidColorBrush(TextColor)
        };

        ScrollViewer scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 340,
            Background = new SolidColorBrush(PopupBackground)
        };
        StackPanel list = new StackPanel { Background = new SolidColorBrush(PopupBackground) };
        scroll.Content = list;

        for (int i = 0; i < _options.Length; i++)
        {
            int index = i;
            Border item = new Border
            {
                Height = 32,
                Padding = new Thickness(9, 5, 9, 5),
                Background = new SolidColorBrush(i == _selectedIndex ? HoverColor : PopupBackground),
                Cursor = Cursors.Hand
            };
            TextBlock text = new TextBlock
            {
                Text = _options[i],
                FontSize = 13,
                Foreground = new SolidColorBrush(TextColor),
                VerticalAlignment = VerticalAlignment.Center
            };
            item.Child = text;
            item.MouseEnter += delegate { if (index != _selectedIndex) item.Background = new SolidColorBrush(HoverColor); };
            item.MouseLeave += delegate { item.Background = new SolidColorBrush(index == _selectedIndex ? HoverColor : PopupBackground); };
            item.MouseLeftButtonDown += delegate
            {
                SelectedIndex = index;
                popup.Close();
            };
            list.Children.Add(item);
        }

        popup.Content = new Border
        {
            Background = new SolidColorBrush(PopupBackground),
            BorderBrush = new SolidColorBrush(AccentColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Child = scroll
        };
        popup.ShowDialog();
    }

    private static Color BrushColor(Brush brush, Color fallback)
    {
        SolidColorBrush solid = brush as SolidColorBrush;
        return solid == null ? fallback : solid.Color;
    }
}
