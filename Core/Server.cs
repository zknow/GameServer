using System.Net.Sockets;

namespace GameServer.Core;

public class GameService : NetCoreServer.TcpServer
{
    public static GameService Instance = null;
    private MatchManager matchMgr;

    public GameService(string address, int port) : base(address, port) { }

    public static void Initialize()
    {
        Instance = new GameService(Config.AppSettings.GameServer.IP, Config.AppSettings.GameServer.Port);
        if (!Instance.Start())
        {
            Console.WriteLine("Start Failed!");
            Environment.Exit(1);
        }
        Console.WriteLine("Server starting...");

        Instance.matchMgr = new MatchManager();
        Instance.matchMgr.StartMatchUpPlayers();
    }

    public void ShowPlayerMatchStatus()
    {
        Console.WriteLine($"PlayerPool:{matchMgr.PlayerPool.Count}");
        Console.WriteLine($"MatchingPool:{matchMgr.MatchingPool.Count}");
        foreach (var player in matchMgr.PlayerPool)
        {
            Console.WriteLine($"player Status:{player.Value.State.ToString()}");
        }
    }

    protected override NetCoreServer.TcpSession CreateSession() { return new Session(this, matchMgr); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"GameServer caught an error with code {error}");
    }

    protected override void OnStopping()
    {
        base.OnStopping();
        Instance.matchMgr.Stop();
    }
}
