using DSharpPlus.SlashCommands;
using MatchPlay.Discord.Pusher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Discord
{
    [SlashCommandGroup("matchplay", "MatchPlay Tournament Subscriptions")]
    public class MatchPlaySlashCommand : ApplicationCommandModule
    {
        public MatchPlayPusherClient matchPlayPusherClient;

        [SlashCommand("subscribe", "Subscribe to a MatchPlay Tournament")]
        public async Task Subscribe(InteractionContext ctx, [Option("tournament", "The tournament ID")] int tournament)
        {
            await matchPlayPusherClient.SubscribeToTournament(tournament);

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Subscribed to tournament {tournament}"));
        }

        [SlashCommand("unsubscribe", "Unsubscribe from a MatchPlay Tournament")]
        public async Task Unsubscribe(InteractionContext ctx, [Option("tournament", "The tournament ID")] int? tournament = null)
        {
            //await matchPlayPusherClient.UnsubscribeFromTournament(tournament.Value);
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent($"Unsubscribed from tournament {tournament}"));
        }

    }
}
