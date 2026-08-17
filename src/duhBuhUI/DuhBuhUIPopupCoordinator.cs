using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// Keeps the reusable date/time picker popups mutually exclusive without
// affecting the dropdown popups hosted inside the time picker.
internal static class DuhBuhUIPopupCoordinator
{
    private static Popup _currentPopup;
    private static FrameworkElement _currentOwner;
    private static Window _currentWindow;

    public static void Open(Popup popup, FrameworkElement owner)
    {
        CloseCurrent();

        _currentPopup = popup;
        _currentOwner = owner;
        _currentWindow = Window.GetWindow(owner);

        if (_currentWindow != null)
            _currentWindow.PreviewMouseDown += OwnerPreviewMouseDown;
    }

    public static void Closed(Popup popup)
    {
        if (!ReferenceEquals(_currentPopup, popup)) return;
        Detach();
    }

    private static void CloseCurrent()
    {
        Popup popup = _currentPopup;
        Detach();
        if (popup != null && popup.IsOpen)
            popup.IsOpen = false;
    }

    private static void Detach()
    {
        if (_currentWindow != null)
            _currentWindow.PreviewMouseDown -= OwnerPreviewMouseDown;

        _currentPopup = null;
        _currentOwner = null;
        _currentWindow = null;
    }

    private static void OwnerPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Popup popup = _currentPopup;
        if (popup == null || !popup.IsOpen) return;

        DependencyObject source = e.OriginalSource as DependencyObject;
        if (source != null)
        {
            if (_currentOwner != null && IsDescendant(source, _currentOwner)) return;
            if (popup.Child != null && IsDescendant(source, popup.Child)) return;
        }

        CloseCurrent();
    }

    private static bool IsDescendant(DependencyObject source, DependencyObject ancestor)
    {
        DependencyObject current = source;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor)) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}
