using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

public sealed class DuhBuhUICustomColorPicker : Control
{
    private Popup _popup;
    private Window _ownerWindow;
    private TextBox _hexBox;
    private Border _swatch;
    private bool _lightTheme;
    private string _selectedColor = "#FFFFFFFF";
    private string _originalColor = "#FFFFFFFF";

    private static readonly Color DarkPanel = Color.FromRgb(36, 39, 45);
    private static readonly Color DarkPopup = Color.FromRgb(28, 30, 34);
    private static readonly Color DarkBorder = Color.FromRgb(75, 80, 90);
    private static readonly Color DarkText = Color.FromRgb(240, 242, 245);
    private static readonly Color DarkSecondary = Color.FromRgb(170, 175, 185);
    private static readonly Color DarkAccent = Color.FromRgb(224, 166, 52);
    private static readonly Color DarkHover = Color.FromRgb(60, 65, 74);

    private static readonly Color LightPanel = Color.FromRgb(255, 255, 255);
    private static readonly Color LightPopup = Color.FromRgb(246, 247, 249);
    private static readonly Color LightBorder = Color.FromRgb(205, 210, 220);
    private static readonly Color LightText = Color.FromRgb(30, 32, 38);
    private static readonly Color LightSecondary = Color.FromRgb(90, 94, 104);
    private static readonly Color LightAccent = Color.FromRgb(176, 120, 22);
    private static readonly Color LightHover = Color.FromRgb(232, 235, 240);

    private static readonly string[] Palette = new[]
    {
        "#FFFFFFFF", "#FFF2F2F2", "#FFBFBFBF", "#FF808080", "#FF404040", "#FF000000",
        "#FFFF0000", "#FFFF8000", "#FFFFFF00", "#FF80FF00", "#FF00FF00", "#FF00FFFF",
        "#FF0080FF", "#FF0000FF", "#FF8000FF", "#FFFF00FF", "#FFFF80C0", "#FF804000",
        "#FF800000", "#FF808000", "#FF008000", "#FF008080", "#FF000080", "#FF800080",
        "#FF00AEEF", "#FF0077B6", "#FF3A86FF", "#FF8338EC", "#FFFF006E", "#FFFB5607",
        "#FFFFBE0B", "#FF2A9D8F", "#FF06D6A0", "#FF118AB2", "#FFEF476F", "#FF6C7576"
    };

    static DuhBuhUICustomColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DuhBuhUICustomColorPicker), new FrameworkPropertyMetadata(typeof(DuhBuhUICustomColorPicker)));
    }

    public DuhBuhUICustomColorPicker()
    {
        Width = 310;
        Height = 34;
        MinHeight = 34;
        Background = Brush(DarkPanel);
        Foreground = Brush(DarkText);
        BorderBrush = Brush(DarkBorder);
        BorderThickness = new Thickness(1);
        Focusable = true;
        Cursor = Cursors.Hand;
        ToolTip = "Click to choose a colour";
    }

    public string SelectedColor
    {
        get { return _selectedColor; }
        set
        {
            string normalized = NormalizeColor(value);
            if (normalized == "") return;
            _selectedColor = normalized;
            if (_hexBox != null && _hexBox.Text != normalized) _hexBox.Text = normalized;
            UpdateSwatch();
            InvalidateVisual();
        }
    }

    public void ApplyTheme(bool light)
    {
        _lightTheme = light;
        Background = Brush(light ? LightPanel : DarkPanel);
        Foreground = Brush(light ? LightText : DarkText);
        BorderBrush = Brush(light ? LightBorder : DarkBorder);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color background = BrushColor(Background, _lightTheme ? LightPanel : DarkPanel);
        Color border = BrushColor(BorderBrush, _lightTheme ? LightBorder : DarkBorder);
        Color accent = _lightTheme ? LightAccent : DarkAccent;
        dc.DrawRoundedRectangle(Brush(background), new Pen(Brush(IsKeyboardFocusWithin ? accent : border), 1),
            new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1)), 4, 4);

        Color color;
        if (!TryParseColor(_selectedColor, out color)) color = Colors.White;
        dc.DrawRoundedRectangle(Brush(color), new Pen(Brush(border), 1),
            new Rect(8, 6, 22, Math.Max(0, ActualHeight - 12)), 3, 3);

        string text = _selectedColor;
        FormattedText formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 13, Brush(_lightTheme ? LightText : DarkText), 1.0);
        dc.DrawText(formatted, new Point(38, Math.Max(0, (ActualHeight - formatted.Height) / 2)));

        FormattedText choose = new FormattedText("Choose…", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI Semibold"), 12, Brush(_lightTheme ? LightAccent : DarkAccent), 1.0);
        dc.DrawText(choose, new Point(Math.Max(120, ActualWidth - choose.Width - 12), Math.Max(0, (ActualHeight - choose.Height) / 2)));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        if (_popup != null && _popup.IsOpen) ClosePopup(false);
        else OpenPopup();
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
        if (e.Key == Key.Escape)
        {
            ClosePopup(false);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private void OpenPopup()
    {
        if (_popup != null && _popup.IsOpen) return;

        _originalColor = _selectedColor;
        Color panel = _lightTheme ? LightPanel : DarkPanel;
        Color popup = _lightTheme ? LightPopup : DarkPopup;
        Color border = _lightTheme ? LightBorder : DarkBorder;
        Color text = _lightTheme ? LightText : DarkText;
        Color secondary = _lightTheme ? LightSecondary : DarkSecondary;
        Color accent = _lightTheme ? LightAccent : DarkAccent;
        Color hover = _lightTheme ? LightHover : DarkHover;

        StackPanel root = new StackPanel { Margin = new Thickness(14), Background = Brush(popup) };
        TextBlock title = new TextBlock
        {
            Text = "Choose colour",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush(text),
            Margin = new Thickness(0, 0, 0, 9)
        };
        root.Children.Add(title);

        Border preview = new Border
        {
            Height = 44,
            CornerRadius = new CornerRadius(4),
            BorderBrush = Brush(border),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10),
            Tag = "preview"
        };
        TextBlock previewText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontSize = 12
        };
        preview.Child = previewText;
        root.Children.Add(preview);

        TextBlock paletteLabel = new TextBlock
        {
            Text = "Palette",
            FontSize = 11,
            Foreground = Brush(secondary),
            Margin = new Thickness(0, 0, 0, 5)
        };
        root.Children.Add(paletteLabel);

        WrapPanel palette = new WrapPanel { Margin = new Thickness(-2, 0, -2, 10) };
        for (int i = 0; i < Palette.Length; i++)
        {
            string colour = Palette[i];
            Button swatch = new Button
            {
                Width = 34,
                Height = 28,
                Margin = new Thickness(2),
                Padding = new Thickness(0),
                Background = BrushFromHex(colour),
                BorderBrush = Brush(border),
                BorderThickness = new Thickness(1),
                Tag = colour,
                ToolTip = colour,
                Focusable = true,
                Cursor = Cursors.Hand
            };
            swatch.Click += delegate(object sender, RoutedEventArgs e)
            {
                string selected = (string)((Button)sender).Tag;
                _selectedColor = selected;
                if (_hexBox != null) _hexBox.Text = selected;
                UpdatePreview(preview, previewText, selected);
                UpdateSwatch();
            };
            swatch.MouseEnter += delegate(object sender, MouseEventArgs e)
            {
                Button b = (Button)sender;
                b.BorderBrush = Brush(accent);
            };
            swatch.MouseLeave += delegate(object sender, MouseEventArgs e)
            {
                Button b = (Button)sender;
                b.BorderBrush = Brush(border);
            };
            palette.Children.Add(swatch);
        }
        root.Children.Add(palette);

        StackPanel customRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        TextBlock customLabel = new TextBlock
        {
            Text = "Hex:",
            Width = 32,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brush(text)
        };
        _hexBox = new TextBox
        {
            Text = _selectedColor,
            Width = 145,
            Height = 30,
            Padding = new Thickness(7, 4, 7, 4),
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = Brush(text),
            Background = Brush(panel),
            BorderBrush = Brush(border),
            BorderThickness = new Thickness(1)
        };
        Button apply = new Button
        {
            Content = "Apply",
            Height = 30,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(7, 0, 0, 0),
            Background = Brush(panel),
            Foreground = Brush(text),
            BorderBrush = Brush(border),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand
        };
        customRow.Children.Add(customLabel);
        customRow.Children.Add(_hexBox);
        customRow.Children.Add(apply);
        root.Children.Add(customRow);

        TextBlock hint = new TextBlock
        {
            Text = "#RRGGBB or #AARRGGBB",
            FontSize = 11,
            Foreground = Brush(secondary),
            Margin = new Thickness(0, -5, 0, 10)
        };
        root.Children.Add(hint);

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Button cancel = MakePopupButton("Cancel", panel, text, border, hover);
        Button ok = MakePopupButton("OK", panel, text, border, hover);
        cancel.Margin = new Thickness(0, 0, 7, 0);
        buttons.Children.Add(cancel);
        buttons.Children.Add(ok);
        root.Children.Add(buttons);

        Action refresh = delegate
        {
            string normalized = NormalizeColor(_hexBox.Text);
            if (normalized != "")
            {
                _selectedColor = normalized;
                UpdatePreview(preview, previewText, normalized);
                UpdateSwatch();
            }
        };
        _hexBox.TextChanged += delegate { refresh(); };
        _hexBox.KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitAndClose();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ClosePopup(false);
                e.Handled = true;
            }
        };
        apply.Click += delegate { refresh(); };
        ok.Click += delegate { CommitAndClose(); };
        cancel.Click += delegate { ClosePopup(false); };

        UpdatePreview(preview, previewText, _selectedColor);

        Border surface = new Border
        {
            Background = Brush(popup),
            BorderBrush = Brush(border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = root
        };

        _ownerWindow = Window.GetWindow(this);
        _popup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            VerticalOffset = 6,
            AllowsTransparency = true,
            StaysOpen = true,
            Focusable = false,
            Child = surface,
            Width = 386
        };
        if (_ownerWindow != null) _ownerWindow.PreviewMouseDown += OwnerPreviewMouseDown;
        DuhBuhUIPopupCoordinator.Open(_popup, this);
        _popup.Closed += PopupClosed;
        _popup.IsOpen = true;
        InvalidateVisual();
    }

    private Button MakePopupButton(string content, Color background, Color text, Color border, Color hover)
    {
        Button button = new Button
        {
            Content = content,
            Width = 82,
            Height = 34,
            Padding = new Thickness(10, 5, 10, 5),
            Background = Brush(background),
            Foreground = Brush(text),
            BorderBrush = Brush(border),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand
        };
        button.MouseEnter += delegate { button.Background = Brush(hover); };
        button.MouseLeave += delegate { button.Background = Brush(background); };
        return button;
    }

    private void OwnerPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_popup == null || !_popup.IsOpen) return;
        DependencyObject source = e.OriginalSource as DependencyObject;
        if (source != null && IsDescendantOf(source, this)) return;
        ClosePopup(false);
    }

    private bool IsDescendantOf(DependencyObject source, DependencyObject ancestor)
    {
        DependencyObject current = source;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor)) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private void CommitAndClose()
    {
        string normalized = NormalizeColor(_hexBox == null ? _selectedColor : _hexBox.Text);
        if (normalized == "") normalized = _selectedColor;
        _selectedColor = normalized;
        ClosePopup(true);
    }

    private void ClosePopup(bool commit)
    {
        if (_popup == null) return;
        if (!commit) _selectedColor = _originalColor;
        else _originalColor = _selectedColor;
        UpdateSwatch();
        _popup.IsOpen = false;
    }

    private void PopupClosed(object sender, EventArgs e)
    {
        Popup popup = _popup;
        DuhBuhUIPopupCoordinator.Closed(popup);
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
        _hexBox = null;
        InvalidateVisual();
    }

    private void UpdateSwatch()
    {
        if (_swatch != null) _swatch.Background = BrushFromHex(_selectedColor);
        InvalidateVisual();
    }

    private void UpdatePreview(Border preview, TextBlock text, string colour)
    {
        Color c;
        if (!TryParseColor(colour, out c)) c = Colors.White;
        preview.Background = Brush(c);
        text.Text = NormalizeColor(colour);
        double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
        text.Foreground = Brush(luminance > 0.6 ? Colors.Black : Colors.White);
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

    private static bool TryParseColor(string value, out Color color)
    {
        color = Colors.Transparent;
        string normalized = NormalizeColor(value);
        if (normalized == "") return false;
        try
        {
            object converted = ColorConverter.ConvertFromString(normalized);
            if (converted is Color)
            {
                color = (Color)converted;
                return true;
            }
        }
        catch { }
        return false;
    }

    private static Brush BrushFromHex(string value)
    {
        Color color;
        return TryParseColor(value, out color) ? Brush(color) : Brush(Colors.Transparent);
    }

    private static SolidColorBrush Brush(Color color)
    {
        return new SolidColorBrush(color);
    }

    private static Color BrushColor(Brush brush, Color fallback)
    {
        SolidColorBrush solid = brush as SolidColorBrush;
        return solid == null ? fallback : solid.Color;
    }
}

public static class DuhBuhUIColorPickerCompatibility
{
    private static readonly HashSet<Window> ProcessedWindows = new HashSet<Window>();
    private const string StorageMarker = "__duhbuh_color_picker_storage";

    [ModuleInitializer]
    public static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded), true);
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null || ProcessedWindows.Contains(window)) return;
        ProcessedWindows.Add(window);
        window.Dispatcher.BeginInvoke(new Action(delegate { ReplaceColorRows(window); }),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private static void ReplaceColorRows(Window window)
    {
        ReplaceInTree(window, window);
    }

    private static void ReplaceInTree(DependencyObject root, Window window)
    {
        StackPanel panel = root as StackPanel;
        if (panel != null) TryReplaceRow(panel, window);

        foreach (object childObject in LogicalTreeHelper.GetChildren(root))
        {
            DependencyObject child = childObject as DependencyObject;
            if (child != null) ReplaceInTree(child, window);
        }
    }

    private static void TryReplaceRow(StackPanel row, Window window)
    {
        if (row.Tag == null || row.Resources.Contains(StorageMarker)) return;
        if (row.Orientation != Orientation.Horizontal || row.Children.Count != 3) return;

        Button first = row.Children[0] as Button;
        TextBox hex = row.Children[1] as TextBox;
        Button choose = row.Children[2] as Button;
        if (first == null || hex == null || choose == null) return;
        if (!string.Equals(Convert.ToString(choose.Content, CultureInfo.InvariantCulture), "Choose…", StringComparison.Ordinal)) return;
        if (!string.Equals(Convert.ToString(first.ToolTip, CultureInfo.InvariantCulture), "Click to choose a colour", StringComparison.Ordinal)) return;

        string key = row.Tag.ToString();
        string current = NormalizeColor(hex.Text);
        if (current == "") current = "#FFFFFFFF";

        DuhBuhUICustomColorPicker picker = new DuhBuhUICustomColorPicker
        {
            SelectedColor = current,
            Width = Math.Max(310, Math.Min(520, row.ActualWidth > 0 ? row.ActualWidth : 360)),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        picker.ApplyTheme(IsLightWindow(window));

        row.Children.Clear();
        row.Children.Add(picker);

        TextBox storage = new TextBox
        {
            Text = current,
            Tag = key,
            Visibility = Visibility.Collapsed
        };
        row.Children.Add(storage);

        picker.LostKeyboardFocus += delegate
        {
            if (storage.Text != picker.SelectedColor) storage.Text = picker.SelectedColor;
        };
        picker.MouseLeftButtonUp += delegate
        {
            storage.Text = picker.SelectedColor;
        };

        row.Resources[StorageMarker] = true;
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

    private static bool IsLightWindow(Window window)
    {
        if (window == null) return false;
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color c = brush.Color;
        double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
        return luminance >= 0.62;
    }
}
