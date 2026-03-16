#pragma once
#include "LcuConnectionInfo.h"
#include <string>
#include <memory>
#include <windows.h>
#include <winhttp.h>

struct HttpResponse {
    int statusCode = 0;
    std::string body;

    bool IsSuccess() const {
        return statusCode >= 200 && statusCode < 300;
    }
};

class LcuHttpClient {
public:
    explicit LcuHttpClient(const LcuConnectionInfo& connectionInfo);
    ~LcuHttpClient();

    HttpResponse Get(const std::string& endpoint);
    HttpResponse Post(const std::string& endpoint);

private:
    std::string _baseUrl;
    std::string _authHeader;
    int _port;

    std::string EncodeBase64(const std::string& input);
    HttpResponse PerformRequest(const std::wstring& method, const std::string& endpoint, const std::string& body = "");
};
