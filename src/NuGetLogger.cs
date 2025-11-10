using NuGet.Common;

namespace PackageGenerator;

public class NuGetLogger : LoggerBase
{
    public override void Log(ILogMessage message)
    {
        Console.WriteLine($"{message.Level}: {message.Message}");
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}
