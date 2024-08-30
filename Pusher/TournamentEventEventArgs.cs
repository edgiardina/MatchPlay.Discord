using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher
{
    public class TournamentEventEventArgs : EventArgs
    {
        public TournamentEvents TournamentEvent { get; set; }
        public object Data { get; set; }

        public TournamentEventEventArgs(TournamentEvents tournamentEvent, object data)
        {
            TournamentEvent = tournamentEvent;
            Data = data;
        }
    }
}
