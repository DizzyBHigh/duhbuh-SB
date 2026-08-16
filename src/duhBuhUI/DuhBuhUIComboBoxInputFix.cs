using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : Attribute
    {
    }
}
#endif

internal static class DuhBuhUIComboBoxInputFix
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(
            typeof(Window),
            UIElement.PreviewMouseLeftButtonDownEvent,
            new System.Windows.Input.MouseButtonEventHandler(OnWindowPreviewMouseLeftButtonDown),
            true);
    }

    private static void OnWindowPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ComboBox combo = FindComboBox(e.OriginalSource as DependencyObject);
        if (combo == null || !combo.IsEnabled || combo.IsDropDownOpen)
            return;

        combo.Focus();
        combo.IsDropDownOpen = true;
        e.Handled = true;
    }

    private static ComboBox FindComboBox(DependencyObject source)
    {
        DependencyObject current = source;
        while (current != null)
        {
            ComboBox combo = current as ComboBox;
            if (combo != null)
                return combo;

            FrameworkElement element = current as FrameworkElement;
            if (element != null && element.TemplatedParent != null)
            {
                ComboBox templatedCombo = element.TemplatedParent as ComboBox;
                if (templatedCombo != null)
                    return templatedCombo;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static DependencyObject GetParent(DependencyObject child)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        if (parent != null)
            return parent;

        FrameworkElement element = child as FrameworkElement;
        if (element != null && element.Parent != null)
            return element.Parent;

        FrameworkContentElement content = child as FrameworkContentElement;
        if (content != null)
            return content.Parent;

        return null;
    }
}
