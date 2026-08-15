// duhBuhUIBannerAssets - branding assets used by the settings UI.
// The PNGs live in the repository. We download them once to a local cache so
// WPF can load them reliably from a file URI inside Streamer.bot.

using System;
using System.IO;
using System.Net;

public static class DuhBuhUIBannerAssets
{
    private const string DarkUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Dark%20Banner.png";
    private const string LightUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Light%20Banner.png";

    public static string DarkUri { get { return GetCachedUri("RTS-Dark-Banner.png", DarkUrl, "__duhbuh_headerDarkImage"); } }
    public static string LightUri { get { return GetCachedUri("RTS-Light-Banner.png", LightUrl, "__duhbuh_headerLightImage"); } }

    private static string GetCachedUri(string fileName, string url, string globalKey)
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

        try
        {
            // DuhBuhUI's string reader uses Streamer.bot globals. Store the resolved
            // URI there so an unset/empty global cannot hide the registered header.
            CPH.SetGlobalVar(globalKey, result, true);
        }
        catch
        {
            // The UI still has the URI in its own registered defaults.
        }

        return result;
    }
}
