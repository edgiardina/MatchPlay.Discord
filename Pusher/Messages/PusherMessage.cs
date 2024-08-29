using System.Text.Json;

namespace MatchPlay.Discord.Pusher.Messages
{
    public abstract class PusherMessage
    {
        public abstract string @event { get; }


        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
