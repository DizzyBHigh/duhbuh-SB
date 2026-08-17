using System;
using System.Windows;
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
    private bool _hover;

    public object Content
    {
        get { return _content; }
        set { _content = value; RebuildVisual(); }
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
        set { _lightTheme = value; RebuildVisual(); }
    }

    public RadioButton()
    {
        Background = Brushes.Transparent;
        BorderBrush = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Padding = new Thickness(0);
        Margin = new Thickness(0, 2, 0, 2);
        HorizontalAlignment = HorizontalAlignment.Left;
        Focusable = true;
        Cursor = Cursors.Hand;

        MouseEnter += delegate { _hover = true; RebuildVisual(); };
        MouseLeave += delegate { _hover = false; RebuildVisual(); };
        MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
        {
            SetCheckedFromUser();
            Focus();
            e.Handled = true;
        };
        KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                SetCheckedFromUser();
                e.Handled = true;
            }
        };

        RebuildVisual();
    }

    private void SetCheckedFromUser()
    {
        if (_isChecked) return;
        _isChecked = true;
        UncheckGroupSiblings();
        RebuildVisual();
    }

    private void UncheckGroupSiblings()
    {
        if (string.IsNullOrEmpty(_groupName)) return;
        DependencyObject parent = VisualTreeHelper.GetParent(this);
        while (parent != null)
        {
            Panel panel = parent as Panel;
            if (panel != null)
            {
                for (int i = 0; i < panel.Children.Count; i++)
                {
                    RadioButton sibling = panel.Children[i] as RadioButton;
                    if (sibling != null && !ReferenceEquals(sibling, this) && string.Equals(sibling.GroupName, _groupName, StringComparison.Ordinal))
                    {
                        sibling._isChecked = false;
                        sibling.RebuildVisual();
                    }
                }
                return;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
    }

    private void RebuildVisual()
    {
        Color accent = Color.FromRgb(232, 171, 42);
        Color text = _lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color secondary = _lightTheme ? Color.FromRgb(90, 94, 104) : Color.FromRgb(170, 175, 185);

        StackPanel row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        Border ring = new Border
        {
            Width = 14,
            Height = 14,
            CornerRadius = new CornerRadius(7),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush((_isChecked || _hover) ? accent : secondary),
            Background = _isChecked ? new SolidColorBrush(accent) : Brushes.Transparent,
            Margin = new Thickness(0, 0, 7, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (_isChecked)
        {
            ring.Child = new Ellipse
            {
                Width = 6,
                Height = 6,
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
    }
}
