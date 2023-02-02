using NetCoreServer;

namespace GameServer.Models;

public class Player
{
    public TcpSession Session { get; set; }

    public long UID { get; set; }

    public bool IsHost { get; set; }

    public MatchState State { get; set; }
}