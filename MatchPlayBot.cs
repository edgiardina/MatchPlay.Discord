using DSharpPlus;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Converters;
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
            matchPlayPusherClient = new MatchPlayPusherClient(_logger);
            matchPlaySubscriptionService = new MatchPlaySubscriptionService(new TournamentSubscriptionService(), matchPlayPusherClient);
        }

        /// <summary>
        /// Start the MatchPlay Discord Bot and listen infinitely for `/matchplay` messages
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await ConnectToMatchPlay();
            await ConnectToDiscord();

            exitEvent.WaitOne();
        }

        private async Task ConnectToDiscord()
        {
            var services = new ServiceCollection()
                    .AddSingleton(matchPlaySubscriptionService)
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

            _logger.LogInformation("Connected to Discord");
        }

        private async Task ConnectToMatchPlay()
        {
            matchPlayPusherClient.TournamentEventReceived += TournamentEventReceived;

            await matchPlayPusherClient.Connect();

            await matchPlaySubscriptionService.ListenToAllActiveSubscriptions();

            _logger.LogInformation("Connected to MatchPlay and listening to active subscriptions");
        }

        private async void TournamentEventReceived(object sender, TournamentEventEventArgs e)
        {
            _logger.LogInformation($"Received tournament event: {e.TournamentEvent}");

            if (e.TournamentEvent == TournamentEvents.RoundCreatedOrUpdated)
            {

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

                // TODO: Parse data into data object
                var data = JsonSerializer.Deserialize<RoundCreatedOrUpdated>(e.Data.ToString(), options);

                // check for subscription, if one exists, send message to Discord
                var subscriptions = await matchPlaySubscriptionService.GetTournamentSubscriptionsAsync(data.TournamentId);
                if (subscriptions != null)
                {
                    foreach (var subscription in subscriptions)
                    {
                        // send message to Discord
                        _logger.LogInformation($"Sending message to Discord channel {subscription.DiscordChannelId}");
                        await discordClient.GetChannelAsync(subscription.DiscordChannelId).Result.SendMessageAsync($"Tournament event: {e.TournamentEvent} Name {data.Name}");
                    }
                }
            }
        }
    }
}

