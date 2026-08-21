namespace TokenGrabber.Core;

using TokenGrabber.Models;

internal sealed class SessionManager
{
    private readonly Dictionary<string, List<GrabbedToken>> _sessions = new();

    public void AddToken(GrabbedToken token)
    {
        if (!_sessions.TryGetValue(token.Platform, out var list))
        {
            list = [];
            _sessions[token.Platform] = list;
        }

        if (!list.Any(t => t.Token == token.Token))
            list.Add(token);
    }

    public IReadOnlyList<GrabbedToken> GetTokens(string platform)
    {
        return _sessions.TryGetValue(platform, out var list) ? list.AsReadOnly() : [];
    }

    public IReadOnlyList<GrabbedToken> GetAllTokens()
    {
        return _sessions.Values.SelectMany(v => v).ToList().AsReadOnly();
    }

    public int TotalCount => _sessions.Values.Sum(v => v.Count);

    public Dictionary<string, int> GetSummary()
    {
        return _sessions.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
    }

    public void Clear() => _sessions.Clear();
}
