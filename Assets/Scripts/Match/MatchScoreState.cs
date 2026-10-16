internal sealed class MatchScoreState
{
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }
    public int TargetScore { get; private set; }
    public TeamSide ServingTeam { get; private set; } = TeamSide.Player;

    public void Reset(int targetScore, TeamSide servingTeam)
    {
        PlayerScore = 0;
        OpponentScore = 0;
        TargetScore = targetScore;
        ServingTeam = servingTeam;
    }

    public void ScorePoint(TeamSide scoringTeam)
    {
        if (scoringTeam == TeamSide.Player)
        {
            PlayerScore++;
        }
        else
        {
            OpponentScore++;
        }

        ServingTeam = scoringTeam;
    }

    public bool IsMatchOver()
    {
        return PlayerScore >= TargetScore || OpponentScore >= TargetScore;
    }

    public void SetPlayerScore(int playerScore)
    {
        PlayerScore = playerScore;
    }

    public void SetScores(int playerScore, int opponentScore)
    {
        PlayerScore = playerScore;
        OpponentScore = opponentScore;
    }
}
