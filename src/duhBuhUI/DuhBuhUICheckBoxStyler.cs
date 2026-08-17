using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

// Reusable checkbox/toggle visuals. Uses bindings rather than named template
// targets because WPF can reject TargetName lookups when a template is applied
// programmatically before the visual tree is loaded.
public static class DuhBuhUICheckBoxStyler
{
    private static bool _initialized;
    private static readonly List<string> _checkboxKeys = new List<string>();

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(CheckBox), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnCheckBoxLoaded));
    }

    public static void RegisterCheckboxKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!_checkboxKeys.Contains(key)) _checkboxKeys.Add(key);
    }

    private static bool IsRegisteredCheckbox(CheckBox checkBox)
    {
        return checkBox != null && checkBox.Tag != null && _checkboxKeys.Contains(Convert.ToString(checkBox.Tag, CultureInfo.InvariantCulture));
    }

    public static void Apply(CheckBox checkBox)
    {
        if (checkBox == null) return;

        SolidColorBrush accent = Brush(Color.FromRgb(224, 166, 52));
        SolidColorBrush border = Brush(Color.FromRgb(75, 80, 90));
        SolidColorBrush off = Brush(Color.FromRgb(52, 56, 64));
        SolidColorBrush text = Brush(Color.FromRgb(240, 242, 245));

        checkBox.Foreground = text;
        checkBox.Focusable = true;
        checkBox.Padding = new Thickness(0);
        checkBox.Margin = new Thickness(0, 5, 0, 2);
        checkBox.FontSize = 15;
        checkBox.Template = IsRegisteredCheckbox(checkBox)
            ? CreateCheckBoxTemplate(accent, border, off, text)
            : CreateToggleTemplate(accent, border, off, text);
    }

    private static void OnCheckBoxLoaded(object sender, RoutedEventArgs e)
    {
        CheckBox checkBox = sender as CheckBox;
        if (checkBox == null) return;
        Apply(checkBox);
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Binding TemplatedBinding(string path, IValueConverter converter)
    {
        return new Binding(path)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
            Mode = BindingMode.OneWay,
            Converter = converter
        };
    }

    private static ControlTemplate CreateCheckBoxTemplate(SolidColorBrush accent, SolidColorBrush border, SolidColorBrush off, SolidColorBrush text)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory box = new FrameworkElementFactory(typeof(Border));
        box.SetValue(Border.WidthProperty, 21.0);
        box.SetValue(Border.HeightProperty, 21.0);
        box.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        box.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        box.SetValue(Border.BorderBrushProperty, border);
        box.SetBinding(Border.BackgroundProperty, TemplatedBinding("IsChecked", new CheckedBrushConverter(accent, off)));
        box.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 10, 0));
        box.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory check = new FrameworkElementFactory(typeof(TextBlock));
        check.SetValue(TextBlock.TextProperty, "✓");
        check.SetValue(TextBlock.FontSizeProperty, 15.0);
        check.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        check.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        check.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        check.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        check.SetBinding(UIElement.VisibilityProperty, TemplatedBinding("IsChecked", new CheckedVisibilityConverter()));
        box.AppendChild(check);
        panel.AppendChild(box);

        FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, TemplatedBinding("Content", null));
        label.SetBinding(TextBlock.ForegroundProperty, TemplatedBinding("Foreground", null));
        label.SetValue(TextBlock.FontSizeProperty, 15.0);
        label.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        panel.AppendChild(label);

        template.VisualTree = panel;
        return template;
    }

    private static ControlTemplate CreateToggleTemplate(SolidColorBrush accent, SolidColorBrush border, SolidColorBrush off, SolidColorBrush text)
    {
        ControlTemplate template = new ControlTemplate(typeof(CheckBox));
        FrameworkElementFactory panel = new FrameworkElementFactory(typeof(StackPanel));
        panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory track = new FrameworkElementFactory(typeof(Border));
        track.SetValue(Border.WidthProperty, 54.0);
        track.SetValue(Border.HeightProperty, 30.0);
        track.SetValue(Border.CornerRadiusProperty, new CornerRadius(15));
        track.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        track.SetBinding(Border.BackgroundProperty, TemplatedBinding("IsChecked", new CheckedBrushConverter(accent, off)));
        track.SetBinding(Border.BorderBrushProperty, TemplatedBinding("IsChecked", new CheckedBrushConverter(accent, border)));
        track.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 11, 0));
        track.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

        FrameworkElementFactory thumb = new FrameworkElementFactory(typeof(Border));
        thumb.SetValue(Border.WidthProperty, 22.0);
        thumb.SetValue(Border.HeightProperty, 22.0);
        thumb.SetValue(Border.CornerRadiusProperty, new CornerRadius(11));
        thumb.SetValue(Border.BackgroundProperty, Brushes.White);
        thumb.SetBinding(FrameworkElement.HorizontalAlignmentProperty, TemplatedBinding("IsChecked", new CheckedAlignmentConverter()));
        thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(3));
        track.AppendChild(thumb);
        panel.AppendChild(track);

        FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, TemplatedBinding("Content", null));
        label.SetBinding(TextBlock.ForegroundProperty, TemplatedBinding("Foreground", null));
        label.SetValue(TextBlock.FontSizeProperty, 15.0);
        label.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        panel.AppendChild(label);

        template.VisualTree = panel;
        return template;
    }

    private static bool IsChecked(object value)
    {
        return value is bool && (bool)value;
    }

    private sealed class CheckedBrushConverter : IValueConverter
    {
        private readonly Brush _checked;
        private readonly Brush _unchecked;
        public CheckedBrushConverter(Brush checkedBrush, Brush uncheckedBrush) { _checked = checkedBrush; _unchecked = uncheckedBrush; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return IsChecked(value) ? _checked : _unchecked; }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return Binding.DoNothing; }
    }

    private sealed class CheckedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return IsChecked(value) ? Visibility.Visible : Visibility.Collapsed; }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return Binding.DoNothing; }
    }

    private sealed class CheckedAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return IsChecked(value) ? HorizontalAlignment.Right : HorizontalAlignment.Left; }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return Binding.DoNothing; }
    }
}
