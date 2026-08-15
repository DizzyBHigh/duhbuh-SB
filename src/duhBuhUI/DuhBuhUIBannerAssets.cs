// duhBuhUIBannerAssets - branding assets used by the settings UI.
// WPF receives a local file URI. The resolver first checks the known development
// checkout, then DUHBUH_SB_ROOT, then the current/application directory, and finally
// downloads the repository asset to the temp cache.
//
// IMPORTANT: This is shared UI code, not a CPHInline action. Do not reference CPH.

using System;
using System.IO;
using System.Net;

public static class DuhBuhUIBannerAssets
{
    private const string DarkUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Dark%20Banner.png";
    private const string LightUrl = "https://raw.githubusercontent.com/DizzyBHigh/duhbuh-SB/main/overlays/assets/RTS%20Light%20Banner.png";

    public static string DarkUri { get { return Resolve("RTS Dark Banner.png", DarkUrl); } }
    public static string LightUri { get { return Resolve("RTS Light Banner.png", LightUrl); } }

    private static string Resolve(string fileName, string url)
    {
        string[] roots = new[]
        {
            @"F:\Projects\duhbuh-SB",
            Environment.GetEnvironmentVariable("DUHBUH_SB_ROOT"),
            Environment.CurrentDirectory,
            AppDomain.CurrentDomain.BaseDirectory
        };

        for (int i = 0; i < roots.Length; i++)
        {
            string root = roots[i];
            if (string.IsNullOrWhiteSpace(root)) continue;

            try
            {
                string path = Path.Combine(root, "overlays", "assets", fileName);
                if (File.Exists(path) && new FileInfo(path).Length >= 1024)
                    return new Uri(Path.GetFullPath(path), UriKind.Absolute).AbsoluteUri;
            }
            catch
            {
                // Try the next location.
            }
        }

        // Walk up from the current/application directory looking for the repo layout.
        string[] starts = new[] { Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory };
        for (int s = 0; s < starts.Length; s++)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(starts[s]);
                for (int depth = 0; dir != null && depth < 8; depth++, dir = dir.Parent)
                {
                    string path = Path.Combine(dir.FullName, "overlays", "assets", fileName);
                    if (File.Exists(path) && new FileInfo(path).Length >= 1024)
                        return new Uri(Path.GetFullPath(path), UriKind.Absolute).AbsoluteUri;
                }
            }
            catch
            {
                // Try the download fallback.
            }
        }

        try
        {
            string cacheDir = Path.Combine(Path.GetTempPath(), "duhBuhUI", "branding");
            Directory.CreateDirectory(cacheDir);
            string cachePath = Path.Combine(cacheDir, fileName);

            if (!File.Exists(cachePath) || new FileInfo(cachePath).Length < 1024)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, cachePath);
                }
            }

            if (File.Exists(cachePath) && new FileInfo(cachePath).Length >= 1024)
                return new Uri(Path.GetFullPath(cachePath), UriKind.Absolute).AbsoluteUri;
        }
        catch
        {
            // The caller can still attempt the repository URL as a final fallback.
        }

        return url;
    }
}
