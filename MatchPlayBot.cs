using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Converters;
using MatchPlay.Discord.Discord;
using MatchPlay.Discord.Pusher;
using MatchPlay.Discord.Pusher.Data;
using MatchPlay.Discord.Subscriptions;
using Microsoft.Extensions.Logging;
using PinballApi;
using System.Reactive.Linq;
using System.Text.Json;

namespace MatchPlay.Discord
{
    public class MatchPlayBot
    {
        ManualResetEvent exitEvent = new ManualResetEvent(false);

        private readonly ILogger<MatchPlayBot> _logger;
        private readonly DiscordClient discordClient;
        private readonly MatchPlayPusherClient matchPlayPusherClient;
        private readonly MatchPlaySubscriptionService matchPlaySubscriptionService;
        private readonly MatchPlayApi matchPlayApi;

        public MatchPlayBot(MatchPlayPusherClient pusherClient, MatchPlaySubscriptionService subscriptionService, DiscordClient discordClient, MatchPlayApi matchPlayApi, ILogger<MatchPlayBot> logger)
        {
            _logger = logger;
            this.discordClient = discordClient;
            matchPlayPusherClient = pusherClient;
            matchPlaySubscriptionService = subscriptionService;
            this.matchPlayApi = matchPlayApi;
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
            var slashCommands = discordClient.GetSlashCommands();

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

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

            switch (e.TournamentEvent)
            {
                case TournamentEvents.RoundCreatedOrUpdated:
                    var roundCreatedOrUpdated = JsonSerializer.Deserialize<RoundCreatedOrUpdated>(e.Data.ToString(), options);
                    await RoundCreatedOrUpdated(roundCreatedOrUpdated);
                    break;
            }
        }

        private async Task RoundCreatedOrUpdated(RoundCreatedOrUpdated roundCreatedOrUpdated)
        {
            _logger.LogInformation($"Round created or updated: {roundCreatedOrUpdated.Name}");

            try
            {
                // check for subscription, if one exists, send message to Discord
                var subscriptions = await matchPlaySubscriptionService.GetTournamentSubscriptionsAsync(roundCreatedOrUpdated.TournamentId);
                if (subscriptions != null)
                {
                    // Look up details from MatchPlay API
                    var tournament = await matchPlayApi.GetTournament((int)roundCreatedOrUpdated.TournamentId, includeArenas: true);

                    var games = await matchPlayApi.GetGames(new List<int> { (int)roundCreatedOrUpdated.TournamentId }, round: (int)roundCreatedOrUpdated.RoundId);

                    if (games != null)
                    {
                        // TODO: craft attractive looking Discord Embed
                        var embed = new DiscordEmbedBuilder()
                            .WithTitle($"{tournament.Name} - {roundCreatedOrUpdated.Name}")
                            .WithDescription($"{roundCreatedOrUpdated.Name} in tournament {tournament.Name} has been created or updated")
                            .WithColor(DiscordColor.Green);

                        foreach (var match in games)
                        {
                            // TODO: look up game and get game name
                            var game = await matchPlayApi.GetGame((int)roundCreatedOrUpdated.TournamentId, match.GameId);

                            var playerString = String.Join("\n", game.PlayerIds.Select(n => tournament.Players.SingleOrDefault(m => m.PlayerId == n)?.Name ?? n.ToString()));

                            embed.AddField(game.Arena?.Name ?? "No Arena", playerString);
                        }

                        // Send embed to channels subscribed to this tournament
                        foreach (var subscription in subscriptions)
                        {
                            // send message to Discord
                            _logger.LogInformation($"Sending message to Discord channel {subscription.DiscordChannelId}");
                            var channel = await discordClient.GetChannelAsync(subscription.DiscordChannelId);
                            await channel.SendMessageAsync(embed);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Round {roundCreatedOrUpdated.RoundId} has no games for tournament {roundCreatedOrUpdated.TournamentId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tournament event");
            }
        }
    }
}