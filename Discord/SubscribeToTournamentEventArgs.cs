using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Discord
{
    public class SubscribeToTournamentEventArgs : EventArgs
    {
        public int TournamentId { get; set; }

        public SubscribeToTournamentEventArgs(int tournamentId)
        {
            TournamentId = tournamentId;
        }
    }
}
