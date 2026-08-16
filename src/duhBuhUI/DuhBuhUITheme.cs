// duhBuhUITheme - safe visual styling for the shared settings UI.
// Uses plain WPF styles and state triggers compatible with Streamer.bot.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public static class DuhBuhUITheme
{
    private static bool _initialized;
    private static readonly List<ComboBox> _styledComboBoxes = new List<ComboBox>();

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
        r["duhBuhSectionBackground"] = Brush(light ? Color.FromRgb(247, 248, 250) : Color.FromRgb(32, 35, 41));
        r["duhBuhSectionBorder"] = Brush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74));
        r["duhBuhAccent"] = Brush(light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52));
        r["duhBuhSectionText"] = Brush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
        r["duhBuhDescriptionText"] = Brush(light ? Color.FromRgb(100, 106, 118) : Color.FromRgb(160, 167, 178));

        // Resource roles found in TawmaeUI's dropdown implementation.
        r["ItemBg"] = Brush(light ? Colors.White : Color.FromRgb(45, 48, 55));
        r["ItemHoverBg"] = Brush(light ? Color.FromRgb(239, 242, 247) : Color.FromRgb(57, 61, 70));
        r["ItemSelectedBg"] = Brush(Color.FromRgb(224, 166, 52));
        r["ItemSelectedFg"] = Brush(Colors.Black);
        r["ShellBg"] = Brush(light ? Colors.White : Color.FromRgb(45, 48, 55));
        r["darkBg"] = Brush(Color.FromRgb(45, 48, 55));

        window.Dispatcher.BeginInvoke(new Action(delegate
        {
            ApplySectionCards(window, light);
            ApplyComboBoxSelectionFixes(window, light);
        }));
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush b = new SolidColorBrush(color);
        b.Freeze();
        return b;
    }

    private static void ApplyComboBoxSelectionFixes(Window window, bool light)
    {
        ApplyComboBoxSelectionFixesToTree(window, light);
    }

    private static void ApplyComboBoxSelectionFixesToTree(DependencyObject node, bool light)
    {
        if (node == null) return;
        ComboBox combo = node as ComboBox;
        if (combo != null)
        {
            if (!_styledComboBoxes.Contains(combo))
            {
                _styledComboBoxes.Add(combo);
                combo.SelectionChanged += delegate { QueueComboBoxRestyle(combo, light); };
                combo.DropDownOpened += delegate { QueueComboBoxRestyle(combo, light); };
                combo.Loaded += delegate { QueueComboBoxRestyle(combo, light); };
            }
            QueueComboBoxRestyle(combo, light);
        }
        int count = VisualTreeHelper.GetChildrenCount(node);
        for (int i = 0; i < count; i++) ApplyComboBoxSelectionFixesToTree(VisualTreeHelper.GetChild(node, i), light);
    }

    private static void QueueComboBoxRestyle(ComboBox combo, bool light)
    {
        if (combo == null) return;
        RestyleComboBoxItems(combo, light);
        combo.Dispatcher.BeginInvoke(new Action(delegate { RestyleComboBoxItems(combo, light); }));
    }

    private static void RestyleComboBoxItems(ComboBox combo, bool light)
    {
        if (combo == null) return;
        Color normalBackground = light ? Colors.White : Color.FromRgb(45, 48, 55);
        Color normalForeground = light ? Color.FromRgb(25, 28, 34) : Color.FromRgb(242, 244, 247);
        Color selectedBackground = Color.FromRgb(224, 166, 52);
        Color selectedForeground = Colors.Black;
        Color hoverBackground = light ? Color.FromRgb(239, 242, 247) : Color.FromRgb(57, 61, 70);
        for (int i = 0; i < combo.Items.Count; i++)
        {
            ComboBoxItem item = combo.ItemContainerGenerator.ContainerFromIndex(i) as ComboBoxItem;
            if (item == null) continue;
            bool selected = item.IsSelected;
            bool highlighted = item.IsHighlighted;
            item.Background = new SolidColorBrush(selected ? selectedBackground : (highlighted ? hoverBackground : normalBackground));
            item.Foreground = new SolidColorBrush(selected ? selectedForeground : normalForeground);
        }
    }

    private static Style CreateButtonStyle(bool light)
    {
        Style style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Color.FromRgb(44, 90, 160) : Color.FromRgb(58, 110, 180))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(32, 68, 125) : Color.FromRgb(90, 140, 205))));
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
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 5, 8, 5)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Style style = new Style(typeof(ComboBox));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7, 4, 7, 4)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        style.Setters.Add(new Setter(ItemsControl.ItemContainerStyleProperty, CreateComboBoxItemStyle(light)));
        return style;
    }

    private static Style CreateComboBoxItemStyle(bool light)
    {
        Color normalBackground = light ? Colors.White : Color.FromRgb(45, 48, 55);
        Color normalForeground = light ? Color.FromRgb(25, 28, 34) : Color.FromRgb(242, 244, 247);
        Color selectedBackground = Color.FromRgb(224, 166, 52);
        Color selectedForeground = Colors.Black;
        Color hoverBackground = light ? Color.FromRgb(239, 242, 247) : Color.FromRgb(57, 61, 70);
        Style style = new Style(typeof(ComboBoxItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normalBackground)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(normalForeground)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
        Trigger highlighted = new Trigger { Property = ComboBoxItem.IsHighlightedProperty, Value = true };
        highlighted.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hoverBackground)));
        highlighted.Setters.Add(new Setter(Control.ForegroundProperty, Brush(normalForeground)));
        style.Triggers.Add(highlighted);
        Trigger selected = new Trigger { Property = ComboBoxItem.IsSelectedProperty, Value = true };
        selected.Setters.Add(new Setter(Control.BackgroundProperty, Brush(selectedBackground)));
        selected.Setters.Add(new Setter(Control.ForegroundProperty, Brush(selectedForeground)));
        style.Triggers.Add(selected);
        return style;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style style = new Style(typeof(DatePicker));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Colors.White : Color.FromRgb(45, 48, 55))));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(30, 32, 38) : Color.FromRgb(240, 242, 245))));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3, 3, 3, 3)));
        return style;
    }

    private static Style CreateCheckBoxStyle(bool light)
    {
        Style style = new Style(typeof(CheckBox));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 4, 0, 4)));
        return style;
    }

    private static Style CreateRadioButtonStyle(bool light)
    {
        Style style = new Style(typeof(RadioButton));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243))));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0, 3, 0, 3)));
        return style;
    }

    private static Style CreateTabItemStyle(bool light)
    {
        Color normalBackground = light ? Color.FromRgb(232, 235, 240) : Color.FromRgb(45, 48, 55);
        Color normalForeground = light ? Color.FromRgb(45, 49, 57) : Color.FromRgb(205, 211, 220);
        Color selectedBackground = light ? Colors.White : Color.FromRgb(39, 42, 48);
        Color selectedForeground = light ? Color.FromRgb(25, 28, 34) : Color.FromRgb(245, 247, 250);
        Color accent = light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52);
        Style style = new Style(typeof(TabItem));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normalBackground)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(normalForeground)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(200, 205, 214) : Color.FromRgb(65, 70, 80))));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1, 1, 1, 0)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 7, 12, 7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(2, 0, 2, 0)));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.MinHeightProperty, 32.0));
        Trigger selected = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
        selected.Setters.Add(new Setter(Control.BackgroundProperty, Brush(selectedBackground)));
        selected.Setters.Add(new Setter(Control.ForegroundProperty, Brush(selectedForeground)));
        selected.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accent)));
        selected.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1, 2, 1, 0)));
        style.Triggers.Add(selected);
        return style;
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
        for (int i = 0; i < original.Count; i++) if (IsSectionHeading(original[i] as TextBlock)) { hasHeading = true; break; }
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
                currentContent.Children.Add(new Border { Height = 3, Background = Brush(light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52)), HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 8) });
                heading.Foreground = Brush(light ? Color.FromRgb(35, 39, 46) : Color.FromRgb(235, 238, 243));
                heading.Background = Brush(light ? Color.FromRgb(238, 241, 246) : Color.FromRgb(43, 47, 54));
                heading.Padding = new Thickness(10, 7, 10, 7);
                heading.Margin = new Thickness(0, 0, 0, 10);
                heading.HorizontalAlignment = HorizontalAlignment.Stretch;
                currentContent.Children.Add(heading);
                category.Children.Add(currentCard);
                continue;
            }
            if (sawFirstHeading && currentContent != null) currentContent.Children.Add(child); else category.Children.Add(child);
        }
    }

    private static Border CreateCard(StackPanel content, bool light)
    {
        return new Border
        {
            Background = Brush(light ? Color.FromRgb(252, 253, 255) : Color.FromRgb(39, 42, 48)),
            BorderBrush = Brush(light ? Color.FromRgb(218, 222, 230) : Color.FromRgb(60, 65, 74)),
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
}
