#pragma once
#include "Game.h"
#include <memory>
#include <atomic>

class MatchAcceptService {
public:
    explicit MatchAcceptService(std::shared_ptr<Game> game);

    void RunAutoAcceptLoop(std::atomic<bool>& cancelToken);

private:
    std::shared_ptr<Game> _game;
};
