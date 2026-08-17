using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Compatibility/registration shim for the custom CheckBox control.
// Registered keys render as square checkboxes; ordinary toggle keys render as switches.
public static class DuhBuhUICheckBoxStyler
{
    private static readonly List<string> _checkboxKeys = new List<string>();
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(StackPanel), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnStackPanelLoaded));
        EventManager.RegisterClassHandler(typeof(Button), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnButtonPreviewMouseDown), true);
        DuhBuhUIRadioStyler.Initialize();
    }

    public static void RegisterCheckboxKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!_checkboxKeys.Contains(key)) _checkboxKeys.Add(key);
    }

    public static bool IsRegisteredCheckbox(object tag)
    {
        string key = Convert.ToString(tag);
        return !string.IsNullOrEmpty(key) && _checkboxKeys.Contains(key);
    }

    public static void Apply(CheckBox checkBox)
    {
        // The custom CheckBox owns rendering. Registration above tells it which
        // visual mode to use without replacing the control instance.
    }

    private static void OnStackPanelLoaded(object sender, RoutedEventArgs e)
    {
        StackPanel row = sender as StackPanel;
        if (row == null || row.Children.Count == 0) return;

        Button swatch = null;
        TextBox hex = null;
        Button choose = null;
        for (int i = 0; i < row.Children.Count; i++)
        {
            Button button = row.Children[i] as Button;
            if (button != null)
            {
                if (button.ToolTip != null && string.Equals(Convert.ToString(button.ToolTip), "Click to choose a colour", StringComparison.Ordinal)) swatch = button;
                if (button.Content != null && string.Equals(Convert.ToString(button.Content), "Choose…", StringComparison.Ordinal)) choose = button;
            }
            TextBox textBox = row.Children[i] as TextBox;
            if (textBox != null) hex = textBox;
        }

        if (swatch == null || hex == null || choose == null) return;
        if (row.Children.Count == 1 && row.Children[0] is DuhBuhUICustomColorPicker) return;

        string key = Convert.ToString(row.Tag);
        if (string.IsNullOrEmpty(key)) return;

        DuhBuhUICustomColorPicker picker = new DuhBuhUICustomColorPicker
        {
            Tag = key,
            Value = hex.Text,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 4, 0, 0)
        };

        // Keep a zero-size tagged TextBox in the logical tree so the existing
        // DuhBuhUI Save traversal can persist the custom control's value without
        // changing the public settings API.
        TextBox persistence = new TextBox
        {
            Tag = key,
            Text = picker.Value,
            Width = 0,
            Height = 0,
            Opacity = 0,
            IsTabStop = false,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0)
        };

        row.Children.Clear();
        row.Children.Add(picker);
        row.Children.Add(persistence);
    }

    private static void OnButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Button button = sender as Button;
        if (button == null || button.Content == null) return;
        string content = Convert.ToString(button.Content);
        if (!string.Equals(content, "Save", StringComparison.Ordinal) && !string.Equals(content, "Save & Exit", StringComparison.Ordinal)) return;

        DependencyObject root = button;
        DependencyObject parent;
        while ((parent = VisualTreeHelper.GetParent(root)) != null) root = parent;
        SyncColorPersistence(root);
    }

    private static void SyncColorPersistence(DependencyObject parent)
    {
        DuhBuhUICustomColorPicker picker = parent as DuhBuhUICustomColorPicker;
        if (picker != null)
        {
            string key = Convert.ToString(picker.Tag);
            if (!string.IsNullOrEmpty(key))
            {
                DependencyObject row = VisualTreeHelper.GetParent(picker);
                if (row != null)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(row); i++)
                    {
                        TextBox text = VisualTreeHelper.GetChild(row, i) as TextBox;
                        if (text != null && string.Equals(Convert.ToString(text.Tag), key, StringComparison.Ordinal)) text.Text = picker.Value;
                    }
                }
            }
        }

        int count;
        try { count = VisualTreeHelper.GetChildrenCount(parent); }
        catch { return; }
        for (int i = 0; i < count; i++) SyncColorPersistence(VisualTreeHelper.GetChild(parent, i));
    }
}
