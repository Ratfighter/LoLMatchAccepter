using LeagueMatchAccepter;

namespace LoLMatchAccepterNet
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("League of Legends Match Auto-Accepter (API Method)");
            Console.WriteLine("-----------------------------------------------");

            try
            {
                LCU lcu = new();
                Console.WriteLine("Searching for League of Legends client...");

                if (string.IsNullOrEmpty(lcu.Port) || string.IsNullOrEmpty(lcu.Password))
                {
                    Console.WriteLine("Failed to find League client. Make sure it's running.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"League client found! Connected to port: {lcu.Port}");
                await lcu.AutoAccept();
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
