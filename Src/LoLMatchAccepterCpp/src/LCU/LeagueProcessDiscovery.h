#pragma once
#include "LcuConnectionInfo.h"
#include <optional>

class LeagueProcessDiscovery {
public:
    std::optional<LcuConnectionInfo> DiscoverLeagueClient();

private:
    std::string GetProcessCommandLine(int processId);
};
