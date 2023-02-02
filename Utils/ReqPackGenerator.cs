using System.Text;
using GameServer.Core;

namespace GameServer.Utils;

public class ReqPackGenerator
{
    public static byte[] AttachHeaderPack(byte[] bytes)
    {
        var buf = new List<byte>() { };
        buf.AddRange(Encoding.ASCII.GetBytes(PackDef.Header));
        buf.AddRange(BitConverter.GetBytes(bytes.Length));
        buf.AddRange(bytes);
        return buf.ToArray();
    }

    // 產生Ping封包
    public static byte[] CreatePingPack()
    {
        return AttachHeaderPack(new byte[] { (byte)GameProtocol.Ping });
    }

    /// <summary>
    /// 產生開房封包
    /// </summary>
    /// <param name="matchTick">對照是哪一組玩家用的識別碼</param>
    /// <param name="hostUID">告訴RelayServer房主是誰</param>
    /// <param name="playerNum">告訴RelayServer這間房有幾個人</param>
    public static byte[] CreateCreateRoomPack(long matchTick, long hostUID, int playerNum)
    {
        var buf = new List<byte>() { (byte)RelayProtocol.CreateRoom };
        buf.AddRange(BitConverter.GetBytes(matchTick));
        buf.AddRange(BitConverter.GetBytes(hostUID));
        buf.AddRange(BitConverter.GetBytes(playerNum));
        return AttachHeaderPack(buf.ToArray());
    }
}