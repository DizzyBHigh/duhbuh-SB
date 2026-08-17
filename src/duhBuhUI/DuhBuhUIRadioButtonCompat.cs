using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

// Compatibility surface for DuhBuhUI.cs: deliberately NOT the WPF RadioButton.
// The existing framework code can keep its simple RadioButton construction syntax,
// while this type supplies duhBuhUI's own visuals and interaction.
public sealed class RadioButton : Border
{
    private bool _isChecked;
    private object _content;
    private string _groupName = "";
    private bool _lightTheme;
    private bool _themeExplicit;
    private bool _hover;
    private bool _pressed;
    private bool _focused;

    public object Content
    {
        get { return _content; }
        set
        {
            _content = value;
            AutomationProperties.SetName(this, value == null ? "" : value.ToString());
            RebuildVisual();
        }
    }

    public bool? IsChecked
    {
        get { return _isChecked; }
        set
        {
            bool next = value == true;
            if (_isChecked == next) return;
            _isChecked = next;
            if (_isChecked) UncheckGroupSiblings();
            RebuildVisual();
        }
    }

    public string GroupName
    {
        get { return _groupName; }
        set { _groupName = value ?? ""; }
    }

    public bool IsLightTheme
    {
        get { return _lightTheme; }
        set
        {
            _themeExplicit = true;
            _lightTheme = value;
            RebuildVisual();
        }
    }

    // Sizing hooks keep the visual compact while preserving an accessible hit target.
    public double IndicatorSize { get; set; } = 14;
    public double HitTargetHeight { get; set; } = 24;
    public double HitTargetMinWidth { get; set; } = 44;

    public RadioButton()
    {
        Background = Brushes.Transparent;
        BorderBrush = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(4, 2, 4, 2);
        Margin = new Thickness(0, 1, 0, 1);
        HorizontalAlignment = HorizontalAlignment.Left;
        Focusable = true;
        Cursor = Cursors.Hand;
        KeyboardNavigation.SetIsTabStop(this, true);

        MouseEnter += delegate { _hover = true; RebuildVisual(); };
        MouseLeave += delegate { _hover = false; _pressed = false; RebuildVisual(); };
        PreviewMouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e)
        {
            _pressed = true;
            Focus();
            RebuildVisual();
        };
        MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
        {
            _pressed = false;
            SetCheckedFromUser();
            Focus();
            e.Handled = true;
        };
        GotKeyboardFocus += delegate { _focused = true; RebuildVisual(); };
        LostKeyboardFocus += delegate { _focused = false; RebuildVisual(); };
        Loaded += delegate { SyncThemeFromWindow(); };
        KeyDown += HandleKeyDown;

        RebuildVisual();
    }

    private void SyncThemeFromWindow()
    {
        if (_themeExplicit) return;
        Window window = Window.GetWindow(this);
        Brush background = window == null ? null : window.Background;
        SolidColorBrush solid = background as SolidColorBrush;
        if (solid == null)
        {
            _lightTheme = SystemColors.WindowColor.R > 150 && SystemColors.WindowColor.G > 150 && SystemColors.WindowColor.B > 150;
        }
        else
        {
            Color c = solid.Color;
            double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
            _lightTheme = luminance >= 0.62;
        }
        RebuildVisual();
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            SetCheckedFromUser();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Home || e.Key == Key.End)
        {
            RadioButton target = FindKeyboardTarget(e.Key);
            if (target != null)
            {
                target.Focus();
                target.SetCheckedFromUser();
                e.Handled = true;
            }
        }
    }

    private void SetCheckedFromUser()
    {
        if (_isChecked) return;
        _isChecked = true;
        UncheckGroupSiblings();
        RebuildVisual();
    }

    private RadioButton FindKeyboardTarget(Key key)
    {
        Panel panel = FindOwningPanel();
        if (panel == null || string.IsNullOrEmpty(_groupName)) return null;

        List<RadioButton> radios = new List<RadioButton>();
        for (int i = 0; i < panel.Children.Count; i++)
        {
            RadioButton radio = panel.Children[i] as RadioButton;
            if (radio != null && string.Equals(radio.GroupName, _groupName, StringComparison.Ordinal)) radios.Add(radio);
        }
        if (radios.Count == 0) return null;

        int index = radios.IndexOf(this);
        if (index < 0) return null;
        int targetIndex = index;
        if (key == Key.Left || key == Key.Up) targetIndex = (index - 1 + radios.Count) % radios.Count;
        else if (key == Key.Right || key == Key.Down) targetIndex = (index + 1) % radios.Count;
        else if (key == Key.Home) targetIndex = 0;
        else if (key == Key.End) targetIndex = radios.Count - 1;
        return radios[targetIndex];
    }

    private Panel FindOwningPanel()
    {
        DependencyObject parent = VisualTreeHelper.GetParent(this);
        while (parent != null)
        {
            Panel panel = parent as Panel;
            if (panel != null) return panel;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private void UncheckGroupSiblings()
    {
        if (string.IsNullOrEmpty(_groupName)) return;
        Panel panel = FindOwningPanel();
        if (panel == null) return;
        for (int i = 0; i < panel.Children.Count; i++)
        {
            RadioButton sibling = panel.Children[i] as RadioButton;
            if (sibling != null && !ReferenceEquals(sibling, this) && string.Equals(sibling.GroupName, _groupName, StringComparison.Ordinal))
            {
                sibling._isChecked = false;
                sibling.RebuildVisual();
            }
        }
    }

    private void RebuildVisual()
    {
        Color accent = Color.FromRgb(232, 171, 42);
        Color text = _lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color secondary = _lightTheme ? Color.FromRgb(90, 94, 104) : Color.FromRgb(170, 175, 185);
        Color focus = _lightTheme ? Color.FromRgb(44, 90, 160) : Color.FromRgb(115, 170, 255);

        BorderBrush = _focused ? new SolidColorBrush(focus) : Brushes.Transparent;
        BorderThickness = _focused ? new Thickness(1) : new Thickness(0);
        Background = _pressed ? new SolidColorBrush(_lightTheme ? Color.FromRgb(232, 234, 238) : Color.FromRgb(52, 55, 62)) : Brushes.Transparent;

        StackPanel row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        double indicator = Math.Max(12, IndicatorSize);
        Border ring = new Border
        {
            Width = indicator,
            Height = indicator,
            CornerRadius = new CornerRadius(indicator / 2),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush((_isChecked || _hover || _pressed) ? accent : secondary),
            Background = _isChecked ? new SolidColorBrush(accent) : Brushes.Transparent,
            Margin = new Thickness(0, 0, 7, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (_isChecked)
        {
            double dot = Math.Max(4, indicator * 0.43);
            ring.Child = new Ellipse
            {
                Width = dot,
                Height = dot,
                Fill = new SolidColorBrush(_lightTheme ? Colors.White : Color.FromRgb(35, 37, 42)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        row.Children.Add(ring);
        row.Children.Add(new TextBlock
        {
            Text = _content == null ? "" : _content.ToString(),
            Foreground = new SolidColorBrush(text),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        });

        Child = row;
        MinHeight = Math.Max(24, HitTargetHeight);
        MinWidth = Math.Max(44, HitTargetMinWidth);
    }
}
