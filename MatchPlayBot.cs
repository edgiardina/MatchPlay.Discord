using DSharpPlus;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Discord;
using MatchPlay.Discord.Pusher;
using MatchPlay.Discord.Pusher.Data;
using MatchPlay.Discord.Services;
using MatchPlay.Discord.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Text.Json;

namespace MatchPlay.Discord
{
    public class MatchPlayBot
    {
        ManualResetEvent exitEvent = new ManualResetEvent(false);


        ILogger<MatchPlayBot> _logger;
        private string discordToken;

        private DiscordClient discordClient;
        private MatchPlayPusherClient matchPlayPusherClient;
        private MatchPlaySubscriptionService matchPlaySubscriptionService;


        public MatchPlayBot(ILogger<MatchPlayBot> logger, string discordToken)
        {
            _logger = logger;
            this.discordToken = discordToken;
            matchPlaySubscriptionService = new MatchPlaySubscriptionService(new TournamentSubscriptionService(), matchPlayPusherClient);
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
                    .AddSingleton<MatchPlaySubscriptionService>()
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

            slashCommands.SlashCommandExecuted += async (s, e) =>
            {
                _logger.LogInformation($"Slash command executed: {e.Context.CommandName}");
            };

            await discordClient.ConnectAsync();
        }

        private async Task ConnectToMatchPlay()
        {
            matchPlayPusherClient = new MatchPlayPusherClient(_logger);

            matchPlayPusherClient.TournamentEventReceived += TournamentEventReceived;

            await matchPlayPusherClient.Connect();

            await matchPlaySubscriptionService.ListenToAllActiveSubscriptions();
        }

        private async void TournamentEventReceived(object sender, TournamentEventEventArgs e)
        {
            _logger.LogInformation($"Received tournament event: {e.TournamentEvent}");

            if (e.TournamentEvent == TournamentEvents.RoundCreatedOrUpdated)
            {
                // TODO: Parse data into data object
                var data = JsonSerializer.Deserialize<RoundCreatedOrUpdated>(e.Data.ToString());

                // check for subscription, if one exists, send message to Discord
                var subscription = await matchPlaySubscriptionService.GetTournamentSubscriptionAsync(data.TournamentId);
                if (subscription != null)
                {
                    // send message to Discord
                    _logger.LogInformation($"Sending message to Discord channel {subscription.DiscordChannelId}");
                    await discordClient.GetChannelAsync(subscription.DiscordChannelId).Result.SendMessageAsync($"Tournament event: {e.TournamentEvent} Name {data.Name}");
                }
            }
        }
    }
}

