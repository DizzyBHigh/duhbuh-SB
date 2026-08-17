using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public sealed class DuhBuhUICustomRadioGroup : System.Windows.Controls.Border
{
    private readonly List<System.Windows.Controls.Border> _items = new List<System.Windows.Controls.Border>();
    private string[] _options = new string[0];
    private string _selectedItem = "";
    private bool _lightTheme;

    public string[] Options
    {
        get { return _options; }
        set { _options = value ?? new string[0]; Rebuild(); }
    }

    public string SelectedItem
    {
        get { return _selectedItem; }
        set { _selectedItem = value ?? ""; UpdateSelection(); }
    }

    public bool IsLightTheme
    {
        get { return _lightTheme; }
        set { _lightTheme = value; ApplyTheme(); }
    }

    public event EventHandler SelectionChanged;

    public DuhBuhUICustomRadioGroup()
    {
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        Focusable = true;
        Margin = new Thickness(0, 5, 0, 0);
    }

    private void Rebuild()
    {
        _items.Clear();
        System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();
        bool threeColumns = _options.Length == 9;

        if (threeColumns)
        {
            for (int i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });
            }
        }

        for (int i = 0; i < _options.Length; i++)
        {
            System.Windows.Controls.Border item = CreateItem(_options[i]);
            _items.Add(item);

            if (threeColumns)
            {
                System.Windows.Controls.Grid.SetRow(item, i / 3);
                System.Windows.Controls.Grid.SetColumn(item, i % 3);
            }

            grid.Children.Add(item);
        }

        Child = grid;
        ApplyTheme();
        UpdateSelection();
    }

    // Deliberately uses a Border rather than WPF Button/RadioButton so the
    // framework's default pressed/focused blue template can never leak into
    // the custom radio control.
    private System.Windows.Controls.Border CreateItem(string text)
    {
        System.Windows.Controls.Border item = new System.Windows.Controls.Border
        {
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(2, 3, 10, 3),
            Margin = new Thickness(0, 1, 12, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            Focusable = true,
            Tag = text
        };

        item.Child = BuildVisual(text, false);

        item.MouseEnter += delegate
        {
            item.Child = BuildVisual(text, true);
        };

        item.MouseLeave += delegate
        {
            item.Child = BuildVisual(text, false);
        };

        item.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
        {
            SelectItem(text);
            item.Focus();
            e.Handled = true;
        };

        item.KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                SelectItem(text);
                e.Handled = true;
            }
        };

        return item;
    }

    private void SelectItem(string value)
    {
        if (string.Equals(_selectedItem, value, StringComparison.Ordinal))
            return;

        _selectedItem = value;
        UpdateSelection();

        if (SelectionChanged != null)
            SelectionChanged(this, EventArgs.Empty);
    }

    private System.Windows.Controls.StackPanel BuildVisual(string text, bool hover)
    {
        bool selected = string.Equals(_selectedItem, text, StringComparison.Ordinal);
        Color accent = Color.FromRgb(232, 171, 42);
        Color primary = _lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color secondary = _lightTheme ? Color.FromRgb(90, 94, 104) : Color.FromRgb(170, 175, 185);

        System.Windows.Controls.StackPanel row = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        System.Windows.Controls.Border ring = new System.Windows.Controls.Border
        {
            Width = 14,
            Height = 14,
            CornerRadius = new CornerRadius(7),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(selected ? accent : (hover ? accent : secondary)),
            Background = selected ? new SolidColorBrush(accent) : Brushes.Transparent,
            Margin = new Thickness(0, 0, 7, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (selected)
        {
            ring.Child = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(_lightTheme ? Color.FromRgb(255, 255, 255) : Color.FromRgb(35, 37, 42)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        System.Windows.Controls.TextBlock label = new System.Windows.Controls.TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(primary),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };

        row.Children.Add(ring);
        row.Children.Add(label);
        return row;
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            string text = Convert.ToString(_items[i].Tag);
            _items[i].Child = BuildVisual(text, false);
        }
    }

    private void ApplyTheme()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            string text = Convert.ToString(_items[i].Tag);
            _items[i].Child = BuildVisual(text, false);
        }
    }
}
