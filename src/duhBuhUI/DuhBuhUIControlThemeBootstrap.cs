using System.Runtime.CompilerServices;

// Ensure shared control themes are registered before any duhBuhUI window,
// including secondary picker dialogs, is displayed.
internal static class DuhBuhUIControlThemeBootstrap
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        DuhBuhUIButtonTheme.Initialize();
    }
}
