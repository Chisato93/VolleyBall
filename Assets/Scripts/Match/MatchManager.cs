using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private int targetScore = 11;
    [SerializeField] private TeamSide servingTeam = TeamSide.Player;
    [SerializeField] private bool showDebugGui = true;

    public MatchState CurrentState { get; private set; } = MatchState.None;
    public TeamSide ServingTeam => servingTeam;
    public TeamSide ReceivingTeam { get; private set; } = TeamSide.Opponent;
    public TeamSide CurrentTouchTeam { get; private set; }
    public int CurrentTouchCount { get; private set; }
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }
    public int TargetScore => targetScore;

    private void Start()
    {
        if (CurrentState == MatchState.None)
        {
            StartMatch();
        }
    }

    public void StartMatch()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        servingTeam = TeamSide.Player;
        StartServe();
    }

    public void StartServe()
    {
        CurrentTouchTeam = servingTeam;
        ReceivingTeam = GetOppositeTeam(servingTeam);
        CurrentTouchCount = 0;
        ChangeState(MatchState.ServeReady);
    }

    public bool BallServed(TeamSide team)
    {
        if (CurrentState != MatchState.ServeReady || team != servingTeam)
        {
            return false;
        }

        CurrentTouchTeam = team;
        ReceivingTeam = GetOppositeTeam(team);
        CurrentTouchCount = 1;
        ChangeState(MatchState.Rally);
        return true;
    }

    public bool RegisterTouch(TeamSide team, BallTouchAction action)
    {
        if (action == BallTouchAction.Serve)
        {
            return BallServed(team);
        }

        if (CurrentState != MatchState.Rally)
        {
            return false;
        }

        if (team != CurrentTouchTeam)
        {
            CurrentTouchTeam = team;
            CurrentTouchCount = 0;
        }

        CurrentTouchCount++;

        if (CurrentTouchCount > 3)
        {
            ScorePoint(GetOppositeTeam(team));
            return false;
        }

        return true;
    }

    public void RegisterNetCross(TeamSide fromTeam, TeamSide toTeam)
    {
        if (CurrentState != MatchState.Rally)
        {
            return;
        }

        if (CurrentTouchTeam != fromTeam)
        {
            return;
        }

        ReceivingTeam = toTeam;
    }

    public void ScorePoint(TeamSide scoringTeam)
    {
        if (CurrentState == MatchState.MatchOver)
        {
            return;
        }

        if (scoringTeam == TeamSide.Player)
        {
            PlayerScore++;
        }
        else
        {
            OpponentScore++;
        }

        servingTeam = scoringTeam;
        ChangeState(IsMatchOver() ? MatchState.MatchOver : MatchState.PointScored);
    }

    public void ResetRally()
    {
        if (CurrentState == MatchState.MatchOver)
        {
            return;
        }

        StartServe();
    }

    public bool CanServe(TeamSide team)
    {
        return CurrentState == MatchState.ServeReady && team == servingTeam;
    }

    public bool CanPlayRallyAction()
    {
        return CurrentState == MatchState.Rally;
    }

    private void OnGUI()
    {
        if (!showDebugGui)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(10f, 10f, 220f, 220f), GUI.skin.box);
        GUILayout.Label($"State: {CurrentState}");
        GUILayout.Label($"Score: Player {PlayerScore} - {OpponentScore} Opponent");
        GUILayout.Label($"Serving: {servingTeam}");
        GUILayout.Label($"Receiving: {ReceivingTeam}");
        GUILayout.Label($"Touch: {CurrentTouchTeam} x {CurrentTouchCount}");

        if (GUILayout.Button("Next"))
        {
            GoToNextDebugStep();
        }

        if (GUILayout.Button("Player Point"))
        {
            ScorePoint(TeamSide.Player);
        }

        if (GUILayout.Button("Opponent Point"))
        {
            ScorePoint(TeamSide.Opponent);
        }

        if (GUILayout.Button("Restart Match"))
        {
            StartMatch();
        }

        GUILayout.EndArea();
    }

    private void GoToNextDebugStep()
    {
        switch (CurrentState)
        {
            case MatchState.None:
            case MatchState.Ready:
                StartMatch();
                break;
            case MatchState.ServeReady:
                BallServed(servingTeam);
                break;
            case MatchState.Rally:
                RegisterTouch(CurrentTouchTeam, BallTouchAction.Receive);
                break;
            case MatchState.PointScored:
                ResetRally();
                break;
            case MatchState.MatchOver:
                StartMatch();
                break;
        }
    }

    private bool IsMatchOver()
    {
        return PlayerScore >= targetScore || OpponentScore >= targetScore;
    }

    private static TeamSide GetOppositeTeam(TeamSide team)
    {
        return team == TeamSide.Player ? TeamSide.Opponent : TeamSide.Player;
    }

    private void ChangeState(MatchState nextState)
    {
        CurrentState = nextState;
    }
}
