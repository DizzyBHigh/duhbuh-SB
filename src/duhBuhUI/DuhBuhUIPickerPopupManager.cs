using System;

// Keeps the top-level date/time picker popups mutually exclusive. The custom
// dropdowns inside those popups deliberately do not participate in this
// manager, so opening an hour/minute dropdown does not close its parent picker.
public static class DuhBuhUIPickerPopupManager
{
    private static object _activeOwner;
    private static Action _closeActive;

    public static void Activate(object owner, Action close)
    {
        if (_activeOwner != null && !ReferenceEquals(_activeOwner, owner))
        {
            Action closeActive = _closeActive;
            if (closeActive != null) closeActive();
        }

        _activeOwner = owner;
        _closeActive = close;
    }

    public static void Deactivate(object owner)
    {
        if (!ReferenceEquals(_activeOwner, owner)) return;
        _activeOwner = null;
        _closeActive = null;
    }
}
