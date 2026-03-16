#include "LcuHttpClient.h"
#include <iostream>
#include <sstream>

LcuHttpClient::LcuHttpClient(const LcuConnectionInfo& connectionInfo) 
    : _port(std::stoi(connectionInfo.Port)) {
    _baseUrl = "https://127.0.0.1:" + connectionInfo.Port;
    
    std::string credentials = "riot:" + connectionInfo.Password;
    _authHeader = "Basic " + EncodeBase64(credentials);
}

LcuHttpClient::~LcuHttpClient() {
}

std::string LcuHttpClient::EncodeBase64(const std::string& input) {
    static const char* base64_chars = 
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        "abcdefghijklmnopqrstuvwxyz"
        "0123456789+/";

    std::string ret;
    int i = 0;
    int j = 0;
    unsigned char char_array_3[3];
    unsigned char char_array_4[4];
    size_t in_len = input.length();
    const unsigned char* bytes_to_encode = reinterpret_cast<const unsigned char*>(input.c_str());

    while (in_len--) {
        char_array_3[i++] = *(bytes_to_encode++);
        if (i == 3) {
            char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
            char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
            char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
            char_array_4[3] = char_array_3[2] & 0x3f;

            for(i = 0; i < 4; i++)
                ret += base64_chars[char_array_4[i]];
            i = 0;
        }
    }

    if (i) {
        for(j = i; j < 3; j++)
            char_array_3[j] = '\0';

        char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
        char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
        char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);

        for (j = 0; j < i + 1; j++)
            ret += base64_chars[char_array_4[j]];

        while((i++ < 3))
            ret += '=';
    }

    return ret;
}

HttpResponse LcuHttpClient::Get(const std::string& endpoint) {
    return PerformRequest(L"GET", endpoint);
}

HttpResponse LcuHttpClient::Post(const std::string& endpoint) {
    return PerformRequest(L"POST", endpoint, "");
}

HttpResponse LcuHttpClient::PerformRequest(const std::wstring& method, const std::string& endpoint, const std::string& body) {
    HttpResponse result;

    HINTERNET hSession = WinHttpOpen(
        L"LoL Match Accepter/1.0",
        WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
        WINHTTP_NO_PROXY_NAME,
        WINHTTP_NO_PROXY_BYPASS, 0);

    if (!hSession) {
        return result;
    }

    HINTERNET hConnect = WinHttpConnect(
        hSession,
        L"127.0.0.1",
        static_cast<INTERNET_PORT>(_port),
        0);

    if (!hConnect) {
        WinHttpCloseHandle(hSession);
        return result;
    }

    std::wstring wEndpoint(endpoint.begin(), endpoint.end());
    HINTERNET hRequest = WinHttpOpenRequest(
        hConnect,
        method.c_str(),
        wEndpoint.c_str(),
        NULL,
        WINHTTP_NO_REFERER,
        WINHTTP_DEFAULT_ACCEPT_TYPES,
        WINHTTP_FLAG_SECURE);

    if (!hRequest) {
        WinHttpCloseHandle(hConnect);
        WinHttpCloseHandle(hSession);
        return result;
    }

    DWORD dwFlags = SECURITY_FLAG_IGNORE_UNKNOWN_CA |
                    SECURITY_FLAG_IGNORE_CERT_DATE_INVALID |
                    SECURITY_FLAG_IGNORE_CERT_CN_INVALID |
                    SECURITY_FLAG_IGNORE_CERT_WRONG_USAGE;

    WinHttpSetOption(hRequest, WINHTTP_OPTION_SECURITY_FLAGS, &dwFlags, sizeof(dwFlags));

    std::wstring wAuthHeader(_authHeader.begin(), _authHeader.end());
    std::wstring authHeaderName = L"Authorization: ";
    std::wstring fullAuthHeader = authHeaderName + wAuthHeader;

    WinHttpAddRequestHeaders(
        hRequest,
        fullAuthHeader.c_str(),
        (DWORD)-1L,
        WINHTTP_ADDREQ_FLAG_ADD);

    BOOL bResults = WinHttpSendRequest(
        hRequest,
        WINHTTP_NO_ADDITIONAL_HEADERS,
        0,
        body.empty() ? WINHTTP_NO_REQUEST_DATA : (LPVOID)body.c_str(),
        static_cast<DWORD>(body.length()),
        static_cast<DWORD>(body.length()),
        0);

    if (bResults) {
        bResults = WinHttpReceiveResponse(hRequest, NULL);
    }

    if (bResults) {
        // Get status code
        DWORD dwStatusCode = 0;
        DWORD dwSize = sizeof(dwStatusCode);
        if (WinHttpQueryHeaders(hRequest,
            WINHTTP_QUERY_STATUS_CODE | WINHTTP_QUERY_FLAG_NUMBER,
            WINHTTP_HEADER_NAME_BY_INDEX,
            &dwStatusCode,
            &dwSize,
            WINHTTP_NO_HEADER_INDEX)) {
            result.statusCode = static_cast<int>(dwStatusCode);
        }

        // Read response body
        dwSize = 0;
        do {
            dwSize = 0;
            if (!WinHttpQueryDataAvailable(hRequest, &dwSize)) {
                break;
            }

            if (dwSize == 0) {
                break;
            }

            char* pszOutBuffer = new char[dwSize + 1];
            if (!pszOutBuffer) {
                break;
            }

            ZeroMemory(pszOutBuffer, dwSize + 1);

            DWORD dwDownloaded = 0;
            if (WinHttpReadData(hRequest, (LPVOID)pszOutBuffer, dwSize, &dwDownloaded)) {
                result.body.append(pszOutBuffer, dwDownloaded);
            }

            delete[] pszOutBuffer;
        } while (dwSize > 0);
    }

    WinHttpCloseHandle(hRequest);
    WinHttpCloseHandle(hConnect);
    WinHttpCloseHandle(hSession);

    return result;
}
