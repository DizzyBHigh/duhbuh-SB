using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

public sealed class DuhBuhUICustomColorPicker : Control
{
    private static DuhBuhUICustomColorPicker _openOwner;
    private Popup _popup;
    private string _value = "#FFFFFFFF";
    private string _originalValue;

    private static readonly Color BackgroundColor = Color.FromRgb(38, 41, 48);
    private static readonly Color PopupColor = Color.FromRgb(28, 30, 35);
    private static readonly Color BorderColor = Color.FromRgb(75, 80, 90);
    private static readonly Color TextColor = Color.FromRgb(240, 242, 245);
    private static readonly Color SecondaryTextColor = Color.FromRgb(170, 176, 186);
    private static readonly Color AccentColor = Color.FromRgb(224, 166, 52);

    public string Value
    {
        get { return _value; }
        set
        {
            string normalized = NormalizeColor(value);
            if (normalized == "") return;
            if (_value == normalized) return;
            _value = normalized;
            InvalidateVisual();
        }
    }

    public DuhBuhUICustomColorPicker()
    {
        Width = 300;
        Height = 34;
        MinHeight = 34;
        Focusable = true;
        Cursor = Cursors.Hand;
        Background = new SolidColorBrush(BackgroundColor);
        Foreground = new SolidColorBrush(TextColor);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color edge = _popup != null && _popup.IsOpen ? AccentColor : BorderColor;
        dc.DrawRoundedRectangle(new SolidColorBrush(BackgroundColor), new Pen(new SolidColorBrush(edge), 1),
            new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 4, 4);

        Color swatchColor = ParseColor(_value, Colors.White);
        dc.DrawRoundedRectangle(new SolidColorBrush(swatchColor), new Pen(new SolidColorBrush(BorderColor), 1),
            new Rect(7, 6, 38, Math.Max(0, ActualHeight - 12)), 3, 3);

        FormattedText text = new FormattedText(_value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 14, new SolidColorBrush(TextColor), VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(text, new Point(55, Math.Max(5, (ActualHeight - text.Height) / 2)));

        double x = ActualWidth - 16;
        double y = ActualHeight / 2;
        StreamGeometry arrow = new StreamGeometry();
        using (StreamGeometryContext c = arrow.Open())
        {
            c.BeginFigure(new Point(x - 5, y - 2), true, true);
            c.LineTo(new Point(x + 5, y - 2), true, false);
            c.LineTo(new Point(x, y + 4), true, false);
        }
        dc.DrawGeometry(new SolidColorBrush(TextColor), null, arrow);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        if (_popup != null && _popup.IsOpen) ClosePopup();
        else OpenPopup();
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
        base.OnKeyDown(e);
    }

    private void OpenPopup()
    {
        if (_popup != null && _popup.IsOpen) return;
        if (_openOwner != null && !ReferenceEquals(_openOwner, this)) _openOwner.ClosePopup();
        _openOwner = this;
        _originalValue = _value;

        StackPanel root = new StackPanel { Margin = new Thickness(14) };
        root.Children.Add(new TextBlock
        {
            Text = "Choose colour",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(TextColor),
            Margin = new Thickness(0, 0, 0, 12)
        });

        Border preview = new Border
        {
            Height = 44,
            CornerRadius = new CornerRadius(5),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 12)
        };
        TextBlock previewText = new TextBlock
        {
            Text = _value,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.SemiBold
        };
        preview.Child = previewText;
        root.Children.Add(preview);
        UpdatePreview(preview, previewText, _value);

        WrapPanel palette = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(-2, 0, -2, 10) };
        string[] colours = new[]
        {
            "#FFFFFFFF", "#FFE6E6E6", "#FFB8B8B8", "#FF808080", "#FF505050", "#FF202020", "#FF000000",
            "#FFFF0000", "#FFFF6600", "#FFFFA000", "#FFFFFF00", "#FF80FF00", "#FF00FF00", "#FF00FFFF",
            "#FF00AEEF", "#FF0080FF", "#FF0000FF", "#FF8000FF", "#FFFF00FF", "#FFFF66CC", "#FFFF8080",
            "#FF804000", "#FF800000", "#FF808000", "#FF008000", "#FF008080", "#FF000080", "#FF800080",
            "#FF2A9D8F", "#FF06D6A0", "#FF118AB2", "#FF3A86FF", "#FF8338EC", "#FFFF006E", "#FFFB5607"
        };
        for (int i = 0; i < colours.Length; i++)
        {
            string colour = colours[i];
            Button swatch = new Button
            {
                Width = 34,
                Height = 30,
                Margin = new Thickness(2),
                Padding = new Thickness(0),
                Tag = colour,
                Background = new SolidColorBrush(ParseColor(colour, Colors.White)),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                ToolTip = colour
            };
            swatch.Click += delegate(object sender, RoutedEventArgs args)
            {
                string selected = (string)((Button)sender).Tag;
                SetTemporaryColor(selected, preview, previewText);
            };
            palette.Children.Add(swatch);
        }
        root.Children.Add(palette);

        TextBlock hexLabel = new TextBlock
        {
            Text = "Hex colour",
            FontSize = 12,
            Foreground = new SolidColorBrush(SecondaryTextColor),
            Margin = new Thickness(0, 0, 0, 4)
        };
        root.Children.Add(hexLabel);

        TextBox hex = new TextBox
        {
            Text = _value,
            Height = 32,
            FontSize = 14,
            Padding = new Thickness(8, 5, 8, 5),
            Foreground = new SolidColorBrush(TextColor),
            Background = new SolidColorBrush(BackgroundColor),
            BorderBrush = new SolidColorBrush(BorderColor)
        };
        root.Children.Add(hex);

        TextBlock hint = new TextBlock
        {
            Text = "#RRGGBB or #AARRGGBB",
            FontSize = 11,
            Foreground = new SolidColorBrush(SecondaryTextColor),
            Margin = new Thickness(0, 3, 0, 12)
        };
        root.Children.Add(hint);

        hex.TextChanged += delegate
        {
            string normalized = NormalizeColor(hex.Text);
            if (normalized != "") SetTemporaryColor(normalized, preview, previewText);
        };

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Button cancel = MakeButton("Cancel");
        Button ok = MakeButton("OK");
        cancel.Margin = new Thickness(0, 0, 8, 0);
        cancel.Click += delegate { _value = _originalValue; ClosePopup(); };
        ok.Click += delegate
        {
            string normalized = NormalizeColor(hex.Text);
            if (normalized != "") _value = normalized;
            else _value = _originalValue;
            ClosePopup();
            InvalidateVisual();
        };
        buttons.Children.Add(cancel);
        buttons.Children.Add(ok);
        root.Children.Add(buttons);

        Border surface = new Border
        {
            Background = new SolidColorBrush(PopupColor),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Child = root
        };

        _popup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 0,
            VerticalOffset = 6,
            AllowsTransparency = true,
            StaysOpen = true,
            Focusable = false,
            Width = 320,
            Child = surface
        };
        _popup.Closed += PopupClosed;
        _popup.IsOpen = true;
        InvalidateVisual();
    }

    private void SetTemporaryColor(string value, Border preview, TextBlock previewText)
    {
        string normalized = NormalizeColor(value);
        if (normalized == "") return;
        _value = normalized;
        UpdatePreview(preview, previewText, normalized);
        InvalidateVisual();
    }

    private static Button MakeButton(string text)
    {
        return new Button
        {
            Content = text,
            MinWidth = 76,
            Height = 34,
            Padding = new Thickness(12, 5, 12, 5),
            Background = new SolidColorBrush(BackgroundColor),
            Foreground = new SolidColorBrush(TextColor),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand
        };
    }

    private static void UpdatePreview(Border preview, TextBlock text, string value)
    {
        Color color = ParseColor(value, Colors.White);
        preview.Background = new SolidColorBrush(color);
        text.Text = value;
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        text.Foreground = new SolidColorBrush(luminance > 0.62 ? Colors.Black : Colors.White);
    }

    private void ClosePopup()
    {
        if (_popup == null) return;
        _popup.IsOpen = false;
    }

    private void PopupClosed(object sender, EventArgs e)
    {
        if (ReferenceEquals(_openOwner, this)) _openOwner = null;
        if (_popup != null) _popup.Closed -= PopupClosed;
        _popup = null;
        InvalidateVisual();
    }

    private static string NormalizeColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        string v = value.Trim();
        if (!v.StartsWith("#", StringComparison.Ordinal)) v = "#" + v;
        if (v.Length == 7) return "#FF" + v.Substring(1).ToUpperInvariant();
        if (v.Length == 9) return "#" + v.Substring(1).ToUpperInvariant();
        return "";
    }

    private static Color ParseColor(string value, Color fallback)
    {
        try { return (Color)ColorConverter.ConvertFromString(NormalizeColor(value)); }
        catch { return fallback; }
    }
}