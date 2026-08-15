// Paste this action into Streamer.bot after adding DuhBuhUI.cs and DuhBuhUIBannerAssets.cs to the same code action.
// Lurks settings uses the shared duhBuhUI controls and persists overlay profile settings.

using System;
using System.Collections.Generic;
using System.Windows;

public class CPHInline
{
    public string extensionName = "duhBuh Lurks";
    public string extensionVersion = "0.3.0";

    public bool Execute()
    {
        var ui = new DuhBuhUI(
            extensionName,
            extensionVersion,
            (key, persisted) => CPH.GetGlobalVar<bool?>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<int?>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<string>(key, persisted),
            (key, persisted) => CPH.GetGlobalVar<object>(key, persisted),
            (key, value, persisted) => CPH.SetGlobalVar(key, value, persisted),
            message => CPH.LogInfo(message)
        );

        // Settings UI branding. Resolve the local repo assets; no image data is embedded in the C# source.
        ui.AddHeader(DuhBuhUIBannerAssets.DarkUri, DuhBuhUIBannerAssets.LightUri);
        ui.AddThemeSelector("Appearance", "Choose the settings UI theme. System currently follows the dark palette until OS theme detection is added.", "General", "duhbuh_ui_theme", "Dark");

        ui.AddTitle("General Settings", "General");
        ui.AddToggleSwitch("Use 24h format", "Display lurk start times using 24-hour time.", "General", "duhbuh_lurks_24hFormat", true);
        ui.AddToggleSwitch("Remove Unpresent Lurkers", "Automatically end lurks when viewers have been absent from Streamer.bot's Twitch Present Viewers list for the configured timeout.", "General", "duhbuh_lurks_removeUnpresentLurkers", true);
        ui.AddSlider("Unpresent Lurker Timeout", "Minutes a lurker must remain absent before their lurk is automatically ended.", "General", "duhbuh_lurks_unpresentTimeoutMinutes", 1, 60, 5);
        ui.AddToggleSwitch("Unlurk Chatters", "End a lurk automatically after the configured number of chat messages.", "General", "duhbuh_lurks_chattingUnlurks", true);
        ui.AddSlider("Unlurk Chatters Threshold", "Number of chat messages required to end a lurk.", "General", "duhbuh_lurks_chattingUnlurksThreshold", 1, 10, 3);
        ui.AddToggleSwitch("Send Messages As Replies", "Post chat responses as replies instead.", "General", "duhbuh_lurks_postMessagesAsReplies", true);
        ui.AddSlider("Leaderboard Ranks", "Number of ranks shown by the leaderboard action.", "General", "duhbuh_lurks_leaderboardRankAmount", 3, 20, 5);

        ui.AddTitle("Chat Responses", "Chat Responses");
        ui.AddTextbox("Lurk Start", "Variables: %user%, %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkStart", "@%user% is now lurking!", false);
        ui.AddTextbox("Already Lurking", "Variables: %user%, %lurkStartTime%", "Chat Responses", "duhbuh_lurks_messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!", false);
        ui.AddTextbox("Lurk End", "Variables: %user%, %lurkTime%, %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.", false);
        ui.AddTextbox("Lurk End (hasn't lurked)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.", false);
        ui.AddTextbox("Lurk Check", "Variables: %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkCheck", "There are currently %lurkerCount% lurkers:", false);
        ui.AddTextbox("Lurk Check (No one lurking)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkCheckNoOneLurking", "@%user%, no one's currently lurking.", false);
        ui.AddTextbox("Lurk Stats", "Variables: %user%, %lurkCount%, %totalLurkTime%, %averageLurkTime%", "Chat Responses", "duhbuh_lurks_messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total time of %totalLurkTime%. Your average lurking time is %averageLurkTime%.", false);
        ui.AddTextbox("Lurk Stats (hasn't lurked yet)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.", false);
        ui.AddTextbox("Leaderboard Infix", "Used between lurk count and total time in leaderboard output.", "Chat Responses", "duhbuh_lurks_messagesLeaderboardInfix", "times for a total of", false);
        ui.AddTextbox("Leaderboard Own Rank", "Variables: %user%, %rank%, %lurkCount%, %totalLurkTime%", "Chat Responses", "duhbuh_lurks_messagesLeaderboardOwnRank", "@%user%, your own rank is #%rank% with %lurkCount% lurks and a total of %totalLurkTime%", false);

        ui.AddTitle("Position & Queue", "Overlay - Lurks");
        ui.AddToggleSwitch("Enable Lurk Notifications", "Show voluntary !lurk and !unlurk notifications. Automatic unpresent-lurker removal remains silent.", "Overlay - Lurks", "duhbuh_overlay_lurks_enabled", true);
        ui.AddDropdown("Position", "Where the Lurk lane is anchored.", "Overlay - Lurks", "duhbuh_overlay_lurks_position", new[] { "top-left", "top-center", "top-right", "middle-left", "center", "middle-right", "bottom-left", "bottom-center", "bottom-right" }, "bottom-center");
        ui.AddSlider("Horizontal Offset", "Pixel inset from the selected anchor.", "Overlay - Lurks", "duhbuh_overlay_lurks_offsetX", 0, 1000, 0);
        ui.AddSlider("Vertical Offset", "Pixel inset from the selected anchor.", "Overlay - Lurks", "duhbuh_overlay_lurks_offsetY", 0, 1000, 0);
        ui.AddSlider("Maximum Visible", "Maximum number of Lurk notifications visible at once.", "Overlay - Lurks", "duhbuh_overlay_lurks_maxVisible", 1, 10, 3);
        ui.AddSlider("Maximum Queued", "Maximum number of additional Lurk notifications waiting to be shown.", "Overlay - Lurks", "duhbuh_overlay_lurks_maxQueued", 0, 50, 20);
        ui.AddRadioGroup("Stack Direction", "Automatic grows inward from the selected edge.", "Overlay - Lurks", "duhbuh_overlay_lurks_stackDirection", new[] { "auto", "forward", "reverse" }, "auto");
        ui.AddSlider("Notification Spacing", "Pixel spacing between Lurk notifications in the same lane.", "Overlay - Lurks", "duhbuh_overlay_lurks_spacing", 0, 100, 10);

        ui.AddTitle("Timing & Animation", "Overlay - Lurks");
        ui.AddSlider("Display Duration (seconds)", "How long each Lurk notification remains visible.", "Overlay - Lurks", "duhbuh_overlay_lurks_durationSeconds", 1, 60, 5);
        ui.AddDropdown("Enter Animation", "How a Lurk notification appears.", "Overlay - Lurks", "duhbuh_overlay_lurks_enterAnimation", new[] { "slide", "fade", "scale", "none" }, "slide");
        ui.AddSlider("Enter Duration (ms)", "Length of the entrance animation.", "Overlay - Lurks", "duhbuh_overlay_lurks_enterDurationMs", 0, 2000, 300);
        ui.AddDropdown("Exit Animation", "How a Lurk notification disappears.", "Overlay - Lurks", "duhbuh_overlay_lurks_exitAnimation", new[] { "fade", "slide", "scale", "none" }, "fade");
        ui.AddSlider("Exit Duration (ms)", "Length of the exit animation.", "Overlay - Lurks", "duhbuh_overlay_lurks_exitDurationMs", 0, 2000, 300);

        ui.AddTitle("Appearance", "Overlay - Lurks");
        ui.AddSlider("Scale (%)", "Overall notification scale. 100% is the default size.", "Overlay - Lurks", "duhbuh_overlay_lurks_scale", 50, 200, 100);
        ui.AddColorPicker("Background Colour", "Notification background. Use #RRGGBB or #AARRGGBB.", "Overlay - Lurks", "duhbuh_overlay_lurks_backgroundColor", "#E60F0F12");
        ui.AddColorPicker("Title Colour", "Title text colour.", "Overlay - Lurks", "duhbuh_overlay_lurks_titleColor", "#FFFFFFFF");
        ui.AddColorPicker("Message Colour", "Main message text colour.", "Overlay - Lurks", "duhbuh_overlay_lurks_messageColor", "#FFFFFFFF");
        ui.AddColorPicker("Meta Colour", "Secondary/meta text colour.", "Overlay - Lurks", "duhbuh_overlay_lurks_metaColor", "#B3FFFFFF");
        ui.AddColorPicker("Border Colour", "Notification border colour.", "Overlay - Lurks", "duhbuh_overlay_lurks_borderColor", "#00000000");
        ui.AddSlider("Background Opacity (%)", "Opacity of the notification background. The colour alpha is also respected.", "Overlay - Lurks", "duhbuh_overlay_lurks_backgroundOpacity", 0, 100, 90);
        ui.AddSlider("Border Width (px)", "Width of the notification border.", "Overlay - Lurks", "duhbuh_overlay_lurks_borderWidth", 0, 10, 0);
        ui.AddSlider("Border Radius (px)", "Corner radius of the notification.", "Overlay - Lurks", "duhbuh_overlay_lurks_borderRadius", 0, 50, 12);
        ui.AddSlider("Title Size (px)", "Title font size.", "Overlay - Lurks", "duhbuh_overlay_lurks_titleSize", 10, 72, 24);
        ui.AddSlider("Message Size (px)", "Main message font size.", "Overlay - Lurks", "duhbuh_overlay_lurks_messageSize", 8, 60, 18);
        ui.AddSlider("Meta Size (px)", "Meta font size.", "Overlay - Lurks", "duhbuh_overlay_lurks_messageSize", 8, 40, 13);

        ui.AddTitle("Translations", "Translations");
        ui.AddTextbox("second/seconds", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationSeconds", "second/seconds", false);
        ui.AddTextbox("minute/minutes", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationMinutes", "minute/minutes", false);
        ui.AddTextbox("hour/hours", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationHours", "hour/hours", false);
        ui.AddTextbox("day/days", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationDays", "day/days", false);
        ui.AddTextbox("for", "Translation for 'for'.", "Translations", "duhbuh_lurks_translationFor", "for", false);
        ui.AddTextbox("and", "Translation for 'and'.", "Translations", "duhbuh_lurks_translationAnd", "and", false);

        ui.AddTitle("Advanced", "Advanced");
        ui.AddClickableButton("Reset All Lurk Times", "Permanently removes all stored lurk start times, counts and totals.", "Reset", "", "Advanced", ResetLurks);
        ui.LogExistingSettings();
        ui.ShowUI();
        return true;
    }

    private void ResetLurks()
    {
        var confirm = MessageBox.Show("Reset all duhBuh Lurks statistics? This cannot be undone.", extensionName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;
        var confirm2 = MessageBox.Show("Are you REALLY REALLY sure? This cannot be undone.", extensionName, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm2 != MessageBoxResult.Yes) return;

        List<UserVariableValue<long>> starts = GetUsers<long>("duhbuh_lurks_start");
        for (int i = 0; i < starts.Count; i++) CPH.UnsetTwitchUserVar(starts[i].UserName, "duhbuh_lurks_start", true);
        List<UserVariableValue<long>> counts = GetUsers<long>("duhbuh_lurks_count");
        for (int i = 0; i < counts.Count; i++) CPH.UnsetTwitchUserVar(counts[i].UserName, "duhbuh_lurks_count", true);
        List<UserVariableValue<long>> totals = GetUsers<long>("duhbuh_lurks_totalSeconds");
        for (int i = 0; i < totals.Count; i++) CPH.UnsetTwitchUserVar(totals[i].UserName, "duhbuh_lurks_totalSeconds", true);
        List<UserVariableValue<long>> chat = GetUsers<long>("duhbuh_lurks_chatMessages");
        for (int i = 0; i < chat.Count; i++) CPH.UnsetTwitchUserVar(chat[i].UserName, "duhbuh_lurks_chatMessages", true);
        List<UserVariableValue<long>> present = GetUsers<long>("duhbuh_lurks_lastPresent");
        for (int i = 0; i < present.Count; i++) CPH.UnsetTwitchUserVar(present[i].UserName, "duhbuh_lurks_lastPresent", true);

        CPH.LogInfo("[duhBuh Lurks] All lurk statistics reset.");
        MessageBox.Show("All lurk statistics have been reset.", extensionName, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private List<UserVariableValue<T>> GetUsers<T>(string key)
    {
        try { return CPH.GetTwitchUsersVar<T>(key, true); }
        catch { return new List<UserVariableValue<T>>(); }
    }
}
