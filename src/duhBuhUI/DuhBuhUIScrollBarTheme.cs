using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

public static class DuhBuhUIScrollBarTheme
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
        Apply(window, IsLightWindow(window));
    }

    public static void Apply(Window window, bool light)
    {
        if (window == null) return;

        Color track = light ? Color.FromRgb(232, 235, 240) : Color.FromRgb(31, 34, 40);
        Color thumb = light ? Color.FromRgb(176, 120, 22) : Color.FromRgb(224, 166, 52);
        Color thumbHover = light ? Color.FromRgb(194, 139, 35) : Color.FromRgb(239, 184, 66);
        Color thumbPressed = light ? Color.FromRgb(145, 96, 12) : Color.FromRgb(185, 126, 24);
        Color border = light ? Color.FromRgb(205, 210, 220) : Color.FromRgb(75, 80, 90);
        Color focus = light ? Color.FromRgb(140, 95, 15) : Color.FromRgb(255, 207, 90);

        string xaml = BuildStyleXaml(track, thumb, thumbHover, thumbPressed, border, focus);
        Style style = (Style)XamlReader.Parse(xaml);
        window.Resources[typeof(ScrollBar)] = style;
    }

    private static string BuildStyleXaml(Color track, Color thumb, Color thumbHover, Color thumbPressed, Color border, Color focus)
    {
        string t = ToHex(track);
        string th = ToHex(thumb);
        string thHover = ToHex(thumbHover);
        string thPressed = ToHex(thumbPressed);
        string b = ToHex(border);
        string f = ToHex(focus);

        return @"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       TargetType='{x:Type ScrollBar}'>
  <Setter Property='SnapsToDevicePixels' Value='True'/>
  <Setter Property='Focusable' Value='True'/>
  <Setter Property='MinWidth' Value='12'/>
  <Setter Property='MinHeight' Value='12'/>
  <Setter Property='Width' Value='12'/>
  <Setter Property='Template'>
    <Setter.Value>
      <ControlTemplate TargetType='{x:Type ScrollBar}'>
        <Grid SnapsToDevicePixels='True'>
          <Grid.Resources>
            <Style x:Key='PageButton' TargetType='{x:Type RepeatButton}'>
              <Setter Property='Focusable' Value='False'/>
              <Setter Property='Background' Value='Transparent'/>
              <Setter Property='BorderThickness' Value='0'/>
              <Setter Property='Template'>
                <Setter.Value>
                  <ControlTemplate TargetType='{x:Type RepeatButton}'>
                    <Border Background='Transparent'/>
                  </ControlTemplate>
                </Setter.Value>
              </Setter>
            </Style>
            <Style x:Key='ThumbStyle' TargetType='{x:Type Thumb}'>
              <Setter Property='Focusable' Value='False'/>
              <Setter Property='MinHeight' Value='24'/>
              <Setter Property='Background' Value='" + th + @"'/>
              <Setter Property='BorderBrush' Value='" + b + @"'/>
              <Setter Property='BorderThickness' Value='1'/>
              <Setter Property='Template'>
                <Setter.Value>
                  <ControlTemplate TargetType='{x:Type Thumb}'>
                    <Border CornerRadius='5'
                            Background='{TemplateBinding Background}'
                            BorderBrush='{TemplateBinding BorderBrush}'
                            BorderThickness='{TemplateBinding BorderThickness}'/>
                  </ControlTemplate>
                </Setter.Value>
              </Setter>
              <Style.Triggers>
                <Trigger Property='IsMouseOver' Value='True'>
                  <Setter Property='Background' Value='" + thHover + @"'/>
                  <Setter Property='BorderBrush' Value='" + f + @"'/>
                </Trigger>
                <Trigger Property='IsDragging' Value='True'>
                  <Setter Property='Background' Value='" + thPressed + @"'/>
                  <Setter Property='BorderBrush' Value='" + f + @"'/>
                </Trigger>
              </Style.Triggers>
            </Style>
          </Grid.Resources>
          <Border Background='" + t + @"' BorderBrush='" + b + @"' BorderThickness='1' CornerRadius='6'/>
          <Track x:Name='PART_Track' Margin='1' IsDirectionReversed='True'>
            <Track.DecreaseRepeatButton>
              <RepeatButton Style='{StaticResource PageButton}' Command='ScrollBar.PageUpCommand'/>
            </Track.DecreaseRepeatButton>
            <Track.Thumb>
              <Thumb Style='{StaticResource ThumbStyle}' Margin='1,0,1,0'/>
            </Track.Thumb>
            <Track.IncreaseRepeatButton>
              <RepeatButton Style='{StaticResource PageButton}' Command='ScrollBar.PageDownCommand'/>
            </Track.IncreaseRepeatButton>
          </Track>
        </Grid>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
  <Style.Triggers>
    <Trigger Property='Orientation' Value='Horizontal'>
      <Setter Property='Height' Value='12'/>
      <Setter Property='Width' Value='Auto'/>
    </Trigger>
  </Style.Triggers>
</Style>";
    }

    private static string ToHex(Color color)
    {
        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
    }

    private static bool IsLightWindow(Window window)
    {
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color color = brush.Color;
        return color.R > 180 && color.G > 180 && color.B > 180;
    }
}
