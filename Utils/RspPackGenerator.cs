using System.Text;
using GameServer.Core;

namespace GameServer.Utils;

public class RspPackGenerator
{
    public static byte[] AttachHeaderPack(byte[] bytes)
    {
        var buf = new List<byte>() { };
        var headerBytes = Encoding.ASCII.GetBytes(PackDef.Header);
        buf.AddRange(headerBytes);
        buf.AddRange(BitConverter.GetBytes(bytes.Length));
        buf.AddRange(bytes);
        return buf.ToArray();
    }

    // 產生心跳回應封包
    public static byte[] CreatePongPack()
    {
        var buf = new List<byte>() { (byte)GameProtocol.Ping };
        buf.Add(Convert.ToByte(true));
        return AttachHeaderPack(buf.ToArray());
    }

    // 產生玩家準備配對回應封包
    public static byte[] CreatePlayerReadyPack(long uid, bool isHost)
    {
        var buf = new List<byte>() { (byte)GameProtocol.PlayerReady };
        buf.Add(Convert.ToByte(true));
        buf.AddRange(BitConverter.GetBytes(uid));
        buf.AddRange(BitConverter.GetBytes(isHost));
        return AttachHeaderPack(buf.ToArray());
    }

    // 產生玩家準備配對回應封包
    public static byte[] CreatePlayerCancelPack(long uid)
    {
        var buf = new List<byte>() { (byte)GameProtocol.PlayerCancel };
        buf.Add(Convert.ToByte(true));
        buf.AddRange(BitConverter.GetBytes(uid));
        return AttachHeaderPack(buf.ToArray());
    }

    // 產生玩家開房成功回應封包
    public static byte[] CreateRoomSuccessPack(string ip, int port, int roomId)
    {
        var buf = new List<byte>() { (byte)GameProtocol.RoomInfo };

        // Add Ip
        var ipBs = Encoding.UTF8.GetBytes(ip);
        buf.AddRange(BitConverter.GetBytes(ipBs.Length));
        buf.AddRange(ipBs);

        //Add Port
        buf.AddRange(BitConverter.GetBytes(port));

        //Add roomId
        buf.AddRange(BitConverter.GetBytes(roomId));
        return AttachHeaderPack(buf.ToArray());
    }

    // 產生發生錯誤回應封包
    public static byte[] CreateErrorMsgPack(byte packNo, string msg)
    {
        var buf = new List<byte>() { packNo };
        buf.Add(Convert.ToByte(false));
        var msgBytes = Encoding.UTF8.GetBytes(msg);
        buf.AddRange(BitConverter.GetBytes(msgBytes.Length));
        buf.AddRange(msgBytes);
        return AttachHeaderPack(buf.ToArray());
    }
}