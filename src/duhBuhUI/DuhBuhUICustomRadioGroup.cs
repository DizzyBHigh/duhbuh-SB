using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public sealed class DuhBuhUICustomRadioGroup : Border
{
    private readonly List<Button> _items = new List<Button>();
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
        KeyboardNavigation.IsTabStop = true;
        Margin = new Thickness(0, 5, 0, 0);
    }

    private void Rebuild()
    {
        _items.Clear();
        Grid grid = new Grid();
        bool threeColumns = _options.Length == 9;
        if (threeColumns)
        {
            for (int i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
        }

        for (int i = 0; i < _options.Length; i++)
        {
            Button item = CreateItem(_options[i], i);
            _items.Add(item);
            if (threeColumns)
            {
                Grid.SetRow(item, i / 3);
                Grid.SetColumn(item, i % 3);
            }
            grid.Children.Add(item);
        }
        Child = grid;
        ApplyTheme();
        UpdateSelection();
    }

    private Button CreateItem(string text, int index)
    {
        Button button = new Button
        {
            Content = BuildVisual(text, false),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(2, 2, 10, 2),
            Margin = new Thickness(0, 1, 12, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Focusable = true,
            Tag = text
        };
        button.Click += delegate
        {
            string value = Convert.ToString(button.Tag);
            if (!string.Equals(_selectedItem, value, StringComparison.Ordinal))
            {
                _selectedItem = value;
                UpdateSelection();
                if (SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
            }
            Focus();
        };
        button.MouseEnter += delegate { button.Content = BuildVisual(text, true); };
        button.MouseLeave += delegate { button.Content = BuildVisual(text, string.Equals(_selectedItem, text, StringComparison.Ordinal)); };
        button.KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
        };
        return button;
    }

    private StackPanel BuildVisual(string text, bool hover)
    {
        bool selected = string.Equals(_selectedItem, text, StringComparison.Ordinal);
        Color accent = Color.FromRgb(232, 171, 42);
        Color primary = _lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color secondary = _lightTheme ? Color.FromRgb(90, 94, 104) : Color.FromRgb(170, 175, 185);
        StackPanel row = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        Border ring = new Border
        {
            Width = 14,
            Height = 14,
            CornerRadius = new CornerRadius(7),
            BorderThickness = new Thickness(2),
            BorderBrush = new SolidColorBrush(selected ? accent : secondary),
            Background = selected ? new SolidColorBrush(accent) : Brushes.Transparent,
            Margin = new Thickness(0, 0, 7, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        if (selected)
        {
            ring.Child = new Ellipse { Width = 6, Height = 6, Fill = new SolidColorBrush(Color.FromRgb(35, 37, 42)), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        }
        TextBlock label = new TextBlock
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
            _items[i].Content = BuildVisual(text, false);
        }
    }

    private void ApplyTheme()
    {
        if (_items == null) return;
        for (int i = 0; i < _items.Count; i++)
        {
            string text = Convert.ToString(_items[i].Tag);
            _items[i].Content = BuildVisual(text, false);
        }
    }
}
