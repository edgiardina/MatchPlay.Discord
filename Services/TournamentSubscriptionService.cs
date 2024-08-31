using MatchPlay.Discord.Subscriptions;
using SQLite;

namespace MatchPlay.Discord.Services
{
    public class TournamentSubscriptionService
    {

        private string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatchPlay.db3");
        public TournamentSubscriptionService()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<TournamentSubscription>();
            }            
        }

        public void SubscribeToTournament(long tournamentId, ulong discordChannelId)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                var existingSubscription = db.Table<TournamentSubscription>().Where(x => x.TournamentId == tournamentId && x.DiscordChannelId == discordChannelId).FirstOrDefault();
                if (existingSubscription == null)
                {
                    db.Insert(new TournamentSubscription
                    {
                        TournamentId = tournamentId,
                        DiscordChannelId = discordChannelId,
                        IsSubscribed = true,
                        LastUpdated = DateTime.Now,
                        Created = DateTime.Now
                    });
                }
                else
                {
                    existingSubscription.IsSubscribed = true;
                    existingSubscription.LastUpdated = DateTime.Now;
                    db.Update(existingSubscription);
                }
            }
        }

        public void UnsubscribeFromTournament(long tournamentId, ulong discordChannelId)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                var existingSubscription = db.Table<TournamentSubscription>().Where(x => x.TournamentId == tournamentId && x.DiscordChannelId == discordChannelId).FirstOrDefault();
                if (existingSubscription != null)
                {
                    existingSubscription.IsSubscribed = false;
                    existingSubscription.LastUpdated = DateTime.Now;
                    db.Update(existingSubscription);
                }
            }
        }

        public List<TournamentSubscription> GetSubscriptionsForTournament(long tournamentId)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                return db.Table<TournamentSubscription>().Where(x => x.TournamentId == tournamentId && x.IsSubscribed).ToList();
            }
        }

        public List<TournamentSubscription> GetAllActiveSubscriptions()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                return db.Table<TournamentSubscription>().Where(x => x.IsSubscribed).ToList();
            }
        }
    }
}
