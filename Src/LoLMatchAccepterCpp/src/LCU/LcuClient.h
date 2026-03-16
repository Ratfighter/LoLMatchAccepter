#pragma once
#include "MatchAcceptService.h"
#include "LeagueProcessDiscovery.h"
#include <memory>
#include <atomic>

class LcuClient {
public:
    LcuClient();
    ~LcuClient();

    bool IsClientFound() const;
    bool AutoAccept();

private:
    std::optional<LcuConnectionInfo> _connectionInfo;
    std::shared_ptr<LcuHttpClient> _client;
    std::shared_ptr<MatchAcceptService> _matchAcceptService;
    std::atomic<bool> _cancelToken;

    bool RunAutoAcceptAsync();
};
