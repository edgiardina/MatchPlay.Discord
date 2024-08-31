using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MatchPlay.Discord.Pusher.Data
{
    public class RoundCreatedOrUpdated
    {
        [JsonPropertyName("roundId")]
        public long RoundId { get; set; }

        [JsonPropertyName("tournamentId")]
        public long TournamentId { get; set; }

        [JsonPropertyName("arenaId")]
        public long? ArenaId { get; set; }

        [JsonPropertyName("score")]
        public long? Score { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("duration")]
        public object Duration { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public DateTime? CompletedAt { get; set; }
    }
}
