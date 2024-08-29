using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher.Messages
{
    public class Subscribe : PusherMessage
    {
        public override string @event => "pusher:subscribe";
        public required Channel data { get; set; }
    }
}
