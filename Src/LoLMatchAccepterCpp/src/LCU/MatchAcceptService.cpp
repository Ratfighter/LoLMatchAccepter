#include "MatchAcceptService.h"
#include <iostream>
#include <thread>
#include <chrono>

MatchAcceptService::MatchAcceptService(std::shared_ptr<Game> game) : _game(game) {
}

void MatchAcceptService::RunAutoAcceptLoop(std::atomic<bool>& cancelToken) {
    while (!cancelToken.load()) {
        try {
            bool isMatchAccepted = _game->WaitForQueue();

            if (isMatchAccepted) {
                std::string currentPhase = _game->WaitUntilPhaseEnds({Game::ReadyCheck});

                if (currentPhase == Game::ChampSelect) {
                    std::cout << "Currently in champ select..." << std::endl;
                    _game->WaitUntilPhaseEnds({Game::ChampSelect});
                }
            }

            bool inGame = _game->IsActive();

            if (inGame) {
                std::cout << "Active game detected. Waiting until game ends..." << std::endl;
                _game->WaitUntilGameEnds();
                std::cout << "Game ended. Navigating to lobby..." << std::endl;
                _game->NavigateToLobby();
                std::cout << "Resuming auto-accept..." << std::endl;
            }
        }
        catch (const std::exception&) {
            std::cout << "Error communicating with League client." << std::endl;
            std::this_thread::sleep_for(std::chrono::milliseconds(6000));
            throw;
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(500));
    }
}
