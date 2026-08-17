using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Compatibility surface for DuhBuhUI.cs: deliberately NOT the WPF ComboBox.
// The existing framework can keep its simple ComboBox construction syntax while
// this type supplies consistent visuals and interaction for the dropdown control.
public class ComboBox : System.Windows.Controls.ComboBox
{
    private bool _lightTheme;
    private bool _themeSynced;
    private bool _hover;
    private bool _focused;
    private bool _open;
    private bool _pressed;

    // Accessibility/sizing hooks: compact visual with a usable hit target.
    public double HitTargetHeight { get; set; } = 30;
    public double HitTargetMinWidth { get; set; } = 180;

    public ComboBox()
    {
        Focusable = true;
        KeyboardNavigation.SetIsTabStop(this, true);
        Padding = new Thickness(8, 3, 8, 3);
        Margin = new Thickness(0, 4, 0, 0);
        MinHeight = HitTargetHeight;
        MinWidth = HitTargetMinWidth;
        HorizontalContentAlignment = HorizontalAlignment.Left;
        VerticalContentAlignment = VerticalAlignment.Center;

        Loaded += delegate { SyncThemeFromWindow(); ApplyVisuals(); };
        DropDownOpened += delegate { _open = true; _focused = true; ApplyVisuals(); };
        DropDownClosed += delegate { _open = false; ApplyVisuals(); };
        GotKeyboardFocus += delegate { _focused = true; ApplyVisuals(); };
        LostKeyboardFocus += delegate { _focused = false; ApplyVisuals(); };
        MouseEnter += delegate { _hover = true; ApplyVisuals(); };
        MouseLeave += delegate { _hover = false; _pressed = false; ApplyVisuals(); };
        PreviewMouseLeftButtonDown += delegate { _pressed = true; Focus(); ApplyVisuals(); };
        PreviewMouseLeftButtonUp += delegate { _pressed = false; ApplyVisuals(); };
        IsEnabledChanged += delegate { ApplyVisuals(); };
    }

    private void SyncThemeFromWindow()
    {
        if (_themeSynced) return;
        _themeSynced = true;
        Window window = Window.GetWindow(this);
        Brush brush = window == null ? null : window.Background;
        SolidColorBrush solid = brush as SolidColorBrush;
        if (solid == null)
        {
            Color system = SystemColors.WindowColor;
            _lightTheme = system.R > 150 && system.G > 150 && system.B > 150;
        }
        else
        {
            Color c = solid.Color;
            double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
            _lightTheme = luminance >= 0.62;
        }
    }

    private void ApplyVisuals()
    {
        Color background = _lightTheme ? Color.FromRgb(255, 255, 255) : Color.FromRgb(43, 46, 52);
        Color text = _lightTheme ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color border = _lightTheme ? Color.FromRgb(170, 174, 184) : Color.FromRgb(92, 96, 105);
        Color accent = _lightTheme ? Color.FromRgb(44, 90, 160) : Color.FromRgb(115, 170, 205);
        Color hover = _lightTheme ? Color.FromRgb(232, 238, 246) : Color.FromRgb(48, 83, 103);
        Color pressed = _lightTheme ? Color.FromRgb(218, 226, 237) : Color.FromRgb(40, 70, 88);
        Color disabled = _lightTheme ? Color.FromRgb(145, 149, 158) : Color.FromRgb(110, 114, 122);

        MinHeight = Math.Max(30, HitTargetHeight);
        MinWidth = Math.Max(180, HitTargetMinWidth);
        Foreground = new SolidColorBrush(IsEnabled ? text : disabled);
        Background = new SolidColorBrush(_pressed ? pressed : background);
        BorderBrush = new SolidColorBrush((_focused || _hover || _open) ? accent : border);
        BorderThickness = new Thickness(1);

        AutomationProperties.SetName(this, "Dropdown");
        AutomationProperties.SetHelpText(this, "Use arrow keys to change the selection and Alt+Down or F4 to open the list.");

        if (ItemContainerStyle == null)
        {
            Style itemStyle = new Style(typeof(System.Windows.Controls.ComboBoxItem));
            itemStyle.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(text)));
            itemStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(background)));
            itemStyle.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
            itemStyle.Setters.Add(new Setter(Control.MinHeightProperty, 30.0));
            itemStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
            itemStyle.Setters.Add(new EventSetter(MouseEnterEvent, new MouseEventHandler(delegate(object sender, MouseEventArgs e)
            {
                System.Windows.Controls.ComboBoxItem item = sender as System.Windows.Controls.ComboBoxItem;
                if (item != null && !item.IsSelected) item.Background = new SolidColorBrush(hover);
            })));
            itemStyle.Setters.Add(new EventSetter(MouseLeaveEvent, new MouseEventHandler(delegate(object sender, MouseEventArgs e)
            {
                System.Windows.Controls.ComboBoxItem item = sender as System.Windows.Controls.ComboBoxItem;
                if (item != null && !item.IsSelected) item.Background = new SolidColorBrush(background);
            })));
            itemStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(delegate(object sender, MouseButtonEventArgs e)
            {
                System.Windows.Controls.ComboBoxItem item = sender as System.Windows.Controls.ComboBoxItem;
                if (item != null) item.Background = new SolidColorBrush(pressed);
            })));
            ItemContainerStyle = itemStyle;
        }
    }
}
