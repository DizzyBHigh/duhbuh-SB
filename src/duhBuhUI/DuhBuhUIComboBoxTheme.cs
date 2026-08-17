using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

// Shared custom ComboBox appearance for duhBuhUI. WPF supplies the control
// plumbing, while this template owns the field, arrow, popup and item visuals.
public static class DuhBuhUIComboBoxTheme
{
    private static readonly Color DarkBackground = Color.FromRgb(38, 41, 48);
    private static readonly Color DarkPopup = Color.FromRgb(30, 33, 39);
    private static readonly Color DarkText = Color.FromRgb(240, 242, 245);
    private static readonly Color DarkBorder = Color.FromRgb(75, 80, 90);
    private static readonly Color Accent = Color.FromRgb(224, 166, 52);

    [ModuleInitializer]
    internal static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;

        window.Resources[typeof(ComboBox)] = CreateComboStyle(light);
        window.Resources[typeof(ComboBoxItem)] = CreateItemStyle(light);
        window.Resources[typeof(ScrollBar)] = CreateScrollBarStyle(light);
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        Apply(window, IsLightWindow(window));
    }

    private static bool IsLightWindow(Window window)
    {
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;

        Color color = brush.Color;
        return color.R > 180 && color.G > 180 && color.B > 180;
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Style CreateComboStyle(bool light)
    {
        Color background = light ? Color.FromRgb(255, 255, 255) : DarkBackground;
        Color foreground = light ? Color.FromRgb(30, 32, 38) : DarkText;
        Color border = light ? Color.FromRgb(170, 176, 186) : DarkBorder;

        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(background)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(foreground)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.HeightProperty, 34.0));
        style.Setters.Add(new Setter(Control.MinWidthProperty, 90.0));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 5, 34, 5)));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateComboTemplate(foreground, Accent, light)));

        Trigger mouseOver = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };
        mouseOver.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(Accent)));
        style.Triggers.Add(mouseOver);

        return style;
    }

    private static ControlTemplate CreateComboTemplate(Color foreground, Color accent, bool light)
    {
        Color popupBackground = light ? Color.FromRgb(255, 255, 255) : DarkPopup;

        ControlTemplate template = new ControlTemplate(typeof(ComboBox));
        FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

        FrameworkElementFactory field = new FrameworkElementFactory(typeof(Border));
        field.SetBinding(Border.BackgroundProperty, TemplatedBinding("Background"));
        field.SetBinding(Border.BorderBrushProperty, TemplatedBinding("BorderBrush"));
        field.SetBinding(Border.BorderThicknessProperty, TemplatedBinding("BorderThickness"));
        field.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        grid.AppendChild(field);

        FrameworkElementFactory toggle = new FrameworkElementFactory(typeof(ToggleButton));
        toggle.SetBinding(ToggleButton.IsCheckedProperty, TemplatedBinding("IsDropDownOpen"));
        toggle.SetValue(Control.BackgroundProperty, Brushes.Transparent);
        toggle.SetValue(Control.BorderThicknessProperty, new Thickness(0));
        toggle.SetValue(Control.PaddingProperty, new Thickness(0));
        toggle.SetValue(Control.FocusableProperty, false);

        ControlTemplate toggleTemplate = new ControlTemplate(typeof(ToggleButton));
        FrameworkElementFactory toggleGrid = new FrameworkElementFactory(typeof(Grid));
        RelativeSource comboSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1);

        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetBinding(ContentPresenter.ContentProperty, new Binding("SelectedItem") { RelativeSource = comboSource });
        content.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding("SelectionBoxItemTemplate") { RelativeSource = comboSource });
        content.SetBinding(ContentPresenter.ContentStringFormatProperty, new Binding("SelectionBoxItemStringFormat") { RelativeSource = comboSource });
        content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(ContentPresenter.MarginProperty, new Thickness(9, 0, 34, 0));
        toggleGrid.AppendChild(content);

        FrameworkElementFactory arrow = new FrameworkElementFactory(typeof(TextBlock));
        arrow.SetValue(TextBlock.TextProperty, "▼");
        arrow.SetValue(TextBlock.FontSizeProperty, 9.0);
        arrow.SetValue(TextBlock.ForegroundProperty, Brush(foreground));
        arrow.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right);
        arrow.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        arrow.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 9, 0));
        toggleGrid.AppendChild(arrow);

        toggleTemplate.VisualTree = toggleGrid;
        toggle.SetValue(Control.TemplateProperty, toggleTemplate);
        grid.AppendChild(toggle);

        FrameworkElementFactory popup = new FrameworkElementFactory(typeof(Popup));
        popup.SetBinding(Popup.IsOpenProperty, TemplatedBinding("IsDropDownOpen"));
        popup.SetValue(Popup.PlacementProperty, PlacementMode.Bottom);
        popup.SetValue(Popup.StaysOpenProperty, false);
        popup.SetValue(Popup.AllowsTransparencyProperty, true);

        FrameworkElementFactory popupBorder = new FrameworkElementFactory(typeof(Border));
        popupBorder.SetValue(Border.BackgroundProperty, Brush(popupBackground));
        popupBorder.SetValue(Border.BorderBrushProperty, Brush(accent));
        popupBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        popupBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        popupBorder.SetValue(Border.PaddingProperty, new Thickness(2));
        popupBorder.SetValue(Border.MinWidthProperty, 90.0);

        FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer));
        scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        scroll.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
        scroll.SetValue(ScrollViewer.CanContentScrollProperty, true);
        scroll.SetBinding(ScrollViewer.MaxHeightProperty, new Binding("MaxDropDownHeight")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
        });

        FrameworkElementFactory items = new FrameworkElementFactory(typeof(ItemsPresenter));
        scroll.AppendChild(items);
        popupBorder.AppendChild(scroll);
        popup.AppendChild(popupBorder);
        grid.AppendChild(popup);

        template.VisualTree = grid;
        return template;
    }

    private static Style CreateItemStyle(bool light)
    {
        Color normal = light ? Color.FromRgb(255, 255, 255) : DarkPopup;
        Color text = light ? Color.FromRgb(30, 32, 38) : DarkText;

        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normal)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(text)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 6, 9, 6)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateItemTemplate()));

        Trigger selected = new Trigger
        {
            Property = Selector.IsSelectedProperty,
            Value = true
        };
        selected.Setters.Add(new Setter(Control.BackgroundProperty, Brush(Color.FromRgb(58, 66, 76))));
        selected.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Triggers.Add(selected);

        Trigger mouseOver = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };
        mouseOver.Setters.Add(new Setter(Control.BackgroundProperty, Brush(Accent)));
        mouseOver.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Triggers.Add(mouseOver);

        return style;
    }

    private static ControlTemplate CreateItemTemplate()
    {
        ControlTemplate template = new ControlTemplate(typeof(ComboBoxItem));
        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, TemplatedBinding("Background"));
        border.SetBinding(Border.BorderBrushProperty, TemplatedBinding("BorderBrush"));
        border.SetBinding(Border.BorderThicknessProperty, TemplatedBinding("BorderThickness"));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        border.SetValue(Border.SnapsToDevicePixelsProperty, true);

        FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.ContentProperty, TemplatedBinding("Content"));
        presenter.SetBinding(ContentPresenter.ContentTemplateProperty, TemplatedBinding("ContentTemplate"));
        presenter.SetBinding(ContentPresenter.ContentStringFormatProperty, TemplatedBinding("ContentStringFormat"));
        presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, TemplatedBinding("HorizontalContentAlignment"));
        presenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, TemplatedBinding("VerticalContentAlignment"));
        presenter.SetBinding(ContentPresenter.MarginProperty, TemplatedBinding("Padding"));
        presenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        border.AppendChild(presenter);

        template.VisualTree = border;
        return template;
    }

    private static Style CreateScrollBarStyle(bool light)
    {
        Color track = light ? Color.FromRgb(235, 237, 241) : Color.FromRgb(27, 29, 34);
        Color thumb = light ? Color.FromRgb(170, 176, 186) : Color.FromRgb(82, 88, 98);

        Style style = new Style(typeof(ScrollBar));
        style.Setters.Add(new Setter(Control.WidthProperty, 10.0));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(track)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(thumb)));
        return style;
    }

    private static Binding TemplatedBinding(string path)
    {
        return new Binding(path)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
        };
    }
}
