using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

// Compatibility bridge: DuhBuhUI historically builds the Date & Time field as
// two controls. Replace that pair at window-load time with the reusable custom
// DateTimePicker, while retaining hidden DatePicker/TimePicker storage so the
// existing persistence path remains unchanged.
public static class DuhBuhUIDateTimeCompatibility
{
    private static readonly HashSet<Window> Processed = new HashSet<Window>();
    private const string StorageTag = "__duhbuh_datetime_storage";

    [ModuleInitializer]
    public static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded), true);
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null || Processed.Contains(window)) return;
        Processed.Add(window);
        ReplaceDateTimePairs(window);
    }

    private static void ReplaceDateTimePairs(DependencyObject root)
    {
        StackPanel panel = root as StackPanel;
        if (panel != null && !Equals(panel.Tag, StorageTag)) ReplaceInPanel(panel);

        foreach (object childObject in LogicalTreeHelper.GetChildren(root))
        {
            DependencyObject child = childObject as DependencyObject;
            if (child != null) ReplaceDateTimePairs(child);
        }
    }

    private static void ReplaceInPanel(StackPanel panel)
    {
        for (int i = 0; i < panel.Children.Count; i++)
        {
            DatePicker date = panel.Children[i] as DatePicker;
            if (date == null || date.Tag == null) continue;

            string key = date.Tag.ToString();
            TimePicker time = FindTimePicker(panel, key + "::time");
            if (time == null) continue;

            DateTimePicker combined = new DateTimePicker
            {
                Tag = key,
                MinWidth = Math.Max(220, date.MinWidth),
                Margin = date.Margin
            };

            DateTime initialDate = date.SelectedDate.HasValue
                ? date.SelectedDate.Value.Date
                : DateTime.Today;
            int hour = time.SelectedTime.HasValue ? time.SelectedTime.Value.Hours : DateTime.Now.Hour;
            int minute = time.SelectedTime.HasValue ? time.SelectedTime.Value.Minutes : DateTime.Now.Minute;
            combined.SelectedDateTime = new DateTime(
                initialDate.Year, initialDate.Month, initialDate.Day, hour, minute, 0);

            Window owner = Window.GetWindow(panel);
            combined.ApplyTheme(IsLightWindow(owner));

            combined.SelectedDateTimeChanged += delegate
            {
                if (!combined.SelectedDateTime.HasValue) return;
                DateTime value = combined.SelectedDateTime.Value;
                date.SelectedDate = value.Date;
                time.SelectedTime = new TimeSpan(value.Hour, value.Minute, 0);
            };

            // Keep the original controls alive for SaveTagged(), but remove
            // them from the visible layout.
            panel.Children.RemoveAt(panel.Children.IndexOf(time));
            int dateIndex = panel.Children.IndexOf(date);
            panel.Children.RemoveAt(dateIndex);

            StackPanel storage = new StackPanel
            {
                Visibility = Visibility.Collapsed,
                Tag = StorageTag
            };
            storage.Children.Add(date);
            storage.Children.Add(time);
            panel.Children.Insert(dateIndex, combined);
            panel.Children.Add(storage);

            i = dateIndex;
        }
    }

    private static TimePicker FindTimePicker(StackPanel panel, string tag)
    {
        for (int i = 0; i < panel.Children.Count; i++)
        {
            TimePicker time = panel.Children[i] as TimePicker;
            if (time != null && Equals(time.Tag, tag)) return time;
        }
        return null;
    }

    private static bool IsLightWindow(Window window)
    {
        if (window == null) return false;
        SolidColorBrush brush = window.Background as SolidColorBrush;
        if (brush == null) return false;
        Color c = brush.Color;
        double luminance = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
        return luminance >= 0.62;
    }
}
