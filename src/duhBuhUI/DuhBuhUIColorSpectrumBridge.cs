using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

public static class DuhBuhUIColorSpectrumBridge
{
    private static readonly MethodInfo AttachMethod = typeof(DuhBuhUIColorSpectrumEnhancement).GetMethod("Attach", BindingFlags.Static | BindingFlags.NonPublic);

    [ModuleInitializer]
    public static void Initialize()
    {
        EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseDownEvent,
            new MouseButtonEventHandler(OnWindowPreviewMouseDown), true);
    }

    private static void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        DuhBuhUICustomColorPicker picker = FindPicker(e.OriginalSource as DependencyObject);
        if (picker == null) return;

        Dispatcher dispatcher = picker.Dispatcher;
        dispatcher.BeginInvoke(new Action(delegate { TryAttach(picker); }), DispatcherPriority.Input);
        dispatcher.BeginInvoke(new Action(delegate { TryAttach(picker); }), DispatcherPriority.Background);
    }

    private static DuhBuhUICustomColorPicker FindPicker(DependencyObject source)
    {
        DependencyObject current = source;
        while (current != null)
        {
            DuhBuhUICustomColorPicker picker = current as DuhBuhUICustomColorPicker;
            if (picker != null) return picker;
            current = GetParent(current);
        }
        return null;
    }

    private static DependencyObject GetParent(DependencyObject value)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(value);
        if (parent != null) return parent;
        FrameworkElement element = value as FrameworkElement;
        return element == null ? null : element.Parent;
    }

    private static void TryAttach(DuhBuhUICustomColorPicker picker)
    {
        if (AttachMethod == null) return;
        try
        {
            object result = AttachMethod.Invoke(null, new object[] { picker });
            if (result is bool && (bool)result) return;
        }
        catch { }

        DispatcherTimer retry = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromMilliseconds(25)
        };
        int attempts = 0;
        retry.Tick += delegate
        {
            attempts++;
            bool attached = false;
            try
            {
                object result = AttachMethod.Invoke(null, new object[] { picker });
                attached = result is bool && (bool)result;
            }
            catch { }
            if (attached || attempts >= 20) retry.Stop();
        };
        retry.Start();
    }
}
