// Standalone Streamer.bot test action for duhBuhUI.dll.
// Paste this into a temporary C# Execute Code action after placing duhBuhUI.dll in Streamer.bot's dll folder.

using System;

public class CPHInline
{
    public bool Execute()
    {
        DuhBuhUIButtonTheme.Initialize();

        DuhBuhUI ui = new DuhBuhUI(
            "duhBuhUI Test",
            "0.1.0",
            (key, persisted) => CPH.GetGlobalVar<bool?>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<int?>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<string>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<object>(key, persisted),
            (key, value, persisted) => CPH.SetGlobalVar(key, value, persisted),
            message => CPH.LogInfo(message)
        );

        ui.AddHeader(
            "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Dark%20Banner.png",
            "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Light%20Banner.png"
        );

        ui.AddThemeSelector(
            "Appearance",
            "Choose the settings UI theme.",
            "General",
            "duhbuh_ui_theme",
            "Dark"
        );

        ui.AddTitle("Control Test", "General");

        ui.AddDropdown(
            "Test Dropdown",
            "This verifies the reusable styled dropdown control.",
            "General",
            "test_dropdown",
            new[] { "First", "Second", "Third" },
            "First"
        );

        ui.AddRadioGroup(
            "Test Radio Group",
            "This verifies a reusable radio group.",
            "General",
            "test_radio",
            new[] { "Alpha", "Beta", "Gamma" },
            "Alpha"
        );

        ui.AddToggleSwitch(
            "Test Toggle",
            "This verifies the reusable toggle control.",
            "General",
            "test_toggle",
            true
        );

        ui.AddSlider(
            "Test Slider",
            "This verifies the reusable slider control.",
            "General",
            "test_slider",
            0,
            100,
            50
        );

        ui.AddTextbox(
            "Test Textbox",
            "This verifies the reusable textbox control.",
            "General",
            "test_textbox",
            "Hello from duhBuhUI",
            false
        );

        ui.ShowUI();
        return true;
    }
}
