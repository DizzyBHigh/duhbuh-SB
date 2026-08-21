using System.Runtime.CompilerServices;

// Ensure shared control themes are registered before any RtsUI window,
// including secondary picker dialogs, is displayed.
internal static class RtsUIControlThemeBootstrap
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RtsUIButtonTheme.Initialize();
    }
}
