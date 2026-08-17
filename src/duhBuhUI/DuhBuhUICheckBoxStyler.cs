using System;
using System.Collections.Generic;
using System.Windows.Controls;

// Compatibility/registration shim for the custom CheckBox control.
// Registered keys render as square checkboxes; ordinary toggle keys render as switches.
public static class DuhBuhUICheckBoxStyler
{
    private static readonly List<string> _checkboxKeys = new List<string>();

    public static void Initialize() { }

    public static void RegisterCheckboxKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!_checkboxKeys.Contains(key)) _checkboxKeys.Add(key);
    }

    public static bool IsRegisteredCheckbox(object tag)
    {
        string key = Convert.ToString(tag);
        return !string.IsNullOrEmpty(key) && _checkboxKeys.Contains(key);
    }

    public static void Apply(CheckBox checkBox)
    {
        // The custom CheckBox owns rendering. Registration above tells it which
        // visual mode to use without replacing the control instance.
    }
}
