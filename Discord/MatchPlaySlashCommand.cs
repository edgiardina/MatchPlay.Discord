using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Pusher;

namespace MatchPlay.Discord.Discord
{
    [SlashCommandGroup("matchplay", "MatchPlay Tournament Subscriptions")]
    public class MatchPlaySlashCommand : ApplicationCommandModule
    {
        public event EventHandler<SubscribeToTournamentEventArgs> SubscribeToTournament;
        public event EventHandler<SubscribeToTournamentEventArgs> UnsubscribeFromTournament;

        [SlashCommand("subscribe", "Subscribe to a MatchPlay Tournament")]
        public async Task Subscribe(InteractionContext ctx, [Option("tournament", "The tournament ID")] int tournament)
        {
            SubscribeToTournament?.Invoke(this, new SubscribeToTournamentEventArgs(tournament));

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Subscribed to tournament {tournament}"));
        }

        [SlashCommand("unsubscribe", "Unsubscribe from a MatchPlay Tournament")]
        public async Task Unsubscribe(InteractionContext ctx, [Option("tournament", "The tournament ID")] int? tournament = null)
        {
            if (tournament != null)
            {
                UnsubscribeFromTournament?.Invoke(this, new SubscribeToTournamentEventArgs(tournament.Value)); 
            }

            // else handle all tournaments
            
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Unsubscribed from tournament {tournament}"));
        }

    }
}
