// duhBuhUITheme - safe visual styling for the shared settings UI.
// IMPORTANT: Do not replace TabControl/TabItem templates or modify generated tab templates.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DuhBuhUITheme
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded));
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        if (window.Title == null || window.Title.IndexOf("Settings", StringComparison.OrdinalIgnoreCase) < 0) return;

        Apply(window, IsLightWindow(window));
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;

        ResourceDictionary r = window.Resources;
        r[typeof(Button)] = CreateButtonStyle(light);
        r[typeof(TextBox)] = CreateTextBoxStyle(light);
        r[typeof(ComboBox)] = CreateComboBoxStyle(light);
        r[typeof(DatePicker)] = CreateDatePickerStyle(light);
        r[typeof(CheckBox)] = CreateCheckBoxStyle(light);
        r[typeof(RadioButton)] = CreateRadioButtonStyle(light);

        r["duhBuhSectionBackground"] = new SolidColorBrush(light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(32, 35, 41));
        r["duhBuhSectionBorder"] = new SolidColorBrush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74));
        r["duhBuhAccent"] = new SolidColorBrush(light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 140, 205));
        r["duhBuhSectionText"] = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
        r["duhBuhDescriptionText"] = new SolidColorBrush(light ? Color.FromRgb(100, 106, 118) : Color.FromRgb(160, 167, 178));

        // Existing controls have already been created by DuhBuhUI by the time
        // the Window Loaded event reaches this handler. Defer one dispatcher
        // turn so the generated tab content is present before we group it.
        window.Dispatcher.BeginInvoke(new Action(delegate
        {
            ApplySectionCards(window, light);
        }));
    }

    private static void ApplySectionCards(Window window, bool light)
    {
        TabControl tabs = FindTabControl(window);
        if (tabs == null) return;

        for (int i = 0; i < tabs.Items.Count; i++)
        {
            TabItem tab = tabs.Items[i] as TabItem;
            if (tab == null) continue;

            ScrollViewer scroll = tab.Content as ScrollViewer;
            if (scroll == null) continue;

            StackPanel category = scroll.Content as StackPanel;
            if (category == null) continue;

            ApplyCardsToCategory(category, light);
        }
    }

    private static TabControl FindTabControl(DependencyObject parent)
    {
        if (parent == null) return null;
        TabControl direct = parent as TabControl;
        if (direct != null) return direct;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            TabControl found = FindTabControl(VisualTreeHelper.GetChild(parent, i));
            if (found != null) return found;
        }

        return null;
    }

    private static void ApplyCardsToCategory(StackPanel category, bool light)
    {
        // Do not process a category twice.
        if (category.Tag is string && (string)category.Tag == "__duhbuh_cards_applied") return;
        category.Tag = "__duhbuh_cards_applied";

        int index = 0;
        while (index < category.Children.Count)
        {
            TextBlock heading = category.Children[index] as TextBlock;
            if (!IsSectionHeading(heading))
            {
                index++;
                continue;
            }

            int nextHeading = index + 1;
            while (nextHeading < category.Children.Count)
            {
                TextBlock candidate = category.Children[nextHeading] as TextBlock;
                if (IsSectionHeading(candidate)) break;
                nextHeading++;
            }

            category.Children.RemoveAt(index);

            StackPanel content = new StackPanel();
            content.Children.Add(heading);

            StackPanel fields = new StackPanel();
            while (index < nextHeading - 1 && index < category.Children.Count)
            {
                UIElement child = category.Children[index];
                category.Children.RemoveAt(index);
                fields.Children.Add(child);
                nextHeading--;
            }

            content.Children.Add(fields);

            Border card = new Border
            {
                Background = new SolidColorBrush(light ? Color.FromRgb(252, 253, 255) : Color.FromRgb(39, 42, 48)),
                BorderBrush = new SolidColorBrush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14, 8, 14, 10),
                Margin = new Thickness(0, 0, 0, 14),
                Child = content
            };

            heading.Foreground = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
            heading.Background = new SolidColorBrush(light ? Color.FromRgb(238, 241, 246) : Color.FromRgb(43, 47, 54));
            heading.Padding = new Thickness(10, 7, 10, 7);
            heading.Margin = new Thickness(0, 0, 0, 10);
            heading.HorizontalAlignment = HorizontalAlignment.Stretch;

            category.Children.Insert(index, card);
            index++;
        }
    }

    private static bool IsSectionHeading(TextBlock text)
    {
        return text != null && text.FontSize >= 17 && text.FontWeight == FontWeights.SemiBold;
    }

    private static bool IsLightWindow(Window window)
    {
        if (window == null) return false;
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color c = brush.Color;
        return c.R > 180 && c.G > 180 && c.B > 180;
    }

    private static Style CreateButtonStyle(bool light)
    {
        Color background = light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 110, 180);
        Color border = light ? Color.FromRgb(32, 68, 125) : Color.FromRgb(90, 140, 205);
        Style style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(background)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14, 7, 14, 7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4, 3, 4, 3)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        return style;
    }

    private static Style CreateTextBoxStyle(bool light)
    {
        Style style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style style = new Style(typeof(DatePicker));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateCheckBoxStyle(bool light)
    {
        Style style = new Style(typeof(CheckBox));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 4, 0, 4)));
        return style;
    }

    private static Style CreateRadioButtonStyle(bool light)
    {
        Style style = new Style(typeof(RadioButton));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 3, 0, 3)));
        return style;
    }
}
