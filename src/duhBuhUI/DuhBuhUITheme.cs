// duhBuhUITheme - safe visual styling for the shared settings UI.
// IMPORTANT: Do not replace TabControl/TabItem templates or modify generated tab templates.

using System;
using System.Collections.Generic;
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
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
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
        r[typeof(ComboBoxItem)] = CreateComboBoxItemStyle(light);
        r[typeof(DatePicker)] = CreateDatePickerStyle(light);
        r[typeof(CheckBox)] = CreateCheckBoxStyle(light);
        r[typeof(RadioButton)] = CreateRadioButtonStyle(light);
        r[typeof(TabItem)] = CreateTabItemStyle(light);
        r["duhBuhSectionBackground"] = new SolidColorBrush(light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(32, 35, 41));
        r["duhBuhSectionBorder"] = new SolidColorBrush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74));
        r["duhBuhAccent"] = new SolidColorBrush(light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52));
        r["duhBuhSectionText"] = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
        r["duhBuhDescriptionText"] = new SolidColorBrush(light ? Color.FromRgb(100, 106, 118) : Color.FromRgb(160, 167, 178));
        window.Dispatcher.BeginInvoke(new Action(delegate { ApplySectionCards(window, light); }));
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
        if (category.Tag is string && (string)category.Tag == "__duhbuh_cards_applied") return;

        List<UIElement> original = new List<UIElement>();
        for (int i = 0; i < category.Children.Count; i++) original.Add(category.Children[i]);

        bool hasHeading = false;
        for (int i = 0; i < original.Count; i++)
        {
            if (IsSectionHeading(original[i] as TextBlock)) { hasHeading = true; break; }
        }
        if (!hasHeading) return;

        category.Tag = "__duhbuh_cards_applied";
        category.Children.Clear();

        StackPanel currentContent = null;
        bool sawFirstHeading = false;

        for (int i = 0; i < original.Count; i++)
        {
            UIElement child = original[i];
            TextBlock heading = child as TextBlock;

            if (IsSectionHeading(heading))
            {
                sawFirstHeading = true;
                currentContent = new StackPanel();
                Border currentCard = CreateCard(currentContent, light);

                Border accent = new Border
                {
                    Height = 3,
                    Background = new SolidColorBrush(light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52)),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                currentContent.Children.Add(accent);

                heading.Foreground = new SolidColorBrush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
                heading.Background = new SolidColorBrush(light ? Color.FromRgb(238, 241, 246) : Color.FromRgb(43, 47, 54));
                heading.Padding = new Thickness(10, 7, 10, 7);
                heading.Margin = new Thickness(0, 0, 0, 10);
                heading.HorizontalAlignment = HorizontalAlignment.Stretch;
                currentContent.Children.Add(heading);
                category.Children.Add(currentCard);
                continue;
            }

            if (sawFirstHeading && currentContent != null)
                currentContent.Children.Add(child);
            else
                category.Children.Add(child);
        }
    }

    private static Border CreateCard(StackPanel content, bool light)
    {
        return new Border
        {
            Background = new SolidColorBrush(light ? Color.FromRgb(252, 253, 255) : Color.FromRgb(39, 42, 48)),
            BorderBrush = new SolidColorBrush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 8, 14, 10),
            Margin = new Thickness(0, 0, 0, 14),
            Child = content
        };
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
        style.Setters.Add(new Setter(Control.WidthProperty, 96.0));
        style.Setters.Add(new Setter(Control.HeightProperty, 36.0));
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

    private static Style CreateComboBoxItemStyle(bool light)
    {
        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(25, 28, 34) : Color.FromRgb(242, 244, 247))));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
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

    private static Style CreateTabItemStyle(bool light)
    {
        Style style = new Style(typeof(TabItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(light ? Color.FromRgb(232, 235, 240) : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(light ? Color.FromRgb(45, 49, 57) : Color.FromRgb(205, 211, 220))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(light ? Color.FromRgb(200, 205, 214) : Color.FromRgb(65, 70, 80))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1, 1, 1, 0)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 7, 12, 7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(2, 0, 2, 0)));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
        return style;
    }
}
