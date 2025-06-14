﻿using LoLMatchAccepterNet.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoLMatchAccepterNet.LCU
{
    public class Game(HttpClient client, string baseUrl)
    {
        public const string InProgress = "InProgress";
        public const string GameStart = "GameStart";
        public const string LoadingScreen = "LoadingScreen";
        public const string None = "None";
        public const string Lobby = "Lobby";
        public const string Matchmaking = "Matchmaking";
        public const string ChampSelect = "ChampSelect";

        private readonly HttpClient _client = client;
        private readonly string _baseUrl = baseUrl;

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
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    return;

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
                }

                if (!gameEnded)
                {
                    // Wait for 30 seconds before checking again to reduce API calls during a game
                    for (int i = 0; i < 15; i++)
                    {
                        // Check for ESC key every 2 seconds
                        if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                            return;
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        public async Task WaitUntilPhaseEnds(params string[] gamePhases)
        {
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    return;

                try
                {
                    string currentPhase = await GetGamePhase();
                    if (!gamePhases.Contains(currentPhase))
                    {
                        return;
                    }
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking game phase: {ex.Message}");
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
    }
}
