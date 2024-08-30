using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher
{
    public enum TournamentEvents
    {
        GameCreatedOrUpdated,
        GamesDeleted,
        SinglePlayerGameCreatedOrUpdated,
        SinglePlayerGamesDeleted,
        TournamentUpdated,
        RoundCreatedOrUpdated,
        RoundsDeleted,
        QueueChanged,
        PlayersAdded,
        ArenasAdded
    }
}
