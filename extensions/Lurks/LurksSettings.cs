// Paste this action into Streamer.bot after adding DuhBuhUI.cs to the same code action.
// The action only defines settings; runtime behavior is in Lurks.cs.

using System;
using System.Collections.Generic;
using System.Windows;

public class CPHInline
{
    public string extensionName = "duhBuh Lurks";
    public string extensionVersion = "0.1.0";

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

        ui.AddTitle("General Settings", "General");
        ui.AddToggleSwitch("Use 24h format", "Display lurk start times using 24-hour time.", "General", "duhbuh_lurks_24hFormat", true);
        ui.AddToggleSwitch("Remove Unpresent Lurkers", "Automatically end lurks when viewers have been absent from Streamer.bot's Twitch Present Viewers list for the configured timeout.", "General", "duhbuh_lurks_removeUnpresentLurkers", true);
        ui.AddSlider("Unpresent Lurker Timeout", "Minutes a lurker must remain absent before their lurk is automatically ended. Your Present Viewers update interval affects how quickly this can be detected.", "General", "duhbuh_lurks_unpresentTimeoutMinutes", 1, 60, 5);
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
