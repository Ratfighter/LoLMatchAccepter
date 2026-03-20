#pragma once
#include "LcuHttpClient.h"
#include <string>
#include <memory>
#include <vector>

class Game {
public:
    static constexpr const char* InProgress = "InProgress";
    static constexpr const char* GameStart = "GameStart";
    static constexpr const char* LoadingScreen = "LoadingScreen";
    static constexpr const char* None = "None";
    static constexpr const char* Lobby = "Lobby";
    static constexpr const char* ReadyCheck = "ReadyCheck";
    static constexpr const char* Matchmaking = "Matchmaking";
    static constexpr const char* ChampSelect = "ChampSelect";
    static constexpr const char* EndOfGame = "EndOfGame";
    static constexpr const char* WaitingForStats = "WaitingForStats";

    Game(std::shared_ptr<LcuHttpClient> client);

    bool IsActive();
    void WaitUntilGameEnds();
    std::string WaitUntilPhaseEnds(const std::vector<std::string>& gamePhases);
    std::string GetGamePhase();
    void StartQueue();
    bool WaitForQueue();
    void NavigateToLobby();

private:
    std::shared_ptr<LcuHttpClient> _client;
};
