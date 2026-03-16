using System.Diagnostics;
using System.Text.RegularExpressions;

namespace LoLMatchAccepterNet.LCU
{
    public partial class LeagueProcessDiscovery
    {
        [GeneratedRegex("--remoting-auth-token=([\\w-]*)")]
        private static partial Regex PasswordRegex();

        [GeneratedRegex("--app-port=([0-9]*)")]
        private static partial Regex PortRegex();

        public LcuConnectionInfo? DiscoverLeagueClient()
        {
            Process[] processes = Process.GetProcessesByName("LeagueClientUx");

            if (processes.Length == 0)
            {
                return null;
            }

            foreach (var process in processes)
            {
                try
                {
                    string commandLine = GetCommandLine(process.Id);

                    var portMatch = PortRegex().Match(commandLine);
                    var passwordMatch = PasswordRegex().Match(commandLine);

                    if (portMatch.Success && passwordMatch.Success)
                    {
                        var connectionInfo = new LcuConnectionInfo
                        {
                            Port = portMatch.Groups[1].Value,
                            Password = passwordMatch.Groups[1].Value
                        };

                        Console.WriteLine($"League client found! Connected to port: {connectionInfo.Port}");
                        return connectionInfo;
                    }
                }
                catch
                {
                    // Continue to next process
                }
            }

            return null;
        }

        private static string GetCommandLine(int processId)
        {
            using var process = Process.GetProcessById(processId);
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            using var objects = searcher.Get();
            var enumerator = objects.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current["CommandLine"]?.ToString() ?? string.Empty;
        }
    }
}
