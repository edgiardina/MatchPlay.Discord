using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher.Messages
{
    public class TournamentEventMessage 
    {
        public string @event { get; set; }
        public object data { get; set; }
    }
}
