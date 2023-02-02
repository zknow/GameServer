using System.Net.Sockets;
using GameServer.Utils;

namespace GameServer.Core;

public class RelayClient : NetCoreServer.TcpClient
{
    public Action<long, int> CreateRoomSuccessEvent = null;
    public Action<long, string> CreateRoomFailedEvent = null;

    public static RelayClient Client = null;

    private bool _stop;
    public bool IsRelayServerConnected { get; set; }
    public List<byte> tmpPackBuf = new List<byte>();

    public RelayClient(string address, int port) : base(address, port) { }

    public static void Initialize()
    {
        try
        {
            Client = new RelayClient(Config.AppSettings.RelayServer.IP, Config.AppSettings.RelayServer.Port);
            Client.ConnectAsync();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Client Connect Failed!" + ex.Message);
        }
    }

    protected override void OnReceived(byte[] rcvBuffer, long offset, long size)
    {
        try
        {
            var buf = new byte[size];
            Array.Copy(rcvBuffer, offset, buf, 0, size);
            tmpPackBuf.AddRange(buf);
            tmpPackBuf = Unpacker.Unpack(tmpPackBuf, SendResponse);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void SendResponse(byte[] rcvBuffer)
    {
        try
        {
            var rcvPackParser = new PackParser(rcvBuffer);
            byte method = rcvPackParser.GetByte();
            Console.WriteLine($"RelayClient Rev {(RelayProtocol)method}");
            switch ((RelayProtocol)method)
            {
                case RelayProtocol.Ping:
                    bool ok = rcvPackParser.GetBoolen();
                    Console.WriteLine($"RelayServer Pong:{ok}");
                    break;
                case RelayProtocol.CreateRoom:
                    bool success = rcvPackParser.GetBoolen();
                    long matchTick = rcvPackParser.GetLong();
                    if (success)
                    {
                        int roomId = rcvPackParser.GetInt();
                        CreateRoomSuccessEvent?.Invoke(matchTick, roomId);
                    }
                    else
                    {
                        string errMsg = rcvPackParser.GetString();
                        CreateRoomFailedEvent?.Invoke(matchTick, errMsg);
                    }
                    break;
                default:
                    Console.WriteLine("無法辨識的指令");
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"Relay Client connected");
        IsRelayServerConnected = true;
    }

    protected override void OnDisconnected()
    {
        IsRelayServerConnected = false;
        Console.WriteLine($"Relay Client disconnected Retry...");
        // Wait for a while...
        Thread.Sleep(3000);

        // Try to connect again
        if (!_stop)
            ConnectAsync();
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Relay TCP client caught an error with code {error}");
    }
}