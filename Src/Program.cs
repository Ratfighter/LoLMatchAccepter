﻿using LeagueMatchAccepter;
using LoLMatchAccepterNet.Api;

namespace LoLMatchAccepterNet
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("League of Legends Match Auto-Accepter");
            Console.WriteLine("-----------------------------------------------");

            try
            {
                Console.WriteLine("Searching for League of Legends client...");
                bool manualExitInitiated = false;
                using (NotificatorServer notificatorServer = new())
                {
                    while (!manualExitInitiated)
                    {
                        using LcuClient lcu = new();
                        if (!lcu.IsClientFound())
                        {
                            Console.WriteLine("Failed to find League client. Retrying in 5 seconds...");
                            Thread.Sleep(5000);
                            continue;
                        }
                        lcu.MatchFound += notificatorServer.SendNotification;
                        manualExitInitiated = await lcu.AutoAccept();
                    }
                }
                Console.WriteLine("Auto-accepter stopped. Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
