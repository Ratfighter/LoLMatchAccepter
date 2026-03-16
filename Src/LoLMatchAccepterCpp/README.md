# LoL Match Accepter C++

C++ implementation of the League of Legends Match Auto-Accepter.

## Description

This is a C++ port of the LoLMatchAccepterNet project. It automatically accepts League of Legends matches when they are found.

## Features

- Automatically discovers the League of Legends client process
- Connects to the League Client Update (LCU) API
- Monitors for match ready checks and automatically accepts them
- Handles game flow from queue to champ select to in-game
- Automatically navigates back to lobby after games

## Requirements

- Windows OS (uses WinAPI, WinHTTP, and WMI)
- Visual Studio 2022 or later with **Desktop development with C++** workload installed
  - Required components:
    - MSVC C++ compiler
    - Windows SDK
    - C++ CMake tools (optional)
- League of Legends client installed

**Note:** If you encounter build errors about missing Microsoft.Cpp.Default.props, you need to install the "Desktop development with C++" workload through Visual Studio Installer.

## Project Structure

- `src/main.cpp` - Entry point of the application
- `src/LCU/LcuConnectionInfo.h` - Data structure for LCU connection information
- `src/LCU/LeagueProcessDiscovery.h/cpp` - Discovers League client process and extracts connection info
- `src/LCU/LcuHttpClient.h/cpp` - HTTP client for communicating with LCU API
- `src/LCU/Game.h/cpp` - Handles game state and match operations
- `src/LCU/MatchAcceptService.h/cpp` - Main service loop for auto-accepting matches
- `src/LCU/LcuClient.h/cpp` - High-level client that orchestrates all components
- `src/json.hpp` - JSON library (nlohmann/json)

## Building

1. Open the solution in Visual Studio
2. Build the LoLMatchAccepterCpp project
3. Run the executable

## Usage

1. Start the League of Legends client
2. Run the LoLMatchAccepterCpp executable
3. The application will automatically detect the League client and start monitoring for matches
4. Press ESC to stop the auto-accepter

## Dependencies

- WinHTTP - For HTTPS communication with LCU API
- WMI (Windows Management Instrumentation) - For process discovery
- nlohmann/json - For JSON parsing (single-header library)

## Implementation Notes

This C++ implementation mirrors the functionality of the C# version with the following differences:

- Uses WinHTTP instead of HttpClient
- Uses WMI via COM for process command-line discovery
- Uses nlohmann/json for JSON parsing instead of Newtonsoft.Json or System.Text.Json
- Uses std::atomic for cancellation instead of CancellationTokenSource
- Uses native Windows API for keyboard input detection
