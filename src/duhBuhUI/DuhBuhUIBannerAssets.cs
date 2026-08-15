// duhBuhUIBannerAssets - local branding asset resolver for the settings UI.
// Keep the PNGs in: overlays/assets/RTS Dark Banner.png and RTS Light Banner.png
// This file is intentionally small; the image files are NOT embedded in C#.

using System;
using System.IO;

public static class DuhBuhUIBannerAssets
{
    private const string DarkFileName = "RTS Dark Banner.png";
    private const string LightFileName = "RTS Light Banner.png";

    public static string DarkUri { get { return Resolve(DarkFileName); } }
    public static string LightUri { get { return Resolve(LightFileName); } }

    private static string Resolve(string fileName)
    {
        string root = Environment.GetEnvironmentVariable("DUHBUH_SB_ROOT");
        string path = FindFrom(root, fileName);
        if (!string.IsNullOrEmpty(path)) return new Uri(path).AbsoluteUri;

        path = FindFrom(Environment.CurrentDirectory, fileName);
        if (!string.IsNullOrEmpty(path)) return new Uri(path).AbsoluteUri;

        path = FindFrom(AppDomain.CurrentDomain.BaseDirectory, fileName);
        if (!string.IsNullOrEmpty(path)) return new Uri(path).AbsoluteUri;

        return "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/" + Uri.EscapeDataString(fileName).Replace("%20", "%20");
    }

    private static string FindFrom(string start, string fileName)
    {
        if (string.IsNullOrWhiteSpace(start)) return null;

        try
        {
            DirectoryInfo directory = new DirectoryInfo(start);
            for (int i = 0; i < 8 && directory != null; i++)
            {
                string candidate = Path.Combine(directory.FullName, "overlays", "assets", fileName);
                if (File.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }
        }
        catch { }

        return null;
    }
}
