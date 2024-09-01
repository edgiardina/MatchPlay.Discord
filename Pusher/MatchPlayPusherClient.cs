using MatchPlay.Discord.Pusher.Messages;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Text.Json;
using Websocket.Client;

namespace MatchPlay.Discord.Pusher
{
    public class MatchPlayPusherClient
    {
        private static Uri url = new Uri("wss://ws.app.matchplay.events/app/tnrxzkahdeullnwje83e?protocol=7&client=js&version=8.4.0-rc2&flash=false");
        ILogger _logger;

        private WebsocketClient client = new WebsocketClient(url);

        public event EventHandler<TournamentEventEventArgs> TournamentEventReceived;

        public MatchPlayPusherClient(ILogger<MatchPlayPusherClient> logger)
        {
            _logger = logger;

        }

        public async Task Connect()
        {
            client.ReconnectTimeout = null;
            client.ReconnectionHappened.Subscribe(info =>
                _logger.LogDebug($"Reconnection happened, type: {info.Type}"));

            client.MessageReceived.Subscribe(msg => _logger.LogDebug($"Message received: {msg}"));

            client.MessageReceived
                  .Where(msg => msg.Text == new Ping().ToJson())
                  .Subscribe(pong =>
                  {
                      _logger.LogDebug("Sending: Pong");
                      client.Send(new Pong().ToJson());
                  });

            client.MessageReceived
                .Where(msg => msg.Text.Contains("event"))
                .Subscribe(msg =>
                {
                    var message = JsonSerializer.Deserialize<TournamentEventMessage>(msg.Text);
                    var eventType = message?.@event.Split("\\").LastOrDefault();

                    if (message != null && eventType != null && Enum.IsDefined(typeof(TournamentEvents), eventType))
                    {
                        // if message.@event ends with a TournamentEvents enum value, set tournamentEvent to that value
                        var tournamentEvent = (TournamentEvents)Enum.Parse(typeof(TournamentEvents), eventType);                

                        TournamentEventReceived?.Invoke(this, new TournamentEventEventArgs(tournamentEvent, message.data));
                    }
                });


            await client.Start();
        }

        public async Task Disconnect()
        {
            client.Dispose();
        }

        public async Task SubscribeToTournament(long tournamentId)
        {
            var subscribe = new Subscribe { data = new Channel { channel = $"tournaments.{tournamentId}" } };

            client.Send(JsonSerializer.Serialize(subscribe));
        }

        public async Task UnsubscribeFromTournament(long tournamentId)
        {
            var unsubscribe = new Unsubscribe { data = new Channel { channel = $"tournaments.{tournamentId}" } };

            client.Send(JsonSerializer.Serialize(unsubscribe));
        }
    }
}
