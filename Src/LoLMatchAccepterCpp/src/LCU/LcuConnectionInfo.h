#pragma once
#include <string>

class LcuConnectionInfo {
public:
    std::string Port;
    std::string Password;

    bool IsValid() const {
        return !Port.empty() && !Password.empty();
    }
};
