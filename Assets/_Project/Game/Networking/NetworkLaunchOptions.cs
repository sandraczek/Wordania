using System;

namespace Wordania.Networking
{
    public enum NetworkMode
    {
        SinglePlayer,
        Host,
        Client,
    }

    /// <summary>
    /// How this game instance takes part in a session. Inspector values can be overridden from the command line,
    /// so one build can run as host and another as client:
    /// <c>-netmode host|client|single  -address 192.168.0.10  -port 7777  -newid</c>
    /// (<c>-newid</c>: random player identity for this run, needed when two instances share PlayerPrefs on one machine).
    /// </summary>
    public sealed class NetworkLaunchOptions
    {
        public const int DefaultPort = 7777;

        public NetworkMode Mode { get; }
        public string Address { get; }
        public int Port { get; }
        public bool UseTemporaryIdentity { get; }

        public bool IsHost => Mode != NetworkMode.Client;
        public bool IsSinglePlayer => Mode == NetworkMode.SinglePlayer;

        public NetworkLaunchOptions(NetworkMode mode, string address, int port, bool useTemporaryIdentity)
        {
            Mode = mode;
            Address = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim();
            Port = port is > 0 and <= 65535 ? port : DefaultPort;
            UseTemporaryIdentity = useTemporaryIdentity;
        }

        public NetworkLaunchOptions WithCommandLine(string[] args)
        {
            NetworkMode mode = Mode;
            string address = Address;
            int port = Port;
            bool newId = UseTemporaryIdentity;

            for (int i = 0; i < args.Length; i++)
            {
                string next = i + 1 < args.Length ? args[i + 1] : null;
                switch (args[i].ToLowerInvariant())
                {
                    case "-netmode" when next != null:
                        mode = next.ToLowerInvariant() switch
                        {
                            "host" => NetworkMode.Host,
                            "client" => NetworkMode.Client,
                            "single" => NetworkMode.SinglePlayer,
                            _ => mode,
                        };
                        i++;
                        break;
                    case "-address" when next != null:
                        address = next;
                        i++;
                        break;
                    case "-port" when next != null:
                        if (int.TryParse(next, out int parsed)) port = parsed;
                        i++;
                        break;
                    case "-newid":
                        newId = true;
                        break;
                }
            }

            return new NetworkLaunchOptions(mode, address, port, newId);
        }

        public NetworkLaunchOptions WithCommandLine() => WithCommandLine(Environment.GetCommandLineArgs());
    }
}
