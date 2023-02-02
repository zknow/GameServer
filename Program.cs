using GameServer.Core;
using GameServer.Utils;

namespace GameServer;

public class Program
{
    static void Main(string[] args)
    {
        Config.LoadConfig();

        RelayClient.Initialize();
        GameService.Initialize();

        ConsoleInputListen();
        Close();
    }

    private static void ConsoleInputListen()
    {
        bool stop = false;
        while (!stop)
        {
            string? line = Console.ReadLine();
            switch (line)
            {
                case "!":
                    stop = true;
                    break;
                case "ping":
                    var pack = ReqPackGenerator.CreatePingPack();
                    RelayClient.Client.SendAsync(pack);
                    break;
                case "info":
                    GameService.Instance.ShowPlayerMatchStatus();
                    break;
                default:
                    break;
            }
        }
    }

    private static void Close()
    {
        // Stop the server
        Console.Write("Server stopping...");
        RelayClient.Client.DisconnectAndStop();
        GameService.Instance.Stop();
        GameService.Instance.Dispose();
        Console.WriteLine("Done!");
    }
}