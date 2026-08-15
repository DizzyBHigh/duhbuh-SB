// duhBuhSettings - shared settings persistence conventions for Streamer.bot C# actions.
// Canonical keys use dotted namespaces. Legacy underscore keys may be read during migration.

using System;

public sealed class DuhBuhSettings
{
    private readonly Func<string, bool, bool?> _getBool;
    private readonly Func<string, bool, int?> _getInt;
    private readonly Func<string, bool, string> _getString;
    private readonly Action<string, object, bool> _setGlobal;
    private readonly Action<string> _logInfo;

    public DuhBuhSettings(
        Func<string, bool, bool?> getBool,
        Func<string, bool, int?> getInt,
        Func<string, bool, string> getString,
        Action<string, object, bool> setGlobal,
        Action<string> logInfo)
    {
        _getBool = getBool;
        _getInt = getInt;
        _getString = getString;
        _setGlobal = setGlobal;
        _logInfo = logInfo;
    }

    public bool GetBool(string key, bool defaultValue, string legacyKey = null)
    {
        bool? value = ReadBool(key);
        if (value.HasValue) return value.Value;
        if (!string.IsNullOrWhiteSpace(legacyKey))
        {
            value = ReadBool(legacyKey);
            if (value.HasValue)
            {
                Write(key, value.Value);
                _logInfo("[duhBuhSettings] Migrated " + legacyKey + " -> " + key);
                return value.Value;
            }
        }
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue, string legacyKey = null)
    {
        int? value = ReadInt(key);
        if (value.HasValue) return value.Value;
        if (!string.IsNullOrWhiteSpace(legacyKey))
        {
            value = ReadInt(legacyKey);
            if (value.HasValue)
            {
                Write(key, value.Value);
                _logInfo("[duhBuhSettings] Migrated " + legacyKey + " -> " + key);
                return value.Value;
            }
        }
        return defaultValue;
    }

    public string GetString(string key, string defaultValue, string legacyKey = null)
    {
        string value = ReadString(key);
        if (value != null) return value;
        if (!string.IsNullOrWhiteSpace(legacyKey))
        {
            value = ReadString(legacyKey);
            if (value != null)
            {
                Write(key, value);
                _logInfo("[duhBuhSettings] Migrated " + legacyKey + " -> " + key);
                return value;
            }
        }
        return defaultValue;
    }

    public void Write(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A settings key is required.", "key");
        _setGlobal(key, value, true);
    }

    private bool? ReadBool(string key)
    {
        try { return _getBool(key, true); }
        catch { return null; }
    }

    private int? ReadInt(string key)
    {
        try { return _getInt(key, true); }
        catch { return null; }
    }

    private string ReadString(string key)
    {
        try { return _getString(key, true); }
        catch { return null; }
    }
}
