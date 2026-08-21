using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Custom text-entry control used by RtsUI. WPF supplies the window/control
// host; rendering, selection and editing are owned here.
//
// The global TextBox name is intentional: existing RtsUI APIs already use
// TextBox, so this replaces the native control without requiring extension code
// changes.
public sealed class TextBox : Control
{
    private string _text = "";
    private int _caret;
    private int _anchor;
    private bool _dragging;
    private bool _focused;

    public event EventHandler TextChanged;

    public string Text
    {
        get { return _text; }
        set { SetText(value ?? "", true); }
    }

    public bool AcceptsReturn { get; set; }
    public TextWrapping TextWrapping { get; set; }

    public TextBox()
    {
        Focusable = true;
        IsTabStop = true;
        Cursor = Cursors.IBeam;
        Height = 34;
        MinHeight = 34;
        Background = new SolidColorBrush(Color.FromRgb(38, 41, 48));
        Foreground = new SolidColorBrush(Color.FromRgb(240, 242, 245));
        BorderBrush = new SolidColorBrush(Color.FromRgb(75, 80, 90));
        BorderThickness = new Thickness(1);
        Padding = new Thickness(9, 5, 9, 5);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        Color bg = Background is SolidColorBrush ? ((SolidColorBrush)Background).Color : Color.FromRgb(38, 41, 48);
        Color fg = Foreground is SolidColorBrush ? ((SolidColorBrush)Foreground).Color : Color.FromRgb(240, 242, 245);
        Color edge = _focused ? Color.FromRgb(224, 166, 52) : Color.FromRgb(75, 80, 90);
        Rect bounds = new Rect(0.5, 0.5, Math.Max(0, ActualWidth - 1), Math.Max(0, ActualHeight - 1));
        dc.DrawRoundedRectangle(new SolidColorBrush(bg), new Pen(new SolidColorBrush(edge), 1), bounds, 3, 3);

        string normalized = (_text ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = AcceptsReturn ? normalized.Split('\n') : new[] { normalized.Replace("\n", " ") };
        double y = AcceptsReturn ? 7 : Math.Max(5, (ActualHeight - 20) / 2);
        int absolute = 0;
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            FormattedText ft = Format(line, fg);
            int start = Math.Max(0, Math.Min(_anchor, _caret) - absolute);
            int end = Math.Min(line.Length, Math.Max(_anchor, _caret) - absolute);
            if (end > start)
            {
                double sx = 9 + Format(line.Substring(0, start), fg).Width;
                double ex = 9 + Format(line.Substring(0, end), fg).Width;
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(185, 224, 166, 52)), null, new Rect(sx, y, Math.Max(1, ex - sx), ft.Height));
            }
            dc.DrawText(ft, new Point(9, y));
            if (_focused && _caret >= absolute && _caret <= absolute + line.Length)
            {
                double cx = 9 + Format(line.Substring(0, _caret - absolute), fg).Width;
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(224, 166, 52)), 1.5), new Point(cx, y - 1), new Point(cx, y + ft.Height + 1));
            }
            absolute += line.Length + 1;
            y += ft.Height + 2;
            if (!AcceptsReturn) break;
        }
    }

    private FormattedText Format(string value, Color color)
    {
        return new FormattedText(value ?? "", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 15, new SolidColorBrush(color), VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    protected override void OnGotFocus(RoutedEventArgs e) { _focused = true; InvalidateVisual(); base.OnGotFocus(e); }
    protected override void OnLostFocus(RoutedEventArgs e) { _focused = false; _dragging = false; ReleaseMouseCapture(); InvalidateVisual(); base.OnLostFocus(e); }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus(); CaptureMouse(); _dragging = true; _caret = HitTestText(e.GetPosition(this)); _anchor = _caret; InvalidateVisual(); e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging && e.LeftButton == MouseButtonState.Pressed) { _caret = HitTestText(e.GetPosition(this)); InvalidateVisual(); e.Handled = true; }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { _dragging = false; ReleaseMouseCapture(); e.Handled = true; base.OnMouseLeftButtonUp(e); }

    private int HitTestText(Point point)
    {
        string normalized = (_text ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = AcceptsReturn ? normalized.Split('\n') : new[] { normalized.Replace("\n", " ") };
        double y = AcceptsReturn ? 7 : Math.Max(5, (ActualHeight - 20) / 2);
        int absolute = 0;
        for (int li = 0; li < lines.Length; li++)
        {
            string line = lines[li];
            FormattedText ft = Format(line, Colors.White);
            if (point.Y <= y + ft.Height)
            {
                for (int i = 0; i <= line.Length; i++)
                    if (point.X < 9 + Format(line.Substring(0, i), Colors.White).Width + 4) return absolute + i;
                return absolute + line.Length;
            }
            absolute += line.Length + 1;
            y += ft.Height + 2;
        }
        return _text.Length;
    }

    protected override void OnTextInput(TextCompositionEventArgs e) { ReplaceSelection(e.Text); e.Handled = true; base.OnTextInput(e); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        if (ctrl)
        {
            if (e.Key == Key.A) { _anchor = 0; _caret = _text.Length; }
            else if (e.Key == Key.C) CopySelection();
            else if (e.Key == Key.X) { CopySelection(); DeleteSelection(); }
            else if (e.Key == Key.V) ReplaceSelection(Clipboard.ContainsText() ? Clipboard.GetText() : "");
            else { base.OnKeyDown(e); return; }
            InvalidateVisual(); e.Handled = true; return;
        }
        switch (e.Key)
        {
            case Key.Back:
                if (HasSelection()) DeleteSelection();
                else if (_caret > 0) { _text = _text.Remove(_caret - 1, 1); _caret--; _anchor = _caret; RaiseChanged(); }
                break;
            case Key.Delete:
                if (HasSelection()) DeleteSelection();
                else if (_caret < _text.Length) { _text = _text.Remove(_caret, 1); RaiseChanged(); }
                break;
            case Key.Left: _caret = Math.Max(0, _caret - 1); if (!shift) _anchor = _caret; break;
            case Key.Right: _caret = Math.Min(_text.Length, _caret + 1); if (!shift) _anchor = _caret; break;
            case Key.Home: _caret = 0; if (!shift) _anchor = _caret; break;
            case Key.End: _caret = _text.Length; if (!shift) _anchor = _caret; break;
            case Key.Enter: if (AcceptsReturn) ReplaceSelection("\n"); else return; break;
            default: base.OnKeyDown(e); return;
        }
        InvalidateVisual(); e.Handled = true;
    }

    private bool HasSelection() { return _anchor != _caret; }
    private void DeleteSelection()
    {
        int a = Math.Min(_anchor, _caret), b = Math.Max(_anchor, _caret);
        _text = _text.Remove(a, b - a); _caret = a; _anchor = a; RaiseChanged();
    }

    private void ReplaceSelection(string value)
    {
        DeleteSelection();
        string insert = value ?? "";
        if (!AcceptsReturn) insert = insert.Replace("\r", "").Replace("\n", " ");
        _text = _text.Insert(_caret, insert); _caret += insert.Length; _anchor = _caret; RaiseChanged();
    }

    private void CopySelection()
    {
        if (!HasSelection()) return;
        int a = Math.Min(_anchor, _caret), b = Math.Max(_anchor, _caret);
        Clipboard.SetText(_text.Substring(a, b - a));
    }

    private void SetText(string value, bool notify)
    {
        _text = value ?? ""; _caret = Math.Min(_caret, _text.Length); _anchor = _caret; InvalidateVisual();
        if (notify) RaiseChanged();
    }

    private void RaiseChanged()
    {
        EventHandler handler = TextChanged; if (handler != null) handler(this, EventArgs.Empty);
        InvalidateMeasure(); InvalidateVisual();
    }
}
