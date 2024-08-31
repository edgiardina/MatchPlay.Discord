using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Subscriptions;

namespace MatchPlay.Discord.Discord
{
    [SlashCommandGroup("matchplay", "MatchPlay Tournament Subscriptions")]
    public class MatchPlaySlashCommand : ApplicationCommandModule
    {
        public MatchPlaySubscriptionService MatchPlaySubscriptionService { get; set; }

        [SlashCommand("subscribe", "Subscribe to a MatchPlay Tournament")]
        public async Task Subscribe(InteractionContext ctx, [Option("tournamentID", "The tournament ID")] long tournament)
        {
            await MatchPlaySubscriptionService.SubscribeAsync(tournament, ctx.Channel.Id);

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Subscribed to tournament {tournament}"));
        }

        [SlashCommand("unsubscribe", "Unsubscribe from a MatchPlay Tournament")]
        public async Task Unsubscribe(InteractionContext ctx, [Option("tournamentID", "The tournament ID")] long? tournament = null)
        {
            if (tournament != null)
            {
                await MatchPlaySubscriptionService.UnsubscribeAsync(tournament.Value, ctx.Channel.Id);
            }

            // else handle all tournaments
            
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Unsubscribed from tournament {tournament}"));
        }

    }
}
