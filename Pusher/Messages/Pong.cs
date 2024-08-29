using System.Text.Json.Serialization;

namespace MatchPlay.Discord.Pusher.Messages
{
    public class Pong : PusherMessage
    {
        [JsonPropertyName("event")]
        public override string @event => "pusher:pong";

    }
}
