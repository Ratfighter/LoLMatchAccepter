using LoLMatchAccepterNet.Api;
using LoLMatchAccepterNet.LCU;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueMatchAccepter
{
    public partial class LcuClient : IDisposable
    {
        private string? Port { get; init; } = null!;
        private string? Password { get; init; } = null!;

        [GeneratedRegex("--remoting-auth-token=([\\w-]*)")]
        private static partial Regex PasswordRegex();

        [GeneratedRegex("--app-port=([0-9]*)")]
        private static partial Regex PortRegex();

        private readonly NotificatorServer _notificator;
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
                        _notificator = new();
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
            
            string baseUrl = $"https://127.0.0.1:{Port}";

            Game game = new(_client, baseUrl);

            Console.WriteLine("Auto-accept running. Press ESC to exit.");
            Console.WriteLine("Waiting for match...");

            while (true)
            {
                // Check if ESC key is pressed to exit
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    return true;
                }

                bool inGame = await game.IsActive();

                if (inGame)
                {
                    // If we're in a game, don't check for match acceptance
                    Console.WriteLine("Active game detected. Waiting until game ends...");
                    await _notificator.SendNotification();
                    await game.WaitUntilGameEnds();
                    Console.WriteLine("Game ended. Resuming auto-accept...");
                }

                try
                {
                    // Check if match is found
                    var matchResponse = await _client.GetAsync($"{baseUrl}/lol-matchmaking/v1/search");

                    if (matchResponse.IsSuccessStatusCode)
                    {
                        string content = await matchResponse.Content.ReadAsStringAsync();
                        JsonDocument doc = JsonDocument.Parse(content);

                        if (doc.RootElement.TryGetProperty("searchState", out JsonElement searchState) &&
                            searchState.GetString() == "Found")
                        {
                            Console.WriteLine("Match found! Accepting...");

                            // Accept the match
                            var acceptResponse = await _client.PostAsync(
                                $"{baseUrl}/lol-matchmaking/v1/ready-check/accept",
                                null
                            );

                            if (acceptResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Match accepted successfully!");

                                // Also check for ready-check status to confirm
                                var readyCheckResponse = await _client.GetAsync($"{baseUrl}/lol-matchmaking/v1/ready-check");
                                if (readyCheckResponse.IsSuccessStatusCode)
                                {
                                    string readyCheckContent = await readyCheckResponse.Content.ReadAsStringAsync();
                                    JsonDocument readyCheckDoc = JsonDocument.Parse(readyCheckContent);

                                    if (readyCheckDoc.RootElement.TryGetProperty("state", out JsonElement state))
                                    {
                                        Console.WriteLine($"Ready check state: {state.GetString()}");
                                    }
                                }

                                Thread.Sleep(5000); // Wait 5 seconds before checking again
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error communicating with League client: {ex.Message}");
                    Thread.Sleep(3000);
                    return false;
                }

                Thread.Sleep(1000); // Check every second
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client.Dispose();
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
