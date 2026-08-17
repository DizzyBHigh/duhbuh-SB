using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom dropdown. This deliberately does not use WPF ComboBox for
// the visible control or its popup. WPF is only the window/input foundation.
public sealed class DuhBuhUICustomDropdown : Control
{
    private string[] _options = new string[0];
    private int _selectedIndex = -1;
    private bool _focused;
    private Popup _popup;

    private Color _popupBackground = Color.FromRgb(28, 30, 34);
    private Color _panelBackground = Color.FromRgb(36, 39, 45);
    private Color _borderColor = Color.FromRgb(75, 80, 90);
    private Color _textColor = Color.FromRgb(240, 242, 245);
    private Color _accentColor = Color.FromRgb(224, 166, 52);
    private Color _hoverColor = Color.FromRgb(60, 65, 74);
    private Color _selectedTextColor = Colors.White;

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
        Background = new SolidColorBrush(_panelBackground);
        Foreground = new SolidColorBrush(_textColor);
        BorderBrush = new SolidColorBrush(_borderColor);
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 34, 5);
    }

    // Theme colors are supplied explicitly by duhBuhUI. This keeps the
    // control independent of WPF ComboBox styles/templates/resources.
    public void ApplyTheme(bool light)
    {
        if (light)
        {
            _popupBackground = Color.FromRgb(246, 247, 249);
            _panelBackground = Color.FromRgb(255, 255, 255);
            _borderColor = Color.FromRgb(205, 210, 220);
            _textColor = Color.FromRgb(30, 32, 38);
            _accentColor = Color.FromRgb(176, 120, 22);
            _hoverColor = Color.FromRgb(232, 235, 240);
            _selectedTextColor = Colors.White;
        }
        else
        {
            _popupBackground = Color.FromRgb(28, 30, 34);
            _panelBackground = Color.FromRgb(36, 39, 45);
            _borderColor = Color.FromRgb(75, 80, 90);
            _textColor = Color.FromRgb(240, 242, 245);
            _accentColor = Color.FromRgb(224, 166, 52);
            _hoverColor = Color.FromRgb(60, 65, 74);
            _selectedTextColor = Colors.White;
        }
        Background = new SolidColorBrush(_panelBackground);
        Foreground = new SolidColorBrush(_textColor);
        BorderBrush = new SolidColorBrush(_borderColor);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = BrushColor(Background, _panelBackground);
        Color fg = BrushColor(Foreground, _textColor);
        Color edge = (_focused || (_popup != null && _popup.IsOpen)) ? _accentColor : BrushColor(BorderBrush, _borderColor);
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
        TogglePopup();
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

    private void TogglePopup()
    {
        if (_popup != null && _popup.IsOpen) ClosePopup();
        else OpenPopup();
    }

    private void OpenPopup()
    {
        if (_options.Length == 0) return;
        if (_popup != null && _popup.IsOpen) return;

        ScrollViewer scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 340,
            Background = new SolidColorBrush(_popupBackground)
        };
        StackPanel list = new StackPanel { Background = new SolidColorBrush(_popupBackground) };
        scroll.Content = list;

        for (int i = 0; i < _options.Length; i++)
        {
            int index = i;
            Border item = new Border
            {
                Height = 32,
                Padding = new Thickness(9, 5, 9, 5),
                Background = new SolidColorBrush(i == _selectedIndex ? _accentColor : _popupBackground),
                Cursor = Cursors.Hand
            };
            TextBlock text = new TextBlock
            {
                Text = _options[i],
                FontSize = 13,
                Foreground = new SolidColorBrush(i == _selectedIndex ? _selectedTextColor : _textColor),
                VerticalAlignment = VerticalAlignment.Center
            };
            item.Child = text;
            item.MouseEnter += delegate
            {
                if (index != _selectedIndex)
                {
                    item.Background = new SolidColorBrush(_hoverColor);
                    text.Foreground = new SolidColorBrush(_textColor);
                }
            };
            item.MouseLeave += delegate
            {
                item.Background = new SolidColorBrush(index == _selectedIndex ? _accentColor : _popupBackground);
                text.Foreground = new SolidColorBrush(index == _selectedIndex ? _selectedTextColor : _textColor);
            };
            item.MouseLeftButtonDown += delegate
            {
                SelectedIndex = index;
                ClosePopup();
            };
            list.Children.Add(item);
        }

        Border surface = new Border
        {
            Background = new SolidColorBrush(_popupBackground),
            BorderBrush = new SolidColorBrush(_borderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Child = scroll
        };

        _popup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = -1,
            VerticalOffset = 1,
            AllowsTransparency = true,
            StaysOpen = false,
            Focusable = false,
            Child = surface,
            Width = Math.Max(ActualWidth + 2, MinWidth + 2)
        };
        _popup.Closed += PopupClosed;
        _popup.IsOpen = true;
        InvalidateVisual();
    }

    private void ClosePopup()
    {
        if (_popup == null) return;
        _popup.IsOpen = false;
    }

    private void PopupClosed(object sender, EventArgs e)
    {
        if (_popup != null)
        {
            _popup.Closed -= PopupClosed;
            _popup = null;
        }
        InvalidateVisual();
    }

    private static Color BrushColor(Brush brush, Color fallback)
    {
        SolidColorBrush solid = brush as SolidColorBrush;
        return solid == null ? fallback : solid.Color;
    }
}
