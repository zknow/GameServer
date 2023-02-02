namespace GameServer.Models;

public class AppSettings
{
    public int MatchPlayerNum { get; set; }

    public GameServerSettings GameServer { get; set; }

    public RelayServerSettings RelayServer { get; set; }
}

public class GameServerSettings
{
    public string IP { get; set; }
    public int Port { get; set; }
}

public class RelayServerSettings
{
    public string IP { get; set; }
    public int Port { get; set; }
}

