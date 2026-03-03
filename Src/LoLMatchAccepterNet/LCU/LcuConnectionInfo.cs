namespace LoLMatchAccepterNet.LCU
{
    public class LcuConnectionInfo
    {
        public string Port { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public bool IsValid => !string.IsNullOrEmpty(Port) && !string.IsNullOrEmpty(Password);
    }
}
