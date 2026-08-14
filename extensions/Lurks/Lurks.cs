using System;
using System.Collections.Generic;

public class CPHInline
{
    private const string StartVar = "duhbuh_lurks_start";
    private const string CountVar = "duhbuh_lurks_count";
    private const string TotalVar = "duhbuh_lurks_totalSeconds";
    private const string ChatMessagesVar = "duhbuh_lurks_chatMessages";
    private const string LastPresentVar = "duhbuh_lurks_lastPresent";

    public bool Execute()
    {
        CPH.TryGetArg("duhbuhLurksAction", out string action);
        if (string.IsNullOrWhiteSpace(action)) return false;
        switch (action.Trim().ToLowerInvariant())
        {
            case "start": return StartLurk();
            case "end": return EndLurk();
            case "check": return CheckLurkers();
            case "stats": return Stats();
            case "leaderboard": return Leaderboard();
            case "chatunlurk": return ChatUnlurk();
            case "removeunpresentlurkers": return RemoveUnpresentLurkers();
            default: return false;
        }
    }

    public bool StartLurk()
    {
        if (!TryUser(out string user, out string display)) return false;
        long existing = GetLong(user, StartVar);
        if (existing > 0)
        {
            string startText = FromUnixSeconds(existing).ToLocalTime().ToString(GetGlobalBool("duhbuh_lurks_24hFormat", true) ? "HH:mm" : "h:mm tt");
            Send(Template("duhbuh_lurks_messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!", display, startText));
            return true;
        }

        long now = UnixSeconds(DateTime.UtcNow);
        CPH.SetTwitchUserVar(user, StartVar, now, true);
        CPH.SetTwitchUserVar(user, LastPresentVar, now, true);
        CPH.UnsetTwitchUserVar(user, ChatMessagesVar, true);
        Send(Template("duhbuh_lurks_messagesLurkStart", "@%user% is now lurking!", display, null, CountActiveLurkers()));
        return true;
    }

    public bool EndLurk()
    {
        if (!TryUser(out string user, out string display)) return false;
        long start = GetLong(user, StartVar);
        if (start <= 0)
        {
            Send(Template("duhbuh_lurks_messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.", display));
            return true;
        }

        long now = UnixSeconds(DateTime.UtcNow);
        long elapsed = now - start;
        if (elapsed < 0) elapsed = 0;
        long count = GetLong(user, CountVar) + 1;
        long total = GetLong(user, TotalVar) + elapsed;
        CPH.SetTwitchUserVar(user, CountVar, count, true);
        CPH.SetTwitchUserVar(user, TotalVar, total, true);
        ClearLurk(user);

        string message = Template("duhbuh_lurks_messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.", display);
        message = message.Replace("%lurkTime%", FormatDuration(TimeSpan.FromSeconds(elapsed)));
        message = message.Replace("%lurkerCount%", CountActiveLurkers().ToString());
        Send(message);
        return true;
    }

    public bool CheckLurkers()
    {
        List<UserVariableValue<long>> users = GetActiveLurkUsers();
        if (users.Count == 0)
        {
            Send(GetGlobalString("duhbuh_lurks_messagesLurkCheckNoOneLurking", "@%user%, no one's currently lurking.").Replace("%user%", CurrentDisplayName()));
            return true;
        }
        List<string> names = new List<string>();
        for (int i = 0; i < users.Count && names.Count < 50; i++) names.Add(users[i].UserName);
        string message = GetGlobalString("duhbuh_lurks_messagesLurkCheck", "There are currently %lurkerCount% lurkers:").Replace("%lurkerCount%", users.Count.ToString());
        Send(message + " " + string.Join(", ", names.ToArray()));
        return true;
    }

    public bool Stats()
    {
        if (!TryUser(out string user, out string display)) return false;
        long count = GetLong(user, CountVar);
        long total = GetLong(user, TotalVar);
        if (count == 0)
        {
            Send(Template("duhbuh_lurks_messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.", display));
            return true;
        }
        TimeSpan average = TimeSpan.FromSeconds((double)total / count);
        string message = GetGlobalString("duhbuh_lurks_messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total of %totalLurkTime%. Your average lurking time is %averageLurkTime%.");
        message = message.Replace("%user%", display).Replace("%lurkCount%", count.ToString()).Replace("%totalLurkTime%", FormatDuration(TimeSpan.FromSeconds(total))).Replace("%averageLurkTime%", FormatDuration(average));
        Send(message);
        return true;
    }

    public bool Leaderboard()
    {
        int amount = GetGlobalInt("duhbuh_lurks_leaderboardRankAmount", 5);
        if (amount < 1) amount = 1;
        if (amount > 20) amount = 20;
        List<UserVariableValue<long>> allUsers = GetTwitchUsersLong(CountVar);
        List<UserVariableValue<long>> ranked = new List<UserVariableValue<long>>();
        for (int i = 0; i < allUsers.Count; i++)
        {
            if (allUsers[i].Value <= 0) continue;
            int insertAt = ranked.Count;
            for (int j = 0; j < ranked.Count; j++) if (allUsers[i].Value > ranked[j].Value) { insertAt = j; break; }
            ranked.Insert(insertAt, allUsers[i]);
            if (ranked.Count > amount) ranked.RemoveAt(ranked.Count - 1);
        }
        if (ranked.Count == 0) { Send("No lurk statistics yet."); return true; }
        List<string> parts = new List<string>();
        for (int i = 0; i < ranked.Count; i++) parts.Add("#" + (i + 1) + " " + ranked[i].UserName + " (" + ranked[i].Value + " lurks / " + FormatDuration(TimeSpan.FromSeconds(GetLong(ranked[i].UserName, TotalVar))) + ")");
        Send(string.Join(" | ", parts.ToArray()));
        return true;
    }

    public bool ChatUnlurk()
    {
        if (!GetGlobalBool("duhbuh_lurks_chattingUnlurks", true)) return true;
        if (!TryUser(out string user, out string display)) return false;
        if (GetLong(user, StartVar) <= 0) return true;
        int threshold = GetGlobalInt("duhbuh_lurks_chattingUnlurksThreshold", 1);
        if (threshold < 1) threshold = 1;
        if (threshold > 10) threshold = 10;
        long messages = GetLong(user, ChatMessagesVar) + 1;
        CPH.SetTwitchUserVar(user, ChatMessagesVar, messages, true);
        if (messages >= threshold) return EndLurk();
        return true;
    }

    public bool RemoveUnpresentLurkers()
    {
        if (!GetGlobalBool("duhbuh_lurks_removeUnpresentLurkers", true)) return true;
        if (!CPH.TryGetArg("users", out List<Dictionary<string, object>> users)) return false;
        long now = UnixSeconds(DateTime.UtcNow);
        int timeoutMinutes = GetGlobalInt("duhbuh_lurks_unpresentTimeoutMinutes", 5);
        if (timeoutMinutes < 1) timeoutMinutes = 1;
        if (timeoutMinutes > 60) timeoutMinutes = 60;
        long timeoutSeconds = (long)timeoutMinutes * 60;
        List<UserVariableValue<long>> active = GetActiveLurkUsers();
        int removed = 0;
        for (int i = 0; i < active.Count; i++)
        {
            string username = active[i].UserName;
            bool present = false;
            for (int j = 0; j < users.Count; j++)
            {
                object login, name;
                if (users[j].TryGetValue("userLogin", out login) && login != null && string.Equals(login.ToString(), username, StringComparison.OrdinalIgnoreCase)) { present = true; break; }
                if (users[j].TryGetValue("userName", out name) && name != null && string.Equals(name.ToString(), username, StringComparison.OrdinalIgnoreCase)) { present = true; break; }
            }
            if (present)
                CPH.SetTwitchUserVar(username, LastPresentVar, now, true);
            else if (now - GetLongWithFallback(username, LastPresentVar, active[i].Value) >= timeoutSeconds)
            {
                long absentFor = now - GetLongWithFallback(username, LastPresentVar, active[i].Value);
                CPH.LogInfo("[duhBuh Lurks] Removed unpresent lurker: " + username + " after " + (absentFor / 60) + " minutes absent (timeout " + timeoutMinutes + " minutes).");
                ClearLurk(username);
                removed++;
            }
        }
        if (removed > 0)
            CPH.LogInfo("[duhBuh Lurks] Present Viewers check removed " + removed + " unpresent lurker(s).");
        return true;
    }

    private void ClearLurk(string user)
    {
        CPH.UnsetTwitchUserVar(user, StartVar, true);
        CPH.UnsetTwitchUserVar(user, ChatMessagesVar, true);
        CPH.UnsetTwitchUserVar(user, LastPresentVar, true);
    }

    private bool TryUser(out string user, out string display)
    {
        CPH.TryGetArg("userName", out user);
        CPH.TryGetArg("user", out string fallback);
        if (string.IsNullOrWhiteSpace(user)) user = fallback;
        CPH.TryGetArg("displayName", out display);
        if (string.IsNullOrWhiteSpace(display)) display = user;
        return !string.IsNullOrWhiteSpace(user);
    }

    private string CurrentDisplayName()
    {
        CPH.TryGetArg("displayName", out string display);
        return string.IsNullOrWhiteSpace(display) ? "you" : display;
    }

    private long GetLong(string user, string key)
    {
        try { return CPH.GetTwitchUserVar<long?>(user, key, true) ?? 0; } catch { return 0; }
    }

    private long GetLongWithFallback(string user, string key, long fallback)
    {
        long value = GetLong(user, key);
        return value > 0 ? value : fallback;
    }

    private List<UserVariableValue<long>> GetActiveLurkUsers()
    {
        try
        {
            List<UserVariableValue<long>> source = CPH.GetTwitchUsersVar<long>(StartVar, true);
            List<UserVariableValue<long>> result = new List<UserVariableValue<long>>();
            for (int i = 0; i < source.Count; i++) if (source[i].Value > 0) result.Add(source[i]);
            return result;
        }
        catch { return new List<UserVariableValue<long>>(); }
    }

    private List<UserVariableValue<long>> GetTwitchUsersLong(string key)
    {
        try { return CPH.GetTwitchUsersVar<long>(key, true); } catch { return new List<UserVariableValue<long>>(); }
    }

    private int CountActiveLurkers() { return GetActiveLurkUsers().Count; }

    private bool GetGlobalBool(string key, bool fallback) { try { return CPH.GetGlobalVar<bool?>(key, true) ?? fallback; } catch { return fallback; } }
    private int GetGlobalInt(string key, int fallback) { try { return CPH.GetGlobalVar<int?>(key, true) ?? fallback; } catch { return fallback; } }
    private string GetGlobalString(string key, string fallback) { try { return CPH.GetGlobalVar<string>(key, true) ?? fallback; } catch { return fallback; } }

    private string Template(string key, string fallback, string user, string startTime = null, int lurkerCount = 0)
    {
        return GetGlobalString(key, fallback).Replace("%user%", user ?? "").Replace("%lurkStartTime%", startTime ?? "").Replace("%lurkerCount%", lurkerCount.ToString());
    }

    private string FormatDuration(TimeSpan value)
    {
        int days = (int)value.TotalDays, hours = value.Hours, minutes = value.Minutes, seconds = value.Seconds;
        List<string> parts = new List<string>();
        if (days > 0) parts.Add(days + " " + Plural("duhbuh_lurks_translationDays", "day/days", days));
        if (hours > 0) parts.Add(hours + " " + Plural("duhbuh_lurks_translationHours", "hour/hours", hours));
        if (minutes > 0) parts.Add(minutes + " " + Plural("duhbuh_lurks_translationMinutes", "minute/minutes", minutes));
        if (seconds > 0 || parts.Count == 0) parts.Add(seconds + " " + Plural("duhbuh_lurks_translationSeconds", "second/seconds", seconds));
        return string.Join(" ", parts.ToArray());
    }

    private string Plural(string key, string fallback, int value)
    {
        string[] split = GetGlobalString(key, fallback).Split('/');
        if (split.Length != 2) return string.Join("/", split);
        return Math.Abs(value) == 1 ? split[0] : split[1];
    }

    private long UnixSeconds(DateTime utc) { return (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds; }
    private DateTime FromUnixSeconds(long value) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value); }

    private void Send(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        CPH.SendMessage(message, true, GetGlobalBool("duhbuh_lurks_postMessagesAsReplies", true));
    }
}
