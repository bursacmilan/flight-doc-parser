using FlightParser;
using FlightParser.ConsoleApp;
using FlightParser.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var path = args.Length switch
{
    0 => await PromptForPathAsync(),
    1 => args[0],
    _ => null,
};

if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
{
    await Console.Error.WriteLineAsync(string.IsNullOrWhiteSpace(path) ? "No file given." : $"File not found: {path}");
    await Console.Error.WriteLineAsync("Usage: FlightParser.ConsoleApp [path-to-flight-planning-pdf]");
    return ExitCodes.InvalidUsage;
}

await using var services = new ServiceCollection()
    .AddLogging(logging => logging
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole(console => console.LogToStandardErrorThreshold = LogLevel.Trace))
    .AddFlightPlanParser()
    .BuildServiceProvider();

var parser = services.GetRequiredService<IFlightPlanParser>();

await using var pdf = File.OpenRead(path);
var result = await parser.ParseAsync(pdf);

return ResultWriter.Write(result, Console.Out, Console.Error);

static async Task<string?> PromptForPathAsync()
{
    await Console.Error.WriteAsync("Path to the flight planning PDF: ");
    var input = await Console.In.ReadLineAsync();
    return input?.Trim().Trim('"', '\'');
}
