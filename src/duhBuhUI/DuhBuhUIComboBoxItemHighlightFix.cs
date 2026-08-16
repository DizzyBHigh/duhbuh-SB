using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;

internal static class DuhBuhUIComboBoxItemHighlightFix
{
    private static readonly SolidColorBrush Gold = CreateBrush(Color.FromRgb(224, 166, 52));

    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(ComboBoxItem), UIElement.MouseEnterEvent, new MouseEventHandler(OnMouseEnter));
        EventManager.RegisterClassHandler(typeof(ComboBoxItem), UIElement.MouseLeaveEvent, new MouseEventHandler(OnMouseLeave));
        EventManager.RegisterClassHandler(typeof(ComboBoxItem), Selector.SelectedEvent, new RoutedEventHandler(OnSelected));
        EventManager.RegisterClassHandler(typeof(ComboBoxItem), Selector.UnselectedEvent, new RoutedEventHandler(OnUnselected));
    }

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        ComboBoxItem item = sender as ComboBoxItem;
        if (item == null) return;
        ApplyGold(item);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        ComboBoxItem item = sender as ComboBoxItem;
        if (item == null) return;
        ApplyNormal(item);
    }

    private static void OnSelected(object sender, RoutedEventArgs e)
    {
        ComboBoxItem item = sender as ComboBoxItem;
        if (item == null) return;
        ApplyGold(item);
    }

    private static void OnUnselected(object sender, RoutedEventArgs e)
    {
        ComboBoxItem item = sender as ComboBoxItem;
        if (item == null) return;
        ApplyNormal(item);
    }

    private static void ApplyGold(ComboBoxItem item)
    {
        item.SetValue(Control.BackgroundProperty, Gold);
        item.SetValue(Control.ForegroundProperty, Brushes.White);
    }

    private static void ApplyNormal(ComboBoxItem item)
    {
        ComboBox combo = FindParentComboBox(item);
        if (combo != null)
        {
            item.SetValue(Control.BackgroundProperty, combo.Background);
            item.SetValue(Control.ForegroundProperty, combo.Foreground);
        }
    }

    private static ComboBox FindParentComboBox(DependencyObject child)
    {
        DependencyObject current = child;
        while (current != null)
        {
            ComboBox combo = current as ComboBox;
            if (combo != null) return combo;
            current = GetParent(current);
        }
        return null;
    }

    private static DependencyObject GetParent(DependencyObject child)
    {
        DependencyObject visual = null;
        if (child is Visual || child is Visual3D)
            visual = VisualTreeHelper.GetParent(child);
        if (visual != null) return visual;

        FrameworkElement element = child as FrameworkElement;
        if (element != null && element.Parent != null) return element.Parent;

        FrameworkContentElement content = child as FrameworkContentElement;
        return content != null ? content.Parent : null;
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
