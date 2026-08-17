using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

// Keeps the existing DuhBuhUI SaveTagged path compatible with the reusable
// picker without making the picker depend on DuhBuhUI internals. The watcher
// only copies the committed/closed value into the hidden storage TextBox.
public static class DuhBuhUIColorPickerPersistence
{
    private sealed class WatchState
    {
        public DuhBuhUICustomColorPicker Picker;
        public TextBox Storage;
        public bool WasOpen;
        public DispatcherTimer Timer;
    }

    private static readonly HashSet<DuhBuhUICustomColorPicker> Attached = new HashSet<DuhBuhUICustomColorPicker>();
    private static readonly FieldInfo PopupField = typeof(DuhBuhUICustomColorPicker).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic);

    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded), true);
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Window window = sender as Window;
        if (window == null) return;
        window.Dispatcher.BeginInvoke(new Action(delegate { AttachWatchers(window); }), DispatcherPriority.ContextIdle);
    }

    private static void AttachWatchers(DependencyObject root)
    {
        foreach (object childObject in LogicalTreeHelper.GetChildren(root))
        {
            DependencyObject child = childObject as DependencyObject;
            if (child == null) continue;

            DuhBuhUICustomColorPicker picker = child as DuhBuhUICustomColorPicker;
            if (picker != null) Attach(picker, FindStorage(root, picker));
            AttachWatchers(child);
        }
    }

    private static TextBox FindStorage(DependencyObject root, DuhBuhUICustomColorPicker picker)
    {
        StackPanel row = picker.Parent as StackPanel;
        if (row == null) return null;
        for (int i = 0; i < row.Children.Count; i++)
        {
            TextBox storage = row.Children[i] as TextBox;
            if (storage != null && storage.Tag != null && storage.Visibility == Visibility.Collapsed)
                return storage;
        }
        return null;
    }

    private static void Attach(DuhBuhUICustomColorPicker picker, TextBox storage)
    {
        if (picker == null || storage == null || Attached.Contains(picker)) return;
        Attached.Add(picker);

        WatchState state = new WatchState
        {
            Picker = picker,
            Storage = storage,
            WasOpen = false,
            Timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(75) }
        };

        state.Timer.Tick += delegate
        {
            bool isOpen = false;
            if (PopupField != null)
            {
                System.Windows.Controls.Primitives.Popup popup = PopupField.GetValue(picker) as System.Windows.Controls.Primitives.Popup;
                isOpen = popup != null && popup.IsOpen;
            }

            if (state.WasOpen && !isOpen)
                state.Storage.Text = picker.SelectedColor;

            state.WasOpen = isOpen;
        };
        state.Timer.Start();
    }
}
