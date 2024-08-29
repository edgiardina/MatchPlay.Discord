using MatchPlay.Discord;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting MatchPlay Discord Bot");

using var factory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
        .AddConsole();

});
var logger = factory.CreateLogger<MatchPlayBot>();

var matchPlayBot = new MatchPlayBot(logger, "");

await matchPlayBot.Run();
