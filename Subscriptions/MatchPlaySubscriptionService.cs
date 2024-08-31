using MatchPlay.Discord.Pusher;
using MatchPlay.Discord.Services;

namespace MatchPlay.Discord.Subscriptions
{
    public class MatchPlaySubscriptionService
    {
        TournamentSubscriptionService _tournamentSubscriptionService;
        MatchPlayPusherClient _pusherService;

        /// <summary>
        /// Manages Subscriptions to MatchPlay tournaments data
        /// </summary>
        /// <param name="tournamentSubscriptionService"></param>
        /// <param name="matchPlayPusherClient"></param>
        public MatchPlaySubscriptionService(TournamentSubscriptionService tournamentSubscriptionService, MatchPlayPusherClient matchPlayPusherClient)
        {
            _tournamentSubscriptionService = tournamentSubscriptionService;
            _pusherService = matchPlayPusherClient;
        }


        public async Task SubscribeAsync(long tournamentId, ulong discordChannelId)
        {
            // Save the subscription to the Sqlite database
            _tournamentSubscriptionService.SubscribeToTournament(tournamentId, discordChannelId);

            // Subscribe to the Pusher channel
            await _pusherService.SubscribeToTournament(tournamentId);
        }

        public async Task UnsubscribeAsync(long tournamentId, ulong discordChannelId)
        {
            // Remove the subscription from the Sqlite database
            _tournamentSubscriptionService.UnsubscribeFromTournament(tournamentId, discordChannelId);

            // Unsubscribe from the Pusher channel
            await _pusherService.UnsubscribeFromTournament(tournamentId);
        }

        public async Task UnsubscribeByChannelAsync(ulong discordChannelId)
        {
            // Unsubscribe from the Pusher channel
            foreach (var subscription in _tournamentSubscriptionService.GetAllActiveSubscriptions().Where(n => n.DiscordChannelId == discordChannelId))
            {
                await _pusherService.UnsubscribeFromTournament(subscription.TournamentId);
            }

            // Remove the subscription from the Sqlite database
            _tournamentSubscriptionService.UnsubscribeChannelFromAllTournaments(discordChannelId);
        }

        public async Task ListenToAllActiveSubscriptions()
        {
            var activeSubscriptions = _tournamentSubscriptionService.GetAllActiveSubscriptions();

            foreach (var subscription in activeSubscriptions)
            {
                await _pusherService.SubscribeToTournament(subscription.TournamentId);
            }
        }

        public async Task<List<TournamentSubscription>> GetTournamentSubscriptionsAsync(long tournamentId)
        {
            return _tournamentSubscriptionService.GetSubscriptionsForTournament(tournamentId);
        }

    }
}
