#include "Game.h"
#include "../json.hpp"
#include <iostream>
#include <thread>
#include <chrono>
#include <vector>
#include <algorithm>

using json = nlohmann::json;

Game::Game(std::shared_ptr<LcuHttpClient> client) : _client(client) {
}

bool Game::IsActive() {
    try {
        std::string gamePhase = GetGamePhase();

        if (gamePhase == InProgress || gamePhase == GameStart || gamePhase == LoadingScreen) {
            return true;
        }

        HttpResponse spectatorResponse = _client->Get("/lol-spectator/v1/spectate/active-games/for-summoner/0");
        if (spectatorResponse.IsSuccess()) {
            return true;
        }
    }
    catch (...) {
    }

    return false;
}

void Game::WaitUntilGameEnds() {
    bool gameEnded = false;

    while (!gameEnded) {
        try {
            std::string gamePhase = GetGamePhase();

            if (gamePhase.empty()) {
                return;
            }

            if (gamePhase == None || gamePhase == Lobby || 
                gamePhase == Matchmaking || gamePhase == EndOfGame) {
                gameEnded = true;
            }
        }
        catch (const std::exception& ex) {
            std::cout << "Error checking if game ended: " << ex.what() << std::endl;
            return;
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(1000));
    }
}

std::string Game::WaitUntilPhaseEnds(const std::vector<std::string>& gamePhases) {
    while (true) {
        try {
            std::string currentPhase = GetGamePhase();
            
            if (std::find(gamePhases.begin(), gamePhases.end(), currentPhase) == gamePhases.end()) {
                return currentPhase;
            }
            
            std::this_thread::sleep_for(std::chrono::milliseconds(1000));
        }
        catch (const std::exception& ex) {
            std::cout << "Error checking game phase: " << ex.what() << std::endl;
            return "";
        }
    }
}

std::string Game::GetGamePhase() {
    HttpResponse response = _client->Get("/lol-gameflow/v1/session");

    if (!response.IsSuccess() || response.body.empty()) {
        return "";
    }

    try {
        auto jsonResponse = json::parse(response.body);
        if (jsonResponse.contains("phase")) {
            return jsonResponse["phase"].get<std::string>();
        }
    }
    catch (...) {
    }

    return "";
}

bool Game::WaitForQueue() {
    HttpResponse matchResponse = _client->Get("/lol-matchmaking/v1/search");

    if (matchResponse.IsSuccess() && !matchResponse.body.empty()) {
        try {
            auto jsonResponse = json::parse(matchResponse.body);
            if (jsonResponse.contains("searchState")) {
                std::string searchState = jsonResponse["searchState"].get<std::string>();
                if (searchState == "Found") {
                    std::cout << "Match found! Accepting..." << std::endl;

                    HttpResponse acceptResult = _client->Post("/lol-matchmaking/v1/ready-check/accept");

                    if (acceptResult.IsSuccess()) {
                        std::cout << "Match accepted successfully!" << std::endl;
                        return true;
                    }
                }
            }
        }
        catch (...) {
        }
    }

    std::string gamePhase = GetGamePhase();
    return gamePhase == ChampSelect || gamePhase == InProgress;
}

void Game::NavigateToLobby() {
    try {
        std::cout << "Navigating to lobby..." << std::endl;

        WaitUntilPhaseEnds({WaitingForStats});

        HttpResponse playAgainResult = _client->Post("/lol-lobby/v2/play-again");

        if (playAgainResult.IsSuccess()) {
            std::cout << "Successfully navigated to lobby via play-again" << std::endl;
            return;
        }
    }
    catch (const std::exception& ex) {
        std::cout << "Error navigating to lobby: " << ex.what() << std::endl;
    }
}
