using DSharpPlus;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Discord;
using MatchPlay.Discord.Pusher;
using MatchPlay.Discord.Services;
using MatchPlay.Discord.Subscriptions;
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
        private readonly TournamentSubscriptionService tournamentSubscriptionService = new TournamentSubscriptionService();


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

            // TODO: have matchplay slash command invoke MatchPlayPusherClient Events

            await discordClient.ConnectAsync();
        }

        private async Task ConnectToMatchPlay()
        {
            matchPlayPusherClient = new MatchPlayPusherClient(_logger);

            matchPlayPusherClient.TournamentEventReceived += TournamentEventReceived;

            await matchPlayPusherClient.Connect();

            // on startup, pull all tournaments and send matchPlay subscribe message for each
            var subscriptions = tournamentSubscriptionService.GetAllActiveSubscriptions();
            foreach (var subscription in subscriptions)
            {
                await matchPlayPusherClient.SubscribeToTournament(subscription.TournamentId);
            }
        }

        private void TournamentEventReceived(object sender, TournamentEventEventArgs e)
        {
            _logger.LogInformation($"Received tournament event: {e.TournamentEvent}");

            if (e.TournamentEvent == TournamentEvents.RoundCreatedOrUpdated)
            {
                // TODO: Parse data into data object

                // check for subscription, if one exists, send message to Discord
                var subscription = tournamentSubscriptionService.GetSubscriptionForTournament(1);
                if (subscription != null)
                {
                    // send message to Discord
                    _logger.LogInformation($"Sending message to Discord channel {subscription.DiscordChannelId}");
                    discordClient.GetChannelAsync(subscription.DiscordChannelId).Result.SendMessageAsync($"Tournament event: {e.TournamentEvent}");
                }
            }
        }
    }
}

