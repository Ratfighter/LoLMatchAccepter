using LeagueMatchAccepter;
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
                bool isRetryingActive = false;
                using (NotificatorServer notificatorServer = new())
                {
                    while (!manualExitInitiated)
                    {
                        using (LcuClient lcu = new())
                        {
                            if (!lcu.IsClientFound())
                            {
                                if (!isRetryingActive)
                                {
                                    Console.WriteLine("Failed to find League client. Retrying every 5 seconds...");
                                    isRetryingActive = true;
                                }
                                await Task.Delay(5000); // Wait before retrying
                                continue;
                            }
                            isRetryingActive = false; // Reset flag if client found
                            lcu.MatchFound += notificatorServer.SendNotification;
                            manualExitInitiated = await lcu.AutoAccept();
                        }
                    }
                    Console.WriteLine("Auto-accepter stopped. Press any key to exit...");
                    Console.ReadKey();
                }
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
