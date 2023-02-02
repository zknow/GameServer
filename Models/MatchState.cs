namespace GameServer.Models;

public enum MatchState
{
    None = 0,
    Ready,
    Matching,
    Playing,
    Done,
}