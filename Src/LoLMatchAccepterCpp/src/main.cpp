#include <iostream>
#include <thread>
#include <chrono>
#include "LCU/LcuClient.h"

int main() {
    std::cout << "League of Legends Match Auto-Accepter" << std::endl;
    std::cout << "-----------------------------------------------" << std::endl;

    try {
        std::cout << "Searching for League of Legends client..." << std::endl;
        bool manualExitInitiated = false;
        bool isRetryingActive = false;

        while (!manualExitInitiated) {
            LcuClient lcu;

            if (!lcu.IsClientFound()) {
                if (!isRetryingActive) {
                    std::cout << "Failed to find League client. Retrying every 5 seconds..." << std::endl;
                    isRetryingActive = true;
                }
                std::this_thread::sleep_for(std::chrono::milliseconds(5000));
                continue;
            }

            isRetryingActive = false;
            manualExitInitiated = lcu.AutoAccept();
        }

        std::cout << "Auto-accepter stopped. Press any key to exit..." << std::endl;
        std::cin.get();
    }
    catch (const std::exception& ex) {
        std::cout << "Error: " << ex.what() << std::endl;
        std::cout << "Press any key to exit..." << std::endl;
        std::cin.get();
    }

    return 0;
}
