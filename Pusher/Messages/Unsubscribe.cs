using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher.Messages
{
    public class Unsubscribe : PusherMessage
    {
        public override string @event => "pusher:unsubscribe";
        public required Channel data { get; set; }
    }
}
