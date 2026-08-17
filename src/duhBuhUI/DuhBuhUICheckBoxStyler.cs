using System.Windows.Controls;

// Compatibility shim retained while existing integrations move to the
// custom CheckBox control. Visuals and interaction are now owned by CheckBox.
public static class DuhBuhUICheckBoxStyler
{
    public static void Initialize()
    {
        // Intentionally empty. Custom CheckBox owns its own rendering.
    }

    public static void RegisterCheckboxKey(string key)
    {
        // Retained for source compatibility with older integrations.
    }

    public static void Apply(CheckBox checkBox)
    {
        // Intentionally empty. Custom CheckBox owns its own rendering.
    }
}
