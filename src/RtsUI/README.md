# RtsUI

Reusable WPF settings UI library for Streamer.bot C# actions.

## Build

The project targets .NET Framework 4.8 and WPF. Build `RtsUI.csproj` on Windows with Visual Studio/MSBuild.

The GitHub Actions workflow builds the library on Windows and publishes `RtsUI.dll` as a workflow artifact.

## Runtime model

Streamer.bot actions supply the Streamer.bot persistence/logging callbacks to `RtsUI`. The library owns the settings-window implementation and control styling.

Example:

```csharp
var ui = new RtsUI(
    extensionName,
    extensionVersion,
    (key, persisted) => CPH.GetGlobalVar<bool?>(key, persisted),
    (key, persisted) => CPH.GetGlobalVar<int?>(key, persisted),
    (key, persisted) => CPH.GetGlobalVar<string>(key, persisted),
    (key, persisted) => CPH.GetGlobalVar<object>(key, persisted),
    (key, value, persisted) => CPH.SetGlobalVar(key, value, persisted),
    message => CPH.LogInfo(message)
);

ui.AddHeader(darkBanner, lightBanner);
ui.AddDropdown("Appearance", "Settings theme.", "General", "__rts_ui_theme", new[] { "Dark", "Light", "System" }, "Dark");
ui.AddToggleSwitch("Enabled", "Enable the feature.", "General", "enabled", true);
ui.AddSlider("Duration", "Display duration.", "General", "duration", 1, 60, 5);
ui.ShowUI();
```

The action should only describe settings and persistence. WPF control creation, theme resources, layout, and visual styling belong in the library.

## Current reusable controls

- Header/banner
- Tabs
- Cards/section layout
- Dropdowns
- Radio groups
- Toggles
- Sliders
- Text boxes
- Color/date/time controls
- Buttons
- Dark/light theme handling
- Settings persistence through supplied callbacks

The Lurks Position 3×3 grid remains a custom layout supported by the library.
