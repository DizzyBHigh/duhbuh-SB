using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

public static class DuhBuhUITheme
{
    private static bool _initialized;
    private static readonly List<TabControl> _styledTabControls = new List<TabControl>();
    private static readonly List<ComboBox> _sizedComboBoxes = new List<ComboBox>();

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
        EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnComboBoxPreviewMouseLeftButtonDown), true);
    }

    private static void OnComboBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo == null || combo.IsDropDownOpen) return;
        combo.Focus();
        combo.IsDropDownOpen = true;
        e.Handled = true;
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
        r["duhBuhSectionBackground"] = Brush(light ? Color.FromRgb(247,248,250) : Color.FromRgb(32,35,41));
        r["duhBuhSectionBorder"] = Brush(light ? Color.FromRgb(218,222,230) : Color.FromRgb(60,65,74));
        r["duhBuhAccent"] = Brush(light ? Color.FromRgb(176,120,22) : Color.FromRgb(224,166,52));
        r["duhBuhSectionText"] = Brush(light ? Color.FromRgb(35,39,46) : Color.FromRgb(235,238,243));
        r["duhBuhDescriptionText"] = Brush(light ? Color.FromRgb(100,106,118) : Color.FromRgb(160,167,178));
        window.Dispatcher.BeginInvoke(new Action(delegate { ApplySectionCards(window, light); ApplyTabVisuals(window, light); ApplyComboBoxSizing(window); }));
    }

    private static SolidColorBrush Brush(Color color) { SolidColorBrush b = new SolidColorBrush(color); b.Freeze(); return b; }

    private static Style CreateButtonStyle(bool light)
    {
        Color normal = light ? Color.FromRgb(44,90,160) : Color.FromRgb(58,110,180);
        Color hover = light ? Color.FromRgb(58,108,184) : Color.FromRgb(72,128,198);
        Color pressed = light ? Color.FromRgb(34,72,132) : Color.FromRgb(44,88,150);
        Color disabled = light ? Color.FromRgb(190,195,204) : Color.FromRgb(70,74,82);
        Color normalBorder = light ? Color.FromRgb(32,68,125) : Color.FromRgb(90,140,205);
        Color accentBorder = Color.FromRgb(224,166,52);
        Style style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(normal)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.White)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(normalBorder)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16,7,16,7)));
        style.Setters.Add(new Setter(Control.MarginProperty, new Thickness(4,3,4,3)));
        style.Setters.Add(new Setter(Control.WidthProperty, 104.0));
        style.Setters.Add(new Setter(Control.HeightProperty, 38.0));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
        style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateButtonTemplate()));
        Trigger over = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true }; over.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hover))); over.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accentBorder))); style.Triggers.Add(over);
        Trigger pressedTrigger = new Trigger { Property = ButtonBase.IsPressedProperty, Value = true }; pressedTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brush(pressed))); pressedTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(accentBorder))); style.Triggers.Add(pressedTrigger);
        Trigger disabledTrigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false }; disabledTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brush(disabled))); disabledTrigger.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Color.FromRgb(150,154,162)))); disabledTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(disabled))); style.Triggers.Add(disabledTrigger);
        return style;
    }

    private static ControlTemplate CreateButtonTemplate()
    {
        ControlTemplate t = new ControlTemplate(typeof(Button));
        FrameworkElementFactory b = new FrameworkElementFactory(typeof(Border));
        b.SetBinding(Border.BackgroundProperty, TemplatedBinding("Background")); b.SetBinding(Border.BorderBrushProperty, TemplatedBinding("BorderBrush")); b.SetBinding(Border.BorderThicknessProperty, TemplatedBinding("BorderThickness")); b.SetValue(Border.CornerRadiusProperty, new CornerRadius(7)); b.SetValue(Border.SnapsToDevicePixelsProperty, true);
        FrameworkElementFactory p = new FrameworkElementFactory(typeof(ContentPresenter));
        p.SetBinding(ContentPresenter.ContentProperty, TemplatedBinding("Content")); p.SetBinding(ContentPresenter.ContentTemplateProperty, TemplatedBinding("ContentTemplate")); p.SetBinding(ContentPresenter.ContentStringFormatProperty, TemplatedBinding("ContentStringFormat")); p.SetBinding(ContentPresenter.HorizontalAlignmentProperty, TemplatedBinding("HorizontalContentAlignment")); p.SetBinding(ContentPresenter.VerticalAlignmentProperty, TemplatedBinding("VerticalContentAlignment")); p.SetBinding(ContentPresenter.MarginProperty, TemplatedBinding("Padding")); p.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true); b.AppendChild(p); t.VisualTree = b; return t;
    }

    private static Style CreateTextBoxStyle(bool light)
    {
        Style s = new Style(typeof(TextBox)); s.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Colors.White : Color.FromRgb(45,48,55)))); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(30,32,38) : Color.FromRgb(240,242,245)))); s.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(205,210,220) : Color.FromRgb(75,80,90)))); s.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1))); s.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8,5,8,5))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3,3,3,3))); return s;
    }

    private static Style CreateComboBoxStyle(bool light)
    {
        Color bg = light ? Colors.White : Color.FromRgb(45,48,55), fg = light ? Color.FromRgb(30,32,38) : Color.FromRgb(240,242,245), br = light ? Color.FromRgb(205,210,220) : Color.FromRgb(75,80,90);
        Style s = new Style(typeof(ComboBox)); s.Setters.Add(new Setter(Control.BackgroundProperty, Brush(bg))); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(fg))); s.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(br))); s.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1))); s.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(7,4,7,4))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3,3,3,3))); s.Setters.Add(new Setter(Control.HeightProperty, 30.0)); s.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left)); s.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center)); s.Setters.Add(new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Left)); s.Setters.Add(new Setter(Control.TemplateProperty, CreateComboBoxTemplate(bg,fg,br))); return s;
    }

    private static ControlTemplate CreateComboBoxTemplate(Color bg, Color fg, Color br)
    {
        ControlTemplate t = new ControlTemplate(typeof(ComboBox));
        FrameworkElementFactory root = new FrameworkElementFactory(typeof(Border)); root.SetBinding(Border.BackgroundProperty, TemplatedBinding("Background")); root.SetBinding(Border.BorderBrushProperty, TemplatedBinding("BorderBrush")); root.SetBinding(Border.BorderThicknessProperty, TemplatedBinding("BorderThickness")); root.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
        FrameworkElementFactory toggle = new FrameworkElementFactory(typeof(ToggleButton)); toggle.SetValue(ToggleButton.BackgroundProperty, Brushes.Transparent); toggle.SetValue(ToggleButton.BorderThicknessProperty, new Thickness(0)); toggle.SetValue(ToggleButton.PaddingProperty, new Thickness(0)); toggle.SetValue(ToggleButton.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch); toggle.SetValue(ToggleButton.VerticalContentAlignmentProperty, VerticalAlignment.Stretch); toggle.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsDropDownOpen") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Mode = BindingMode.TwoWay }); toggle.SetValue(ToggleButton.TemplateProperty, CreateDropdownToggleTemplate(fg)); grid.AppendChild(toggle);
        FrameworkElementFactory popup = new FrameworkElementFactory(typeof(Popup)); popup.SetValue(Popup.PlacementProperty, PlacementMode.Bottom); popup.SetValue(Popup.AllowsTransparencyProperty, true); popup.SetValue(Popup.FocusableProperty, false); popup.SetValue(Popup.StaysOpenProperty, false); popup.SetBinding(Popup.IsOpenProperty, new Binding("IsDropDownOpen") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent), Mode = BindingMode.TwoWay }); popup.SetBinding(Popup.PlacementTargetProperty, new Binding(".") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        FrameworkElementFactory popupBorder = new FrameworkElementFactory(typeof(Border)); popupBorder.SetValue(Border.BackgroundProperty, Brush(bg)); popupBorder.SetValue(Border.BorderBrushProperty, Brush(br)); popupBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1)); popupBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3)); popupBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left); popupBorder.SetValue(Border.SnapsToDevicePixelsProperty, true);
        FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer)); scroll.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto); scroll.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled); scroll.SetValue(ScrollViewer.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
        FrameworkElementFactory items = new FrameworkElementFactory(typeof(ItemsPresenter)); items.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left); scroll.AppendChild(items); popupBorder.AppendChild(scroll); popup.AppendChild(popupBorder); grid.AppendChild(popup); root.AppendChild(grid); t.VisualTree = root; return t;
    }

    private static ControlTemplate CreateDropdownToggleTemplate(Color fg)
    {
        ControlTemplate t = new ControlTemplate(typeof(ToggleButton)); FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));
        FrameworkElementFactory display = new FrameworkElementFactory(typeof(ContentPresenter));
        display.SetBinding(ContentPresenter.ContentProperty, new Binding("SelectionBoxItem") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); display.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding("SelectionBoxItemTemplate") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); display.SetBinding(ContentPresenter.ContentStringFormatProperty, new Binding("SelectionBoxItemStringFormat") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); display.SetBinding(ContentPresenter.MarginProperty, new Binding("Padding") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); display.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new Binding("HorizontalContentAlignment") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); display.SetBinding(ContentPresenter.VerticalAlignmentProperty, new Binding("VerticalContentAlignment") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ComboBox), 1) }); grid.AppendChild(display);
        FrameworkElementFactory arrow = new FrameworkElementFactory(typeof(TextBlock)); arrow.SetValue(TextBlock.TextProperty, "▼"); arrow.SetValue(TextBlock.FontSizeProperty, 11.0); arrow.SetValue(TextBlock.ForegroundProperty, Brush(fg)); arrow.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right); arrow.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center); arrow.SetValue(TextBlock.MarginProperty, new Thickness(0,0,8,0)); grid.AppendChild(arrow); t.VisualTree = grid; return t;
    }

    private static Style CreateComboBoxItemStyle(bool light)
    {
        Color bg = light ? Colors.White : Color.FromRgb(45,48,55), fg = light ? Color.FromRgb(25,28,34) : Color.FromRgb(242,244,247), hover = light ? Color.FromRgb(238,242,248) : Color.FromRgb(58,63,72), selected = light ? Color.FromRgb(224,174,74) : Color.FromRgb(224,166,52);
        Style s = new Style(typeof(ComboBoxItem)); s.Setters.Add(new Setter(Control.BackgroundProperty, Brush(bg))); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(fg))); s.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8,6,8,6))); s.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left)); s.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center)); s.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0))); s.Setters.Add(new Setter(Control.HeightProperty, 30.0)); s.Setters.Add(new Setter(Control.MinWidthProperty, 0.0));
        Trigger h = new Trigger { Property = ComboBoxItem.IsHighlightedProperty, Value = true }; h.Setters.Add(new Setter(Control.BackgroundProperty, Brush(hover))); h.Setters.Add(new Setter(Control.ForegroundProperty, Brush(fg))); s.Triggers.Add(h);
        Trigger sel = new Trigger { Property = ComboBoxItem.IsSelectedProperty, Value = true }; sel.Setters.Add(new Setter(Control.BackgroundProperty, Brush(selected))); sel.Setters.Add(new Setter(Control.ForegroundProperty, Brush(Colors.Black))); s.Triggers.Add(sel); return s;
    }

    private static void ApplyComboBoxSizing(Window window)
    {
        List<ComboBox> combos = new List<ComboBox>(); FindComboBoxes(window, combos);
        for (int i = 0; i < combos.Count; i++)
        {
            ComboBox combo = combos[i];
            if (!_sizedComboBoxes.Contains(combo))
            {
                _sizedComboBoxes.Add(combo);
                combo.Loaded += delegate { SizeComboBox(combo); };
                combo.ItemContainerGenerator.StatusChanged += delegate { SizeComboBox(combo); };
            }
            SizeComboBox(combo);
        }
    }

    private static void FindComboBoxes(DependencyObject parent, List<ComboBox> results)
    {
        if (parent == null) return; ComboBox combo = parent as ComboBox; if (combo != null) { results.Add(combo); return; }
        int count = VisualTreeHelper.GetChildrenCount(parent); for (int i = 0; i < count; i++) FindComboBoxes(VisualTreeHelper.GetChild(parent,i), results);
    }

    private static void SizeComboBox(ComboBox combo)
    {
        if (combo == null || combo.Items.Count == 0) return; double widest = 0;
        for (int i = 0; i < combo.Items.Count; i++)
        {
            string text = combo.Items[i] == null ? string.Empty : combo.Items[i].ToString();
            TextBlock measure = new TextBlock { Text = text, FontFamily = combo.FontFamily, FontSize = combo.FontSize, FontStyle = combo.FontStyle, FontWeight = combo.FontWeight };
            measure.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity)); if (measure.DesiredSize.Width > widest) widest = measure.DesiredSize.Width;
        }
        Thickness p = combo.Padding; double width = widest + p.Left + p.Right + 28 + 2; if (width > 0 && Math.Abs(combo.Width - width) > 0.5) combo.Width = width;
    }

    private static Style CreateDatePickerStyle(bool light)
    {
        Style s = new Style(typeof(DatePicker)); s.Setters.Add(new Setter(Control.BackgroundProperty, Brush(light ? Colors.White : Color.FromRgb(45,48,55)))); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(30,32,38) : Color.FromRgb(240,242,245)))); s.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(light ? Color.FromRgb(205,210,220) : Color.FromRgb(75,80,90)))); s.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(3,3,3,3))); return s;
    }
    private static Style CreateCheckBoxStyle(bool light) { Style s = new Style(typeof(CheckBox)); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(35,39,46) : Color.FromRgb(235,238,243)))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0,4,0,4))); return s; }
    private static Style CreateRadioButtonStyle(bool light) { Style s = new Style(typeof(RadioButton)); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(light ? Color.FromRgb(35,39,46) : Color.FromRgb(235,238,243)))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(0,3,0,3))); return s; }

    private static Style CreateTabItemStyle(bool light)
    {
        Color bg = light ? Color.FromRgb(232,235,240) : Color.FromRgb(31,34,40), fg = light ? Color.FromRgb(45,49,57) : Color.FromRgb(205,211,220), br = light ? Color.FromRgb(200,205,214) : Color.FromRgb(65,70,80);
        Style s = new Style(typeof(TabItem)); s.Setters.Add(new Setter(Control.BackgroundProperty, Brush(bg))); s.Setters.Add(new Setter(Control.ForegroundProperty, Brush(fg))); s.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(br))); s.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1,1,1,0))); s.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12,7,12,7))); s.Setters.Add(new Setter(Control.MarginProperty, new Thickness(2,0,2,0))); s.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold)); s.Setters.Add(new Setter(Control.MinHeightProperty, 32.0)); s.Setters.Add(new Setter(Control.TemplateProperty, CreateTabItemTemplate())); return s;
    }
    private static ControlTemplate CreateTabItemTemplate()
    {
        ControlTemplate t = new ControlTemplate(typeof(TabItem)); FrameworkElementFactory b = new FrameworkElementFactory(typeof(Border)); b.SetBinding(Border.BackgroundProperty,TemplatedBinding("Background")); b.SetBinding(Border.BorderBrushProperty,TemplatedBinding("BorderBrush")); b.SetBinding(Border.BorderThicknessProperty,TemplatedBinding("BorderThickness")); b.SetBinding(Border.PaddingProperty,TemplatedBinding("Padding")); b.SetValue(Border.SnapsToDevicePixelsProperty,true);
        FrameworkElementFactory p = new FrameworkElementFactory(typeof(ContentPresenter)); p.SetValue(ContentPresenter.ContentSourceProperty,"Header"); p.SetBinding(ContentPresenter.HorizontalAlignmentProperty,TemplatedBinding("HorizontalContentAlignment")); p.SetBinding(ContentPresenter.VerticalAlignmentProperty,TemplatedBinding("VerticalContentAlignment")); p.SetValue(ContentPresenter.RecognizesAccessKeyProperty,true); b.AppendChild(p); t.VisualTree=b; return t;
    }
    private static Binding TemplatedBinding(string path) { return new Binding(path) { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }; }

    private static void ApplyTabVisuals(Window window, bool light)
    {
        TabControl tabs = FindTabControl(window); if (tabs == null) return;
        if (!_styledTabControls.Contains(tabs)) { _styledTabControls.Add(tabs); tabs.SelectionChanged += delegate { UpdateTabVisuals(tabs,light); }; }
        for (int i=0;i<tabs.Items.Count;i++) { TabItem tab=tabs.Items[i] as TabItem; if(tab==null)continue; TextBlock h=tab.Header as TextBlock; if(h==null&&tab.Header!=null){h=new TextBlock{Text=tab.Header.ToString(),FontSize=13,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center,HorizontalAlignment=HorizontalAlignment.Center};tab.Header=h;} }
        UpdateTabVisuals(tabs,light);
    }
    private static void UpdateTabVisuals(TabControl tabs,bool light)
    {
        if(tabs==null)return; Color normalBg=light?Color.FromRgb(232,235,240):Color.FromRgb(31,34,40), normalFg=light?Color.FromRgb(45,49,57):Color.FromRgb(225,229,235), selectedBg=light?Colors.White:Color.FromRgb(43,47,54), selectedFg=light?Color.FromRgb(25,28,34):Colors.White, br=light?Color.FromRgb(200,205,214):Color.FromRgb(65,70,80), accent=light?Color.FromRgb(176,120,22):Color.FromRgb(224,166,52);
        for(int i=0;i<tabs.Items.Count;i++){TabItem tab=tabs.Items[i] as TabItem;if(tab==null)continue;bool selected=tab.IsSelected;tab.Template=CreateTabItemTemplate();tab.Background=Brush(selected?selectedBg:normalBg);tab.Foreground=Brush(selected?selectedFg:normalFg);tab.BorderBrush=Brush(selected?accent:br);tab.BorderThickness=selected?new Thickness(1,2,1,0):new Thickness(1,1,1,0);tab.Padding=new Thickness(12,7,12,7);tab.FontWeight=FontWeights.SemiBold;TextBlock h=tab.Header as TextBlock;if(h!=null){h.Foreground=Brush(selected?selectedFg:normalFg);h.FontWeight=FontWeights.SemiBold;h.TextAlignment=TextAlignment.Center;}}
    }

    private static void ApplySectionCards(Window window,bool light)
    {
        TabControl tabs=FindTabControl(window);if(tabs==null)return;for(int i=0;i<tabs.Items.Count;i++){TabItem tab=tabs.Items[i] as TabItem;if(tab==null)continue;ScrollViewer scroll=tab.Content as ScrollViewer;if(scroll==null)continue;StackPanel category=scroll.Content as StackPanel;if(category==null)continue;ApplyCardsToCategory(category,light);}
    }
    private static TabControl FindTabControl(DependencyObject parent)
    {
        if(parent==null)return null;TabControl direct=parent as TabControl;if(direct!=null)return direct;int count=VisualTreeHelper.GetChildrenCount(parent);for(int i=0;i<count;i++){TabControl found=FindTabControl(VisualTreeHelper.GetChild(parent,i));if(found!=null)return found;}return null;
    }
    private static void ApplyCardsToCategory(StackPanel category,bool light)
    {
        if(category.Tag is string&&(string)category.Tag=="__duhbuh_cards_applied")return;List<UIElement> original=new List<UIElement>();for(int i=0;i<category.Children.Count;i++)original.Add(category.Children[i]);bool hasHeading=false;for(int i=0;i<original.Count;i++)if(IsSectionHeading(original[i] as TextBlock)){hasHeading=true;break;}if(!hasHeading)return;category.Tag="__duhbuh_cards_applied";category.Children.Clear();StackPanel current=null;bool saw=false;
        for(int i=0;i<original.Count;i++){UIElement child=original[i];TextBlock heading=child as TextBlock;if(IsSectionHeading(heading)){saw=true;current=new StackPanel();Border card=CreateCard(current,light);current.Children.Add(new Border{Height=3,Background=Brush(light?Color.FromRgb(176,120,22):Color.FromRgb(224,166,52)),HorizontalAlignment=HorizontalAlignment.Stretch,Margin=new Thickness(0,0,0,8)});heading.Foreground=Brush(light?Color.FromRgb(35,39,46):Color.FromRgb(235,238,243));heading.Background=Brush(light?Color.FromRgb(238,241,246):Color.FromRgb(43,47,54));heading.Padding=new Thickness(10,7,10,7);heading.Margin=new Thickness(0,0,0,10);heading.HorizontalAlignment=HorizontalAlignment.Stretch;current.Children.Add(heading);category.Children.Add(card);continue;}if(saw&&current!=null)current.Children.Add(child);else category.Children.Add(child);}
    }
    private static Border CreateCard(StackPanel content,bool light){return new Border{Background=Brush(light?Color.FromRgb(252,253,255):Color.FromRgb(39,42,48)),BorderBrush=Brush(light?Color.FromRgb(218,222,230):Color.FromRgb(60,65,74)),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(10),Padding=new Thickness(14,8,14,10),Margin=new Thickness(0,0,0,14),Child=content};}
    private static bool IsSectionHeading(TextBlock text){return text!=null&&text.FontSize>=17&&text.FontWeight==FontWeights.SemiBold;}
    private static bool IsLightWindow(Window window){if(window==null)return false;SolidColorBrush b=window.Background as SolidColorBrush;if(b==null)return false;Color c=b.Color;return c.R>180&&c.G>180&&c.B>180;}
}
