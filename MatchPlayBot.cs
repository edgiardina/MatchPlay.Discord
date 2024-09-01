﻿using DSharpPlus;
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

            if (e.TournamentEvent == TournamentEvents.RoundCreatedOrUpdated)
            {
                try
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

                    var data = JsonSerializer.Deserialize<RoundCreatedOrUpdated>(e.Data.ToString(), options);

                    // check for subscription, if one exists, send message to Discord
                    var subscriptions = await matchPlaySubscriptionService.GetTournamentSubscriptionsAsync(data.TournamentId);
                    if (subscriptions != null)
                    {
                        // Look up details from MatchPlay API

                        // TODO: craft attractive looking Discord Embed

                        // Send embed to channels subscribed to this tournament
                        foreach (var subscription in subscriptions)
                        {
                            // send message to Discord
                            _logger.LogInformation($"Sending message to Discord channel {subscription.DiscordChannelId}");
                            var channel = await discordClient.GetChannelAsync(subscription.DiscordChannelId);
                            await channel.SendMessageAsync($"Tournament event: {e.TournamentEvent} Name {data.Name}");                         
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
}