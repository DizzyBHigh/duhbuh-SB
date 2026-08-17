using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

// Reusable textbox visuals. The native TextBox supplies text editing, selection,
// keyboard and clipboard behavior; the complete visual template is owned here.
public static class DuhBuhUITextBoxStyler
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(TextBox), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnTextBoxLoaded));
    }

    private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        if (textBox == null) return;
        Apply(textBox);
    }

    public static void Apply(TextBox textBox)
    {
        if (textBox == null) return;

        textBox.Foreground = Brush(Color.FromRgb(240, 242, 245));
        textBox.Background = Brush(Color.FromRgb(36, 39, 45));
        textBox.BorderThickness = new Thickness(0);
        textBox.Padding = new Thickness(10, 5, 10, 5);
        textBox.FontSize = 14;
        textBox.SelectionBrush = Brush(Color.FromRgb(224, 166, 52));
        textBox.SelectionTextBrush = Brushes.White;
        textBox.CaretBrush = Brush(Color.FromRgb(224, 166, 52));
        textBox.MinHeight = textBox.AcceptsReturn ? 68 : 34;
        textBox.Template = CreateTemplate();
    }

    private static ControlTemplate CreateTemplate()
    {
        SolidColorBrush background = Brush(Color.FromRgb(36, 39, 45));
        SolidColorBrush border = Brush(Color.FromRgb(75, 80, 90));
        SolidColorBrush accent = Brush(Color.FromRgb(224, 166, 52));

        ControlTemplate template = new ControlTemplate(typeof(TextBox));

        FrameworkElementFactory frame = new FrameworkElementFactory(typeof(Border));
        frame.SetValue(Border.BackgroundProperty, background);
        frame.SetBinding(Border.BorderBrushProperty, new Binding("IsKeyboardFocusWithin")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
            Mode = BindingMode.OneWay,
            Converter = new FocusBorderConverter(accent, border)
        });
        frame.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        frame.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));

        FrameworkElementFactory host = new FrameworkElementFactory(typeof(ScrollViewer));
        host.SetValue(FrameworkElement.NameProperty, "PART_ContentHost");
        host.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        host.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        host.SetValue(ScrollViewer.CanContentScrollProperty, false);
        host.SetValue(FrameworkElement.MarginProperty, new Thickness(0));

        frame.AppendChild(host);
        template.VisualTree = frame;
        return template;
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private sealed class FocusBorderConverter : IValueConverter
    {
        private readonly Brush _focused;
        private readonly Brush _normal;

        public FocusBorderConverter(Brush focused, Brush normal)
        {
            _focused = focused;
            _normal = normal;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && (bool)value ? _focused : _normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
