﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MatchPlay.Discord.Subscriptions.Models
{
    public class TournamentSubscription
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public long TournamentId { get; set; }
        public ulong DiscordChannelId { get; set; }
        public bool IsSubscribed { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Created { get; set; }

    }
}
