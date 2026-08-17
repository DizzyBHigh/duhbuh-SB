// duhBuhUIBannerAssets - branding assets used by the settings UI.

using System;
using System.IO;
using System.Net;

public static class DuhBuhUIBannerAssets
{
    private const string DarkUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Dark%20Banner.png";
    private const string LightUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Light%20Banner.png";

    static DuhBuhUIBannerAssets()
    {
        DuhBuhUITheme.Initialize();
    }

    public static string DarkUri { get { return GetCachedUri("RTS-Dark-Banner.png", DarkUrl); } }
    public static string LightUri { get { return GetCachedUri("RTS-Light-Banner.png", LightUrl); } }

    private static string GetCachedUri(string fileName, string url)
    {
        string result = url;
        try
        {
            string directory = Path.Combine(Path.GetTempPath(), "duhBuhUI", "branding");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, fileName);

            if (!File.Exists(path) || new FileInfo(path).Length < 1024)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }

            if (File.Exists(path) && new FileInfo(path).Length >= 1024)
                result = new Uri(Path.GetFullPath(path), UriKind.Absolute).AbsoluteUri;
        }
        catch
        {
            // Keep the remote URI as the fallback.
        }

        return result;
    }
}
