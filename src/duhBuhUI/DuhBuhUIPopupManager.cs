using System;

// Ensures duhBuhUI's anchored popups behave like a single popup surface:
// opening one date/time picker closes any other date/time picker popup first.
internal static class DuhBuhUIPopupManager
{
    private static object _owner;
    private static Action _close;

    public static void Register(object owner, Action close)
    {
        if (_owner != null && !ReferenceEquals(_owner, owner))
        {
            Action previousClose = _close;
            _owner = null;
            _close = null;
            if (previousClose != null) previousClose();
        }

        _owner = owner;
        _close = close;
    }

    public static void Unregister(object owner)
    {
        if (!ReferenceEquals(_owner, owner)) return;
        _owner = null;
        _close = null;
    }
}
