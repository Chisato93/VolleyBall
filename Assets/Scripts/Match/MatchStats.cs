using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMatchStats
{
    public string PlayerName;
    public TeamSide TeamSide;
    public int Serves;
    public int Receives;
    public int Sets;
    public int Spikes;
    public int Blocks;
    public int Points;
    public int AttackSuccesses;
    public int AttackFailures;

    public PlayerMatchStats(string playerName, TeamSide teamSide)
    {
        PlayerName = playerName;
        TeamSide = teamSide;
    }

    public void RecordTouch(BallTouchAction action)
    {
        switch (action)
        {
            case BallTouchAction.Serve:
                Serves++;
                break;
            case BallTouchAction.Receive:
                Receives++;
                break;
            case BallTouchAction.Set:
                Sets++;
                break;
            case BallTouchAction.Spike:
                Spikes++;
                break;
            case BallTouchAction.Block:
                Blocks++;
                break;
        }
    }
}

public class MatchStats
{
    private readonly List<PlayerMatchStats> playerStats = new List<PlayerMatchStats>();
    private readonly Dictionary<CharacterRole, PlayerMatchStats> statsByRole = new Dictionary<CharacterRole, PlayerMatchStats>();

    public IReadOnlyList<PlayerMatchStats> PlayerStats => playerStats;
    public int FinalPlayerScore { get; private set; }
    public int FinalOpponentScore { get; private set; }

    public void Reset()
    {
        playerStats.Clear();
        statsByRole.Clear();
        FinalPlayerScore = 0;
        FinalOpponentScore = 0;
    }

    public void RecordTouch(CharacterRole actor, BallTouchAction action)
    {
        PlayerMatchStats stats = GetOrCreatePlayerStats(actor);
        if (stats == null)
        {
            return;
        }

        stats.RecordTouch(action);
    }

    public void RecordAttackResult(CharacterRole actor, bool isSuccess)
    {
        PlayerMatchStats stats = GetOrCreatePlayerStats(actor);
        if (stats == null)
        {
            return;
        }

        if (isSuccess)
        {
            stats.AttackSuccesses++;
            stats.Points++;
        }
        else
        {
            stats.AttackFailures++;
        }
    }

    public void SetFinalScore(int playerScore, int opponentScore)
    {
        FinalPlayerScore = playerScore;
        FinalOpponentScore = opponentScore;
    }

    private PlayerMatchStats GetOrCreatePlayerStats(CharacterRole actor)
    {
        if (actor == null)
        {
            return null;
        }

        if (statsByRole.TryGetValue(actor, out PlayerMatchStats stats))
        {
            return stats;
        }

        stats = new PlayerMatchStats(GetPlayerName(actor), actor.TeamSide);
        statsByRole.Add(actor, stats);
        playerStats.Add(stats);
        return stats;
    }

    private static string GetPlayerName(CharacterRole actor)
    {
        if (actor.PlayerData != null && !string.IsNullOrWhiteSpace(actor.PlayerData.PlayerName))
        {
            return actor.PlayerData.PlayerName;
        }

        return actor.gameObject.name;
    }
}
