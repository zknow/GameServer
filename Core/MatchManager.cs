using System.Collections.Concurrent;
using GameServer.Models;
using GameServer.Utils;
using NetCoreServer;

namespace GameServer.Core;

public class MatchSession
{
    public long MatchTick { get; set; }
    public string RelayServerIP { get; set; }
    public int RelayServerPort { get; set; }
    public TcpSession Session { get; set; }
}

public class MatchManager
{
    public ConcurrentDictionary<Guid, Player> PlayerPool;
    public ConcurrentDictionary<Guid, MatchSession> MatchingPool;

    private object matchLock = new object();

    public bool IsStoped { get; set; } = false;

    public int MatchNum = -1;

    public MatchManager()
    {
        PlayerPool = new ConcurrentDictionary<Guid, Player>();
        MatchingPool = new ConcurrentDictionary<Guid, MatchSession>();

        RelayClient.Client.CreateRoomSuccessEvent += OnCreateRoomSuccessEvent;
        RelayClient.Client.CreateRoomFailedEvent += OnCreateRoomFailedEvent;
    }

    public void SetPlayerUIDAndIsHost(Guid sessionId, long uid, bool isHost)
    {
        lock (matchLock)
        {
            if (PlayerPool.ContainsKey(sessionId))
            {
                PlayerPool[sessionId].UID = uid;
                PlayerPool[sessionId].IsHost = isHost;
            }
        }
    }

    public void SetPlayerStatus(Guid sessionId, MatchState state)
    {
        lock (matchLock)
        {
            if (PlayerPool.ContainsKey(sessionId))
            {
                PlayerPool[sessionId].State = state;
            }
        }
    }

    public async void StartMatchUpPlayers()
    {
        while (!IsStoped)
        {
            await Task.Delay(1000);
            if (MatchNum == -1)
            {
                continue;
            }

            lock (matchLock)
            {
                var rdyPlayers = PlayerPool.Where(c => c.Value.State == MatchState.Ready);
                if (rdyPlayers.Count() >= MatchNum) // 等待人數滿足開房最低限制
                {
                    var hostPlayer = rdyPlayers.FirstOrDefault(p => p.Value.IsHost).Value;
                    if (hostPlayer != null)
                    {
                        long matchTick = DateTime.Now.Ticks;
                        foreach (var player in rdyPlayers)
                        {
                            // 將準備配對的這群玩家設定成配對中，以防下次又被抓出來配對
                            player.Value.State = MatchState.Matching;
                            MatchingPool.TryAdd(player.Key, new MatchSession()
                            {
                                MatchTick = matchTick,
                                RelayServerIP = Config.AppSettings.RelayServer.IP,
                                RelayServerPort = Config.AppSettings.RelayServer.Port,
                                Session = player.Value.Session,
                            });
                        }
                        var reqPack = ReqPackGenerator.CreateCreateRoomPack(matchTick, hostPlayer.UID, rdyPlayers.Count());
                        RelayClient.Client.SendAsync(reqPack);
                    }
                }
            }
        }
    }

    private void OnCreateRoomSuccessEvent(long matchTick, int roomId)
    {
        lock (matchLock)
        {
            var matchs = MatchingPool.Where(m => m.Value.MatchTick == matchTick).Select(x => x.Value);
            foreach (var matchData in matchs)
            {
                var pack = RspPackGenerator.CreateRoomSuccessPack(matchData.RelayServerIP, matchData.RelayServerPort, roomId);
                matchData.Session.SendAsync(pack);
                MatchingPool.TryRemove(matchData.Session.Id, out _);
                SetPlayerStatus(matchData.Session.Id, MatchState.Done);
            }
            Console.WriteLine("OnCreateRoomSuccessEvent");
        }
    }

    private void OnCreateRoomFailedEvent(long matchTick, string errMsg)
    {
        lock (matchLock)
        {
            var matchs = MatchingPool.Where(m => m.Value.MatchTick == matchTick).Select(x => x.Value);
            foreach (var matchData in matchs)
            {
                MatchingPool.TryRemove(matchData.Session.Id, out _);
                SetPlayerStatus(matchData.Session.Id, MatchState.Ready);
                Console.WriteLine($"OnCreateRoomFailedEvent:{errMsg}");
            }
        }
    }

    public void Stop()
    {
        IsStoped = true;
    }
}