#include "LcuClient.h"
#include <iostream>
#include <thread>
#include <conio.h>

LcuClient::LcuClient() : _cancelToken(false) {
    LeagueProcessDiscovery processDiscovery;
    _connectionInfo = processDiscovery.DiscoverLeagueClient();

    if (_connectionInfo.has_value() && _connectionInfo->IsValid()) {
        _client = std::make_shared<LcuHttpClient>(_connectionInfo.value());
        auto game = std::make_shared<Game>(_client);
        _matchAcceptService = std::make_shared<MatchAcceptService>(game);
    }
}

LcuClient::~LcuClient() {
    _cancelToken.store(true);
}

bool LcuClient::IsClientFound() const {
    return _connectionInfo.has_value() && _connectionInfo->IsValid();
}

bool LcuClient::AutoAccept() {
    if (!_matchAcceptService) {
        return false;
    }

    std::cout << "Auto-accept running. Press ESC to exit." << std::endl;
    std::cout << "Waiting for match..." << std::endl;

    _cancelToken.store(false);
    std::thread backgroundThread([this]() {
        this->RunAutoAcceptAsync();
    });

    while (backgroundThread.joinable()) {
        if (_kbhit()) {
            int ch = _getch();
            if (ch == 27) { // ESC key
                _cancelToken.store(true);
                break;
            }
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }

    if (backgroundThread.joinable()) {
        backgroundThread.join();
    }

    return true;
}

bool LcuClient::RunAutoAcceptAsync() {
    try {
        _matchAcceptService->RunAutoAcceptLoop(_cancelToken);
        return true;
    }
    catch (...) {
        return false;
    }
}
