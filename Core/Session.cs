using System.Net.Sockets;
using GameServer.Models;
using GameServer.Utils;

namespace GameServer.Core;

public class Session : NetCoreServer.TcpSession
{
    public List<byte> tmpPackBuf = new List<byte>();
    private MatchManager matchMgr;

    public Session(GameService server, MatchManager matchMgr) : base(server)
    {
        this.matchMgr = matchMgr;
    }

    protected override void OnConnected()
    {
        var player = new Player() { Session = this };
        matchMgr.PlayerPool.TryAdd(Id, player);
        Console.WriteLine($"Id {Id} connected!");
    }

    protected override void OnDisconnected()
    {
        matchMgr.PlayerPool.TryRemove(Id, out _);
        matchMgr.MatchingPool.TryRemove(Id, out _);
        Console.WriteLine($"Id {Id} disconnected!");
    }

    protected override void OnReceived(byte[] rcvBuffer, long offset, long size)
    {
        try
        {
            var buf = new byte[size];
            Array.Copy(rcvBuffer, offset, buf, 0, size);
            tmpPackBuf.AddRange(buf);
            tmpPackBuf = Unpacker.Unpack(tmpPackBuf, HandlingRequest);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void HandlingRequest(byte[] rcvBuffer)
    {
        byte packNo;
        try
        {
            byte[] rspPack;
            var packParser = new PackParser(rcvBuffer);
            packNo = packParser.GetByte();
            switch ((GameProtocol)packNo)
            {
                case GameProtocol.Ping:
                    rspPack = RspPackGenerator.CreatePongPack();
                    break;
                case GameProtocol.PlayerReady:
                    long uid = packParser.GetLong();
                    bool isHost = packParser.GetBoolen();
                    if (RelayClient.Client.IsRelayServerConnected)
                    {
                        matchMgr.SetPlayerUIDAndIsHost(Id, uid, isHost);
                        matchMgr.SetPlayerStatus(Id, MatchState.Ready);
                        if (isHost)
                        {
                            try
                            {
                                int playerNum = packParser.GetInt();
                                matchMgr.MatchNum = playerNum;
                            }
                            catch (System.Exception)
                            {
                                matchMgr.MatchNum = Config.AppSettings.MatchPlayerNum;
                            }
                        }
                        rspPack = RspPackGenerator.CreatePlayerReadyPack(uid, isHost);
                    }
                    else
                    {
                        rspPack = RspPackGenerator.CreateErrorMsgPack(packNo, "RelayServer Is Disconnected!!");
                    }
                    break;
                case GameProtocol.PlayerCancel:
                    uid = packParser.GetLong();
                    matchMgr.SetPlayerStatus(Id, MatchState.None);
                    matchMgr.MatchingPool.TryRemove(Id, out _);
                    rspPack = RspPackGenerator.CreatePlayerCancelPack(uid);
                    break;
                default:
                    rspPack = RspPackGenerator.CreateErrorMsgPack(packNo, "無法辨識的指令");
                    break;
            }
            SendAsync(rspPack);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"TCP session caught an error with code {error}");
    }
}
