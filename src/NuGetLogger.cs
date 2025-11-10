using System.Text.RegularExpressions;
using NuGet.Common;

namespace PackageGenerator;

public class NuGetLogger : LoggerBase
{
    private static readonly Regex secondRegex = new(@"\(retry after: (\d+)s\)", RegexOptions.Compiled);
    public int? WaitSeconds { get; private set; }

    public override void Log(ILogMessage message)
    {
        if (message.Message.TrimStart().StartsWith("Forbidden", StringComparison.InvariantCultureIgnoreCase))
        {
            var match = secondRegex.Match(message.Message);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var seconds))
            {
                WaitSeconds = seconds;
            }
        }
        Console.WriteLine($"{message.Level}: {message.Message}");
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}
