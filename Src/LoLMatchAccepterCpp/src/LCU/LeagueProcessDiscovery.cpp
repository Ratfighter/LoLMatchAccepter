#include "LeagueProcessDiscovery.h"
#include <windows.h>
#include <tlhelp32.h>
#include <comdef.h>
#include <Wbemidl.h>
#include <iostream>
#include <regex>
#include <codecvt>
#include <locale>

#pragma comment(lib, "wbemuuid.lib")

// Helper function to convert wide string to narrow string
static std::string WStringToString(const std::wstring& wstr) {
    if (wstr.empty()) return std::string();
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
    std::string strTo(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
    return strTo;
}

std::optional<LcuConnectionInfo> LeagueProcessDiscovery::DiscoverLeagueClient() {
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) {
        return std::nullopt;
    }

    PROCESSENTRY32W pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32W);

    if (!Process32FirstW(hSnapshot, &pe32)) {
        CloseHandle(hSnapshot);
        return std::nullopt;
    }

    do {
        std::wstring processName = pe32.szExeFile;
        if (processName == L"LeagueClientUx.exe") {
            try {
                std::string commandLine = GetProcessCommandLine(pe32.th32ProcessID);

                std::regex portRegex(R"(--app-port=([0-9]*))");
                std::regex passwordRegex(R"(--remoting-auth-token=([\w-]*))");

                std::smatch portMatch, passwordMatch;

                if (std::regex_search(commandLine, portMatch, portRegex) &&
                    std::regex_search(commandLine, passwordMatch, passwordRegex)) {
                    
                    LcuConnectionInfo connectionInfo;
                    connectionInfo.Port = portMatch[1].str();
                    connectionInfo.Password = passwordMatch[1].str();

                    std::cout << "League client found! Connected to port: " << connectionInfo.Port << std::endl;
                    CloseHandle(hSnapshot);
                    return connectionInfo;
                }
            }
            catch (...) {
            }
        }
    } while (Process32NextW(hSnapshot, &pe32));

    CloseHandle(hSnapshot);
    return std::nullopt;
}

std::string LeagueProcessDiscovery::GetProcessCommandLine(int processId) {
    HRESULT hres;
    
    hres = CoInitializeEx(0, COINIT_MULTITHREADED);
    if (FAILED(hres)) {
        return "";
    }

    hres = CoInitializeSecurity(
        NULL, -1, NULL, NULL,
        RPC_C_AUTHN_LEVEL_DEFAULT,
        RPC_C_IMP_LEVEL_IMPERSONATE,
        NULL, EOAC_NONE, NULL
    );

    IWbemLocator* pLoc = NULL;
    hres = CoCreateInstance(
        CLSID_WbemLocator, 0,
        CLSCTX_INPROC_SERVER,
        IID_IWbemLocator, (LPVOID*)&pLoc);

    if (FAILED(hres)) {
        CoUninitialize();
        return "";
    }

    IWbemServices* pSvc = NULL;
    hres = pLoc->ConnectServer(
        _bstr_t(L"ROOT\\CIMV2"),
        NULL, NULL, 0, NULL, 0, 0, &pSvc
    );

    if (FAILED(hres)) {
        pLoc->Release();
        CoUninitialize();
        return "";
    }

    hres = CoSetProxyBlanket(
        pSvc, RPC_C_AUTHN_WINNT, RPC_C_AUTHZ_NONE, NULL,
        RPC_C_AUTHN_LEVEL_CALL, RPC_C_IMP_LEVEL_IMPERSONATE,
        NULL, EOAC_NONE
    );

    if (FAILED(hres)) {
        pSvc->Release();
        pLoc->Release();
        CoUninitialize();
        return "";
    }

    std::string query = "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + std::to_string(processId);
    IEnumWbemClassObject* pEnumerator = NULL;
    hres = pSvc->ExecQuery(
        bstr_t("WQL"),
        bstr_t(query.c_str()),
        WBEM_FLAG_FORWARD_ONLY | WBEM_FLAG_RETURN_IMMEDIATELY,
        NULL, &pEnumerator
    );

    std::string commandLine;
    if (SUCCEEDED(hres)) {
        IWbemClassObject* pclsObj = NULL;
        ULONG uReturn = 0;

        while (pEnumerator) {
            HRESULT hr = pEnumerator->Next(WBEM_INFINITE, 1, &pclsObj, &uReturn);
            if (0 == uReturn) {
                break;
            }

            VARIANT vtProp;
            hr = pclsObj->Get(L"CommandLine", 0, &vtProp, 0, 0);
            if (SUCCEEDED(hr) && vtProp.vt == VT_BSTR) {
                commandLine = WStringToString(vtProp.bstrVal);
            }
            VariantClear(&vtProp);
            pclsObj->Release();
        }
        pEnumerator->Release();
    }

    pSvc->Release();
    pLoc->Release();
    CoUninitialize();

    return commandLine;
}
