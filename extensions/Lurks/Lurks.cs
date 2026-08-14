using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CPHInline
{
    private const string Prefix = "duhbuh.lurks.";
    private const string StartVar = "duhbuh_lurks_start";
    private const string CountVar = "duhbuh_lurks_count";
    private const string TotalVar = "duhbuh_lurks_totalSeconds";

    public bool Execute()
    {
        CPH.TryGetArg("duhbuhLurksAction", out string action);
        if (string.IsNullOrWhiteSpace(action))
        {
            CPH.LogWarn("[duhBuh Lurks] Missing 'duhbuhLurksAction'. Use start, end, check, stats, leaderboard, or chatUnlurk.");
            return false;
        }

        switch (action.Trim().ToLowerInvariant())
        {
            case "start": return StartLurk();
            case "end": return EndLurk();
            case "check": return CheckLurks();
            case "stats": return Stats();
            case "leaderboard": return Leaderboard();
            case "chatunlurk": return ChatUnlurk();
            default:
                CPH.LogWarn("[duhBuh Lurks] Unknown action: " + action);
                return false;
        }
    }

    private bool StartLurk()
    {
        if (!TryUser(out string user, out string display)) return false;
        if (GetStart(user).HasValue)
        {
            Send(Template("duhbuh_lurks_messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!", display, GetStart(user)?.ToLocalTime().ToString("HH:mm")));
            return true;
        }

        CPH.SetTwitchUserVar(user, StartVar, DateTime.UtcNow, true);
        var count = GetLong(user, CountVar);
        var lurkerCount = CountActiveLurkers();
        Send(Template("duhbuh_lurks_messagesLurkStart", "@%user% is now lurking!", display, null, lurkerCount));
        return true;
    }

    private bool EndLurk()
    {
        if (!TryUser(out string user, out string display)) return false;
        var start = GetStart(user);
        if (!start.HasValue)
        {
            Send(Template("duhbuh_lurks_messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.", display));
            return true;
        }

        var duration = DateTime.UtcNow - start.Value;
        if (duration.TotalSeconds < 0) duration = TimeSpan.Zero;
        var count = GetLong(user, CountVar) + 1;
        var total = GetLong(user, TotalVar) + (long)Math.Round(duration.TotalSeconds);
        CPH.SetTwitchUserVar(user, CountVar, count, true);
        CPH.SetTwitchUserVar(user, TotalVar, total, true);
        CPH.UnsetTwitchUserVar(user, StartVar, true);

        var message = Template("duhbuh_lurks_messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.", display);
        message = message.Replace("%lurkTime%", FormatDuration(duration));
        message = message.Replace("%lurkerCount%", CountActiveLurkers().ToString());
        Send(message);
        return true;
    }

    private bool CheckLurks()
    {
        var count = CountActiveLurkers();
        if (count == 0)
        {
            Send(GetGlobalString("duhbuh_lurks_messagesLurkCheckNoOneLurking", "No one's currently lurking.").Replace("%user%", CurrentDisplayName()));
            return true;
        }

        var names = CPH.GetTwitchUsersVar<DateTime>(StartVar, true)
            .Where(x => x.Value != default(DateTime))
            .Select(x => x.UserName)
            .Take(50)
            .ToList();
        var message = GetGlobalString("duhbuh_lurks_messagesLurkCheck", "There are currently %lurkerCount% lurkers:")
            .Replace("%lurkerCount%", count.ToString());
        Send(message + " " + string.Join(", ", names));
        return true;
    }

    private bool Stats()
    {
        if (!TryUser(out string user, out string display)) return false;
        var count = GetLong(user, CountVar);
        var total = GetLong(user, TotalVar);
        if (count == 0)
        {
            Send(Template("duhbuh_lurks_messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.", display));
            return true;
        }

        var average = TimeSpan.FromSeconds((double)total / count);
        var message = GetGlobalString("duhbuh_lurks_messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total of %totalLurkTime%. Your average lurking time is %averageLurkTime%.");
        message = message.Replace("%user%", display)
            .Replace("%lurkCount%", count.ToString())
            .Replace("%totalLurkTime%", FormatDuration(TimeSpan.FromSeconds(total)))
            .Replace("%averageLurkTime%", FormatDuration(average));
        Send(message);
        return true;
    }

    private bool Leaderboard()
    {
        var amount = GetGlobalInt("duhbuh_lurks_leaderboardRankAmount", 5);
        var users = CPH.GetTwitchUsersVar<long>(CountVar, true)
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Take(Math.Max(1, Math.Min(amount, 20)))
            .ToList();
        if (users.Count == 0)
        {
            Send("No lurk statistics yet.");
            return true;
        }

        var parts = new List<string>();
        int rank = 1;
        foreach (var item in users)
        {
            var total = GetLong(item.UserName, TotalVar);
            parts.Add($"#{rank} {item.UserName} ({item.Value} lurks / {FormatDuration(TimeSpan.FromSeconds(total))})");
            rank++;
        }
        Send(string.Join(" | ", parts));
        return true;
    }

    private bool ChatUnlurk()
    {
        if (!GetGlobalBool("duhbuh_lurks_chattingUnlurks", true)) return true;
        if (!TryUser(out string user, out string display)) return false;
        if (!GetStart(user).HasValue) return true;

        var threshold = Math.Max(1, Math.Min(10, GetGlobalInt("duhbuh_lurks_chattingUnlurksThreshold", 1)));
        var key = "duhbuh_lurks_chatMessages";
        var messages = GetLong(user, key) + 1;
        CPH.SetTwitchUserVar(user, key, messages, false);
        if (messages >= threshold)
        {
            CPH.UnsetTwitchUserVar(user, key, false);
            return EndLurk();
        }
        return true;
    }

    // Run this action from the Twitch Present Viewers trigger to remove viewers who
    // have not appeared in the present-viewer list for the configured period.
    public bool RemoveUnpresentLurkers()
    {
        if (!GetGlobalBool("duhbuh_lurks_removeUnpresentLurkers", true)) return true;
        if (!CPH.TryGetArg("users", out List<Dictionary<string, object>> users)) return false;
        var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in users)
        {
            if (item.TryGetValue("userLogin", out object login) && login != null) present.Add(login.ToString());
            else if (item.TryGetValue("userName", out object name) && name != null) present.Add(name.ToString());
        }

        var now = DateTime.UtcNow;
        foreach (var item in CPH.GetTwitchUsersVar<DateTime>(StartVar, true))
        {
            if (present.Contains(item.UserName)) continue;
            // Present Viewers is normally a periodic trigger; keep the removal grace
            // period at 15 minutes as a deliberately conservative default.
            if (now - item.LastWrite >= TimeSpan.FromMinutes(15))
            {
                CPH.UnsetTwitchUserVar(item.UserName, StartVar, true);
                CPH.LogInfo("[duhBuh Lurks] Removed absent lurk: " + item.UserName);
            }
        }
        return true;
    }

    private bool TryUser(out string user, out string display)
    {
        CPH.TryGetArg("userName", out user);
        CPH.TryGetArg("user", out string fallback);
        if (string.IsNullOrWhiteSpace(user)) user = fallback;
        CPH.TryGetArg("displayName", out display);
        if (string.IsNullOrWhiteSpace(display)) display = user;
        if (string.IsNullOrWhiteSpace(user))
        {
            CPH.LogWarn("[duhBuh Lurks] No Twitch userName/user argument was supplied.");
            return false;
        }
        return true;
    }

    private string CurrentDisplayName()
    {
        CPH.TryGetArg("displayName", out string display);
        return display ?? "you";
    }

    private DateTime? GetStart(string user)
    {
        try { return CPH.GetTwitchUserVar<DateTime?>(user, StartVar, true); }
        catch { return null; }
    }

    private long GetLong(string user, string key)
    {
        try { return CPH.GetTwitchUserVar<long?>(user, key, true) ?? 0; }
        catch { return 0; }
    }

    private int CountActiveLurkers()
    {
        try { return CPH.GetTwitchUsersVar<DateTime>(StartVar, true).Count(x => x.Value != default(DateTime)); }
        catch { return 0; }
    }

    private bool GetGlobalBool(string key, bool fallback)
    {
        try { return CPH.GetGlobalVar<bool?>(key, true) ?? fallback; } catch { return fallback; }
    }

    private int GetGlobalInt(string key, int fallback)
    {
        try { return CPH.GetGlobalVar<int?>(key, true) ?? fallback; } catch { return fallback; }
    }

    private string GetGlobalString(string key, string fallback)
    {
        try { return CPH.GetGlobalVar<string>(key, true) ?? fallback; } catch { return fallback; }
    }

    private string Template(string key, string fallback, string user, string startTime = null, int lurkerCount = 0)
    {
        return GetGlobalString(key, fallback)
            .Replace("%user%", user ?? "")
            .Replace("%lurkStartTime%", startTime ?? "")
            .Replace("%lurkerCount%", lurkerCount.ToString());
    }

    private string FormatDuration(TimeSpan value)
    {
        var days = (int)value.TotalDays;
        var hours = value.Hours;
        var minutes = value.Minutes;
        var seconds = value.Seconds;
        var parts = new List<string>();
        if (days > 0) parts.Add(days + " " + Plural("duhbuh_lurks_translationDays", "day/days", days));
        if (hours > 0) parts.Add(hours + " " + Plural("duhbuh_lurks_translationHours", "hour/hours", hours));
        if (minutes > 0) parts.Add(minutes + " " + Plural("duhbuh_lurks_translationMinutes", "minute/minutes", minutes));
        if (seconds > 0 || parts.Count == 0) parts.Add(seconds + " " + Plural("duhbuh_lurks_translationSeconds", "second/seconds", seconds));
        return string.Join(" ", parts);
    }

    private string Plural(string key, string fallback, int value)
    {
        var text = GetGlobalString(key, fallback);
        var split = text.Split('/');
        if (split.Length != 2) return text;
        return Math.Abs(value) == 1 ? split[0] : split[1];
    }

    private void Send(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        CPH.SendMessage(message, true, true);
    }
}
