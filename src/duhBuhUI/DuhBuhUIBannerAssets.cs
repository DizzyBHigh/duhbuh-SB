// duhBuhUIBannerAssets - local branding asset resolver for the settings UI.
// The banner PNGs are runtime assets, not embedded in C#.

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
        if (!string.IsNullOrEmpty(path)) return ToFileUri(path);

        // Known local checkout used during development.
        path = FindFrom(@"F:\Projects\duhbuh-SB", fileName);
        if (!string.IsNullOrEmpty(path)) return ToFileUri(path);

        path = FindFrom(Environment.CurrentDirectory, fileName);
        if (!string.IsNullOrEmpty(path)) return ToFileUri(path);

        path = FindFrom(AppDomain.CurrentDomain.BaseDirectory, fileName);
        if (!string.IsNullOrEmpty(path)) return ToFileUri(path);

        // Last resort. This only works once the PNGs have been committed to GitHub.
        return "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/" + Uri.EscapeDataString(fileName);
    }

    private static string ToFileUri(string path)
    {
        return new Uri(Path.GetFullPath(path), UriKind.Absolute).AbsoluteUri;
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
