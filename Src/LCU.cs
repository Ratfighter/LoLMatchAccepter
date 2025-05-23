using LoLMatchAccepterNet;
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
    public partial class LCU
    {
        public string? Port { get; init; } = null!;
        public string? Password { get; init; } = null!;

        [GeneratedRegex("--remoting-auth-token=([\\w-]*)")]
        private static partial Regex PasswordRegex();

        [GeneratedRegex("--app-port=([0-9]*)")]
        private static partial Regex PortRegex();

        private readonly HttpClient client;

        public LCU()
        {
            // Find League of Legends client process
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            client = new HttpClient(handler);

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

                    if (!string.IsNullOrEmpty(Port) && !string.IsNullOrEmpty(Password))
                    {
                        break;
                    }
                }
                catch
                {
                    // Continue to next process
                }
            }
        }

        private static string GetCommandLine(int processId)
        {
            using (var process = Process.GetProcessById(processId))
            {
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    using (var objects = searcher.Get())
                    {
                        foreach (var obj in objects)
                        {
                            return obj["CommandLine"]?.ToString() ?? string.Empty;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public async Task AutoAccept()
        {
            // Set up basic authentication
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{Password}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");
            
            string baseUrl = $"https://127.0.0.1:{Port}";

            var game = new Game(client, baseUrl);


            Console.WriteLine("Auto-accept running. Press ESC to exit.");
            Console.WriteLine("Waiting for match...");

            while (true)
            {
                // Check if ESC key is pressed to exit
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    break;

                bool inGame = await game.IsActive();

                if (inGame)
                {
                    // If we're in a game, don't check for match acceptance
                    Console.WriteLine("Active game detected. Waiting until game ends...");
                    await game.WaitUntilGameEnds();
                    Console.WriteLine("Game ended. Resuming auto-accept...");
                }

                try
                {
                    // Check if match is found
                    var matchResponse = await client.GetAsync($"{baseUrl}/lol-matchmaking/v1/search");

                    if (matchResponse.IsSuccessStatusCode)
                    {
                        string content = await matchResponse.Content.ReadAsStringAsync();
                        JsonDocument doc = JsonDocument.Parse(content);

                        if (doc.RootElement.TryGetProperty("searchState", out JsonElement searchState) &&
                            searchState.GetString() == "Found")
                        {
                            Console.WriteLine("Match found! Accepting...");

                            // Accept the match
                            var acceptResponse = await client.PostAsync(
                                $"{baseUrl}/lol-matchmaking/v1/ready-check/accept",
                                null
                            );

                            if (acceptResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Match accepted successfully!");

                                // Also check for ready-check status to confirm
                                var readyCheckResponse = await client.GetAsync($"{baseUrl}/lol-matchmaking/v1/ready-check");
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
                    // Wait a bit longer if there was an error
                    Thread.Sleep(3000);
                }

                Thread.Sleep(1000); // Check every second
            }

            Console.WriteLine("Auto-accepter stopped. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
