using System.Text;

namespace GameServer.Core;

public enum GameProtocol
{
    Ping = 0,
    PlayerReady,
    PlayerCancel,
    RoomInfo,
}

public enum RelayProtocol
{
    Ping = 0,
    CreateRoom,
}

public class PackDef
{
    public static string Header => "Key";

    public static int HeaderLen => Encoding.ASCII.GetBytes(Header).Length;

    public static int DataLen = 4;
}