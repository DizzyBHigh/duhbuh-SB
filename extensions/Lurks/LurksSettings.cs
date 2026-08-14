// Paste this action into Streamer.bot after adding DuhBuhUI.cs to the same code action,
// or use the combined source/package supplied by duhBuh releases.
//
// The action only defines settings; runtime behavior is in Lurks.cs.

using System;
using System.Windows;

public class CPHInline
{
    public string extensionName = "duhBuh Lurks";
    public string extensionVersion = "0.1.0";

    public bool Execute()
    {
        var ui = new DuhBuhUI(CPH, extensionName, extensionVersion);
        ui.AddTitle("General Settings", "General");
        ui.AddToggleSwitch("Use 24h format", "Display lurk start times using 24-hour time.", "General", "duhbuh_lurks_24hFormat", true);
        ui.AddToggleSwitch("Remove Unpresent Lurkers", "Remove lurks after 15 minutes without appearing in Streamer.bot's Twitch Present Viewers list.", "General", "duhbuh_lurks_removeUnpresentLurkers", true);
        ui.AddToggleSwitch("Unlurk Chatters", "End a lurk automatically after the configured number of chat messages.", "General", "duhbuh_lurks_chattingUnlurks", true);
        ui.AddSlider("Unlurk Chatters Threshold", "Number of chat messages required to end a lurk.", "General", "duhbuh_lurks_chattingUnlurksThreshold", 1, 10, 1);
        ui.AddToggleSwitch("Send Messages As Replies", "Reserved for the reply-aware message implementation.", "General", "duhbuh_lurks_postMessagesAsReplies", true);
        ui.AddSlider("Leaderboard Ranks", "Number of ranks shown by the leaderboard action.", "General", "duhbuh_lurks_leaderboardRankAmount", 3, 20, 5);

        ui.AddTitle("Chat Responses", "Chat Responses");
        ui.AddTextbox("Lurk Start", "Variables: %user%, %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkStart", "@%user% is now lurking!", false);
        ui.AddTextbox("Already Lurking", "Variables: %user%, %lurkStartTime%", "Chat Responses", "duhbuh_lurks_messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!", false);
        ui.AddTextbox("Lurk End", "Variables: %user%, %lurkTime%, %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.", false);
        ui.AddTextbox("Lurk End (hasn't lurked)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.", false);
        ui.AddTextbox("Lurk Check", "Variables: %lurkerCount%", "Chat Responses", "duhbuh_lurks_messagesLurkCheck", "There are currently %lurkerCount% lurkers:", false);
        ui.AddTextbox("Lurk Check (No one lurking)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkCheckNoOneLurking", "@%user%, no one's currently lurking.", false);
        ui.AddTextbox("Lurk Stats", "Variables: %user%, %lurkCount%, %totalLurkTime%, %averageLurkTime%", "Chat Responses", "duhbuh_lurks_messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total of %totalLurkTime%. Your average lurking time is %averageLurkTime%.", false);
        ui.AddTextbox("Lurk Stats (hasn't lurked yet)", "Variables: %user%", "Chat Responses", "duhbuh_lurks_messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.", false);

        ui.AddTitle("Translations", "Translations");
        ui.AddTextbox("second/seconds", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationSeconds", "second/seconds", false);
        ui.AddTextbox("minute/minutes", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationMinutes", "minute/minutes", false);
        ui.AddTextbox("hour/hours", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationHours", "hour/hours", false);
        ui.AddTextbox("day/days", "Singular/plural separated with '/'.", "Translations", "duhbuh_lurks_translationDays", "day/days", false);

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
        CPH.UnsetAllUsersVar("duhbuh_lurks_start", true);
        CPH.UnsetAllUsersVar("duhbuh_lurks_count", true);
        CPH.UnsetAllUsersVar("duhbuh_lurks_totalSeconds", true);
        CPH.UnsetAllUsersVar("duhbuh_lurks_chatMessages", false);
        CPH.LogInfo("[duhBuh Lurks] All lurk statistics reset.");
        MessageBox.Show("All lurk statistics have been reset.", extensionName, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
