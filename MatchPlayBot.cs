using DSharpPlus;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Discord;
using MatchPlay.Discord.Pusher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;

namespace MatchPlay.Discord
{
    public class MatchPlayBot
    {
        ManualResetEvent exitEvent = new ManualResetEvent(false);


        ILogger<MatchPlayBot> _logger;
        private string discordToken;

        private DiscordClient discordClient;
        private MatchPlayPusherClient matchPlayPusherClient;


        public MatchPlayBot(ILogger<MatchPlayBot> logger, string discordToken)
        {
            _logger = logger;
            this.discordToken = discordToken;
        }

        /// <summary>
        /// Start the MatchPlay Discord Bot and listen infinitely for `/matchplay` messages
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await ConnectToDiscord();
            await ConnectToMatchPlay();


            exitEvent.WaitOne();
        }

        private async Task ConnectToDiscord()
        {
            var services = new ServiceCollection()
                    .AddSingleton(matchPlayPusherClient)
                    .BuildServiceProvider();

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = this.discordToken,
                TokenType = TokenType.Bot
            });

            var slashCommands = discordClient.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });
            slashCommands.RegisterCommands<MatchPlaySlashCommand>();

            await discordClient.ConnectAsync();
        }

        private async Task ConnectToMatchPlay()
        {
            matchPlayPusherClient = new MatchPlayPusherClient(_logger);
            await matchPlayPusherClient.Connect();
        }
    }
}
