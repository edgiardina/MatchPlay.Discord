using System.Text.Json.Serialization;

namespace MatchPlay.Discord.Pusher.Messages
{
    public class Ping : PusherMessage
    {
        [JsonPropertyName("event")]
        public override string @event => "pusher:ping";
    }
}
