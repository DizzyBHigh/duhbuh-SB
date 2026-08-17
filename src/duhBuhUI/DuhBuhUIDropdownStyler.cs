using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Shared presentation/interaction pass for the existing WPF ComboBox dropdown control.
// This deliberately does not replace ComboBox or change its persistence contract.
public static class DuhBuhUIDropdownStyler
{
    public static void Apply(ComboBox combo, string theme, string accessibleName)
    {
        if (combo == null) return;

        bool light = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase);
        Color background = light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(43, 46, 52);
        Color popupBackground = light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(43, 46, 52);
        Color text = light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color border = light ? Color.FromRgb(170, 174, 184) : Color.FromRgb(92, 96, 105);
        Color accent = light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(115, 170, 205);
        Color hover = light ? Color.FromRgb(232, 238, 246) : Color.FromRgb(48, 83, 103);
        Color pressed = light ? Color.FromRgb(218, 226, 237) : Color.FromRgb(40, 70, 88);
        Color disabled = light ? Color.FromRgb(145, 149, 158) : Color.FromRgb(110, 114, 122);

        combo.MinHeight = 30;
        combo.MinWidth = Math.Max(180, combo.MinWidth);
        combo.Padding = new Thickness(8, 3, 8, 3);
        combo.Margin = new Thickness(0, 4, 0, 0);
        combo.HorizontalContentAlignment = HorizontalAlignment.Left;
        combo.VerticalContentAlignment = VerticalAlignment.Center;
        combo.Foreground = new SolidColorBrush(text);
        combo.Background = new SolidColorBrush(background);
        combo.BorderBrush = new SolidColorBrush(border);
        combo.BorderThickness = new Thickness(1);
        combo.Focusable = true;
        KeyboardNavigation.SetIsTabStop(combo, true);
        AutomationProperties.SetName(combo, accessibleName ?? "Dropdown");
        AutomationProperties.SetHelpText(combo, "Use arrow keys to change the selection and Alt+Down or F4 to open the list.");

        Style itemStyle = new Style(typeof(ComboBoxItem));
        itemStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(text)));
        itemStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(popupBackground)));
        itemStyle.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
        itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        itemStyle.Setters.Add(new Setter(Control.MinHeightProperty, 30.0));
        itemStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
        itemStyle.Setters.Add(new EventSetter(MouseEnterEvent, new MouseEventHandler(delegate(object sender, MouseEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            if (item != null && !item.IsSelected) item.Background = new SolidColorBrush(hover);
        })));
        itemStyle.Setters.Add(new EventSetter(MouseLeaveEvent, new MouseEventHandler(delegate(object sender, MouseEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            if (item != null && !item.IsSelected) item.Background = new SolidColorBrush(popupBackground);
        })));
        itemStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(delegate(object sender, MouseButtonEventArgs e)
        {
            ComboBoxItem item = sender as ComboBoxItem;
            if (item != null) item.Background = new SolidColorBrush(pressed);
        })));
        combo.ItemContainerStyle = itemStyle;

        combo.DropDownOpened += delegate
        {
            combo.BorderBrush = new SolidColorBrush(accent);
        };
        combo.DropDownClosed += delegate
        {
            combo.BorderBrush = new SolidColorBrush(border);
        };
        combo.GotKeyboardFocus += delegate
        {
            combo.BorderBrush = new SolidColorBrush(accent);
        };
        combo.LostKeyboardFocus += delegate
        {
            combo.BorderBrush = new SolidColorBrush(border);
        };
        combo.MouseEnter += delegate
        {
            if (!combo.IsKeyboardFocusWithin && !combo.IsDropDownOpen) combo.BorderBrush = new SolidColorBrush(accent);
        };
        combo.MouseLeave += delegate
        {
            if (!combo.IsKeyboardFocusWithin && !combo.IsDropDownOpen) combo.BorderBrush = new SolidColorBrush(border);
        };
        combo.IsEnabledChanged += delegate
        {
            if (!combo.IsEnabled)
            {
                combo.Foreground = new SolidColorBrush(disabled);
                combo.BorderBrush = new SolidColorBrush(border);
            }
            else
            {
                combo.Foreground = new SolidColorBrush(text);
            }
        };
    }
}
