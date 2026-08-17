using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DuhBuhUIRadioStyler
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(StackPanel), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnStackPanelLoaded));
    }

    private static void OnStackPanelLoaded(object sender, RoutedEventArgs e)
    {
        StackPanel row = sender as StackPanel;
        if (row == null || row.Children.Count == 0) return;

        for (int i = 0; i < row.Children.Count; i++)
        {
            StackPanel stack = row.Children[i] as StackPanel;
            if (stack != null && HasRadioChildren(stack))
            {
                ReplaceGroup(row, stack, null);
                continue;
            }

            Grid grid = row.Children[i] as Grid;
            if (grid != null && grid.Tag != null && HasRadioChildren(grid))
            {
                ReplaceGroup(row, grid, 3);
            }
        }
    }

    private static bool HasRadioChildren(Panel panel)
    {
        for (int i = 0; i < panel.Children.Count; i++) if (panel.Children[i] is RadioButton) return true;
        return false;
    }

    private static void ReplaceGroup(Panel parent, Panel original, int? columns)
    {
        if (original.Tag == null) return;
        if (original.Visibility == Visibility.Collapsed) return;

        List<RadioButton> radios = new List<RadioButton>();
        for (int i = 0; i < original.Children.Count; i++)
        {
            RadioButton radio = original.Children[i] as RadioButton;
            if (radio != null) radios.Add(radio);
        }
        if (radios.Count == 0) return;

        string[] options = new string[radios.Count];
        string selected = "";
        for (int i = 0; i < radios.Count; i++)
        {
            options[i] = radios[i].Content == null ? "" : radios[i].Content.ToString();
            if (radios[i].IsChecked == true) selected = options[i];
        }

        bool light = IsLightTheme(parent);
        DuhBuhUICustomRadioGroup custom = new DuhBuhUICustomRadioGroup
        {
            Tag = original.Tag,
            Options = options,
            SelectedItem = selected,
            IsLightTheme = light,
            Margin = new Thickness(0, 5, 0, 0)
        };
        if (columns.HasValue && options.Length != 9) columns = null;

        custom.SelectionChanged += delegate
        {
            for (int i = 0; i < radios.Count; i++) radios[i].IsChecked = string.Equals(options[i], custom.SelectedItem, StringComparison.Ordinal);
        };

        int index = parent.Children.IndexOf(original);
        original.Visibility = Visibility.Collapsed;
        if (index < 0) parent.Children.Add(custom); else parent.Children.Insert(index, custom);
    }

    private static bool IsLightTheme(DependencyObject element)
    {
        DependencyObject current = element;
        while (current != null)
        {
            Panel panel = current as Panel;
            if (panel != null && panel.Background is SolidColorBrush)
            {
                Color c = ((SolidColorBrush)panel.Background).Color;
                if (c.R > 220 && c.G > 220 && c.B > 220) return true;
                if (c.R < 80 && c.G < 80 && c.B < 80) return false;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}
