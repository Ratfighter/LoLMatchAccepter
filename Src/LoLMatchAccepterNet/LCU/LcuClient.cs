using LoLMatchAccepterNet.LCU;

namespace LeagueMatchAccepter
{
    public class LcuClient : IDisposable
    {
        private readonly LcuConnectionInfo? _connectionInfo;
        private readonly HttpClient? _client;
        private readonly MatchAcceptService? _matchAcceptService;
        private CancellationTokenSource? _cts;
        private bool disposedValue;

        public LcuClient()
        {
            var processDiscovery = new LeagueProcessDiscovery();
            _connectionInfo = processDiscovery.DiscoverLeagueClient();

            if (_connectionInfo?.IsValid == true)
            {
                var clientFactory = new LcuHttpClientFactory();
                _client = clientFactory.CreateClient(_connectionInfo);

                int port = int.Parse(_connectionInfo.Port);
                var game = new Game(_client, port);
                _matchAcceptService = new MatchAcceptService(game);
            }
        }

        public bool IsClientFound()
        {
            return _connectionInfo?.IsValid == true;
        }

        public async Task<bool> AutoAccept()
        {
            if (_matchAcceptService == null)
            {
                return false;
            }

            Console.WriteLine("Auto-accept running. Press ESC to exit.");
            Console.WriteLine("Waiting for match...");

            _cts = new CancellationTokenSource();
            var backgroundTask = Task.Run(() => RunAutoAcceptAsync(_cts.Token));

            while (!backgroundTask.IsCompleted)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    _cts.Cancel();
                    break;
                }
                await Task.Delay(1000);
            }

            try
            {
                return await backgroundTask;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
        }

        private async Task<bool> RunAutoAcceptAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _matchAcceptService!.RunAutoAcceptLoopAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _client?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
