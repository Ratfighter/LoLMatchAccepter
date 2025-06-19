using System.Text.Json;

namespace LoLMatchAccepterNet.LCU
{
    public class Game
    {
        public const string InProgress = "InProgress";
        public const string GameStart = "GameStart";
        public const string LoadingScreen = "LoadingScreen";
        public const string None = "None";
        public const string Lobby = "Lobby";
        public const string ReadyCheck = "ReadyCheck";
        public const string Matchmaking = "Matchmaking";
        public const string ChampSelect = "ChampSelect";

        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public Game(HttpClient client, int port)
        {
            _client = client;
            this._baseUrl = $"https://127.0.0.1:{port}";
        }

        public async Task<bool> IsActive()
        {
            try
            {
                string gamePhase = await GetGamePhase();

                if (gamePhase == InProgress ||
                    gamePhase == GameStart || gamePhase == LoadingScreen)
                {
                    return true;
                }

                var spectatorResponse = await _client.GetAsync($"{_baseUrl}/lol-spectator/v1/spectate/active-games/for-summoner/0");
                if (spectatorResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Status 200 means there's an active game
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking game status: {ex.Message}");
            }

            return false;
        }

        public async Task WaitUntilGameEnds()
        {
            bool gameEnded = false;

            while (!gameEnded)
            {
                try
                {

                    string gamePhase = await GetGamePhase();

                    //Game not found
                    if (string.IsNullOrEmpty(gamePhase))
                    {
                        return;
                    }

                    if (gamePhase == "None" || gamePhase == "Lobby" || gamePhase == "Matchmaking")
                    {
                        gameEnded = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking if game ended: {ex.Message}");
                    return;
                }

                await Task.Delay(1000);
            }
        }

        public async Task<string> WaitUntilPhaseEnds(params string[] gamePhases)
        {
            while (true)
            {
                try
                {
                    string currentPhase = await GetGamePhase();
                    if (!gamePhases.Contains(currentPhase))
                    {
                        return currentPhase;
                    }
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking game phase: {ex.Message}");
                    return string.Empty;
                }
            }
        }

        public async Task<string> GetGamePhase()
        {
            var gameSessionResponse = await _client.GetAsync($"{_baseUrl}/lol-gameflow/v1/session");
            if (gameSessionResponse.IsSuccessStatusCode)
            {
                string content = await gameSessionResponse.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("phase", out JsonElement phase))
                {
                    string gamePhase = phase.GetString()!;

                    return gamePhase;
                }
            }
            return string.Empty;
        }

        public async Task<bool> WaitForQueue()
        {
            var matchResponse = await _client.GetAsync($"{_baseUrl}/lol-matchmaking/v1/search");
            if (matchResponse.IsSuccessStatusCode)
            {
                string content = await matchResponse.Content.ReadAsStringAsync();
                JsonDocument doc = JsonDocument.Parse(content);

                doc.RootElement.TryGetProperty("searchState", out JsonElement searchState);
                if (searchState.GetString() == "Found")
                {
                    Console.WriteLine("Match found! Accepting...");
                    var acceptResponse = await _client.PostAsync(
                        $"{_baseUrl}/lol-matchmaking/v1/ready-check/accept",
                        null
                    );

                    if (acceptResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Match accepted successfully!");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
