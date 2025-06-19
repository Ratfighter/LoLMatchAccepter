using LoLMatchAccepterNet.LCU;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace LeagueMatchAccepter
{
    public partial class LcuClient : IDisposable
    {
        public event EventHandler? MatchFound;
        private string? Port { get; init; } = null!;
        private string? Password { get; init; } = null!;

        [GeneratedRegex("--remoting-auth-token=([\\w-]*)")]
        private static partial Regex PasswordRegex();

        [GeneratedRegex("--app-port=([0-9]*)")]
        private static partial Regex PortRegex();

        private readonly HttpClient _client;
        private bool disposedValue;

        public LcuClient()
        {
            // Find League of Legends client process
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _client = new HttpClient(handler);

            if (processes.Length == 0)
            {
                return;
            }

            foreach (var process in processes)
            {
                try
                {
                    string commandLine = GetCommandLine(process.Id);

                    // Extract port
                    var portMatch = PortRegex().Match(commandLine);
                    if (portMatch.Success)
                    {
                        Port = portMatch.Groups[1].Value;
                    }

                    // Extract password
                    var passwordMatch = PasswordRegex().Match(commandLine);
                    if (passwordMatch.Success)
                    {
                        Password = passwordMatch.Groups[1].Value;
                    }

                    if (IsClientFound())
                    {
                        break;
                    }
                    Console.WriteLine($"League client found! Connected to port: {Port}");
                }
                catch
                {
                    // Continue to next process
                }
            }
        }

        public bool IsClientFound()
        {
            return !string.IsNullOrEmpty(Port) && !string.IsNullOrEmpty(Password);
        }

        private static string GetCommandLine(int processId)
        {
            using var process = Process.GetProcessById(processId);
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            using var objects = searcher.Get();
            var enumerator = objects.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current["CommandLine"]?.ToString() ?? string.Empty;
        }

        public async Task<bool> AutoAccept()
        {
            // Set up basic authentication
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{Password}"));
            _client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

            int port = int.Parse(Port!);

            Game game = new(_client, port);

            Console.WriteLine("Auto-accept running. Press ESC to exit.");
            Console.WriteLine("Waiting for match...");

            while (true)
            {
                // Check if ESC key is pressed to exit
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    return true;
                }

                try
                {
                    bool isMatchAccepted = await game.WaitForQueue();

                    if (isMatchAccepted)
                    {
                        string currentPhase = await game.WaitUntilPhaseEnds(Game.ReadyCheck);

                        if (currentPhase == Game.ChampSelect)
                        {
                            Console.WriteLine("Currently in champ select...");
                            MatchFound?.Invoke(null, EventArgs.Empty);
                            await game.WaitUntilPhaseEnds(Game.ChampSelect);
                        }
                        else if (currentPhase == Game.LoadingScreen || currentPhase == Game.InProgress) //Only works for gamemodes that instantly get you in-game (e.g.: Swift, Brawl)
                        {
                            MatchFound?.Invoke(null, EventArgs.Empty);
                        }
                    }

                    bool inGame = await game.IsActive();

                    if (inGame)
                    {
                        // If we're in a game, don't check for match acceptance
                        Console.WriteLine("Active game detected. Waiting until game ends...");
                        await game.WaitUntilGameEnds();
                        Console.WriteLine("Game ended. Resuming auto-accept...");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error communicating with League client.");
                    await Task.Delay(6000);
                    return false;
                }

                await Task.Delay(500);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
                    MatchFound = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
