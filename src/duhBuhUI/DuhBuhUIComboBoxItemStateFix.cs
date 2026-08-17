using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

// Final visual-state layer for the custom dropdown item template.
// The template owns the item visuals so WPF theme selection brushes cannot
// reintroduce the default blue/grey states.
public static class DuhBuhUIComboBoxItemStateFix
{
    private static readonly Color Accent = Color.FromRgb(224, 166, 52);
    private static readonly Color DarkPopup = Color.FromRgb(30, 33, 39);
    private static readonly Color DarkText = Color.FromRgb(240, 242, 245);
    private static readonly Color LightPopup = Color.FromRgb(255, 255, 255);
    private static readonly Color LightText = Color.FromRgb(30, 32, 38);

    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        window.Dispatcher.BeginInvoke(new Action(() => Apply(window)), DispatcherPriority.Loaded);
    }

    private static void Apply(Window window)
    {
        bool light = IsLightWindow(window);
        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? LightPopup : DarkPopup)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? LightText : DarkText)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 6, 9, 6)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 30.0));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateTemplate(light)));
        window.Resources[typeof(ComboBoxItem)] = style;
    }

    private static ControlTemplate CreateTemplate(bool light)
    {
        Color normal = light ? LightPopup : DarkPopup;
        Color normalText = light ? LightText : DarkText;

        ControlTemplate template = new ControlTemplate(typeof(ComboBoxItem));

        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.Name = "ItemBorder";
        border.SetValue(Border.BackgroundProperty, Brush(normal));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        border.SetValue(Border.SnapsToDevicePixelsProperty, true);

        FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.Name = "ItemContent";
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        presenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        presenter.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding("Content")
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new System.Windows.Data.Binding("ContentTemplate")
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetBinding(ContentPresenter.ContentStringFormatProperty, new System.Windows.Data.Binding("ContentStringFormat")
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding("Padding")
        {
            RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
        });
        presenter.SetValue(TextElement.ForegroundProperty, Brush(normalText));
        border.AppendChild(presenter);

        template.VisualTree = border;

        Trigger selected = new Trigger
        {
            Property = Selector.IsSelectedProperty,
            Value = true
        };
        selected.Setters.Add(new Setter(Border.BackgroundProperty, Brush(Accent), "ItemBorder"));
        selected.Setters.Add(new Setter(TextElement.ForegroundProperty, Brushes.White, "ItemContent"));

        Trigger mouseOver = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };
        mouseOver.Setters.Add(new Setter(Border.BackgroundProperty, Brush(Accent), "ItemBorder"));
        mouseOver.Setters.Add(new Setter(TextElement.ForegroundProperty, Brushes.White, "ItemContent"));

        Trigger highlighted = new Trigger
        {
            Property = ComboBoxItem.IsHighlightedProperty,
            Value = true
        };
        highlighted.Setters.Add(new Setter(Border.BackgroundProperty, Brush(Accent), "ItemBorder"));
        highlighted.Setters.Add(new Setter(TextElement.ForegroundProperty, Brushes.White, "ItemContent"));

        template.Triggers.Add(selected);
        template.Triggers.Add(mouseOver);
        template.Triggers.Add(highlighted);

        return template;
    }

    private static bool IsLightWindow(Window window)
    {
        SolidColorBrush brush = window.Background as SolidColorBrush;
        Color color = brush != null ? brush.Color : SystemColors.WindowColor;
        double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        return luminance >= 0.62;
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
