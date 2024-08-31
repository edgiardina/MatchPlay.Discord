using MatchPlay.Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

Console.WriteLine("Starting MatchPlay Discord Bot");

using var factory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("MatchPlay.Discord.Program", LogLevel.Debug)
        .AddConsole();

});
var logger = factory.CreateLogger<MatchPlayBot>();

var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>();
IConfiguration config = builder.Build();

var matchPlayBot = new MatchPlayBot(logger, config["Discord:Token"]);

await matchPlayBot.Run();
