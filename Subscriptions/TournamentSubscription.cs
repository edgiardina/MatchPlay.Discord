using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MatchPlay.Discord.Subscriptions
{
    public class TournamentSubscription
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int TournamentId { get; set; }
        public ulong DiscordChannelId { get; set; }
        public bool IsSubscribed { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Created { get; set; }

    }
}
