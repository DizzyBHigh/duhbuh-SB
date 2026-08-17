using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

// Shared custom ComboBox appearance for duhBuhUI. WPF supplies the control
// plumbing, while this template owns the field, arrow, popup and item visuals.
public static class DuhBuhUIComboBoxTheme
{
    public static void Apply(Window window, bool light)
    {
        if (window == null) return;
        window.Resources[typeof(ComboBox)] = CreateComboStyle(light);
        window.Resources[typeof(ComboBoxItem)] = CreateItemStyle(light);
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Style CreateComboStyle(bool light)
    {
        Color background = light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(38, 41, 48);
        Color foreground = light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color border = light ? Color.FromRgb(170, 176, 186) : Color.FromRgb(75, 80, 90);
        Color hover = light ? Color.FromRgb(235, 240, 248) : Color.FromRgb(52, 56, 65);
        Color accent = Color.FromRgb(224, 166, 52);

        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(background)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(foreground)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.HeightProperty, 34.0));
        style.Setters.Add(new Setter(Control.MinWidthProperty, 90.0));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 5, 34, 5)));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateComboTemplate(background, foreground, border, accent)));

        Trigger mouseOver = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        mouseOver.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accent)));
        style.Triggers.Add(mouseOver);

        return style;
    }

    private static ControlTemplate CreateComboTemplate(Color background, Color foreground, Color border, Color accent)
    {
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
        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetBinding(ContentPresenter.ContentProperty, new Binding("SelectedItem") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        content.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding("SelectionBoxItemTemplate") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
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
        popupBorder.SetValue(Border.BackgroundProperty, Brush(background));
        popupBorder.SetValue(Border.BorderBrushProperty, Brush(accent));
        popupBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        popupBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        popupBorder.SetValue(Border.PaddingProperty, new Thickness(2));
        popupBorder.SetValue(Border.MinWidthProperty, 90.0);

        FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer));
        scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        scroll.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
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
        Color normal = light ? Color.FromRgb(255, 255, 255) : Color.FromRgb(38, 41, 48);
        Color text = light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245);
        Color hover = Color.FromRgb(224, 166, 52);

        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normal)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(text)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));

        Trigger mouseOver = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        mouseOver.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hover)));
        mouseOver.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Triggers.Add(mouseOver);

        Trigger selected = new Trigger { Property = Selector.IsSelectedProperty, Value = true };
        selected.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hover)));
        selected.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Triggers.Add(selected);
        return style;
    }

    private static Binding TemplatedBinding(string path)
    {
        return new Binding(path) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) };
    }
}
