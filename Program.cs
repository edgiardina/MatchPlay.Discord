using DSharpPlus;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord;
using MatchPlay.Discord.Pusher;
using MatchPlay.Discord.Services;
using MatchPlay.Discord.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PinballApi;

Console.WriteLine("Starting MatchPlay Discord Bot");

var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>();
IConfiguration config = builder.Build();

var services = new ServiceCollection()
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options.AddConsole();
                })
                .AddSingleton<MatchPlayBot>()
                .AddSingleton<MatchPlayPusherClient>()
                .AddSingleton<MatchPlaySubscriptionService>()
                .AddSingleton(serviceProvider =>
                {
                    var discordClient = new DiscordClient(new DiscordConfiguration
                    {
                        Token = config["Discord:Token"],
                        TokenType = TokenType.Bot
                    });

                    discordClient.UseSlashCommands(new SlashCommandsConfiguration
                    {
                        Services = serviceProvider
                    });

                    return discordClient;
                })
                .AddSingleton<TournamentSubscriptionService>()
                .AddSingleton(serviceProvider => { 
                    return new MatchPlayApi(config["MatchPlay:MatchPlayApiToken"]);
                })
                .BuildServiceProvider();

var matchPlayBot = services.GetRequiredService<MatchPlayBot>();

await matchPlayBot.Run();