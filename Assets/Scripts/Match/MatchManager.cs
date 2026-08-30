using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private int targetScore = 11;
    [SerializeField] private TeamSide servingTeam = TeamSide.Player;
    [SerializeField] private bool showDebugGui = true;
    [SerializeField] private CareerCalendarManager careerCalendarManager;
    [SerializeField] private TrainingSelectionUI trainingSelectionUI;
    [SerializeField] private GameFlowManager gameFlowManager;

    public MatchState CurrentState { get; private set; } = MatchState.None;
    public TeamSide ServingTeam => servingTeam;
    public TeamSide ReceivingTeam { get; private set; } = TeamSide.Opponent;
    public TeamSide CurrentTouchTeam { get; private set; }
    public int CurrentTouchCount { get; private set; }
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }
    public int TargetScore => targetScore;
    public MatchStats CurrentMatchStats => matchStats;

    private TeamSide lastAttackTeam;
    private CharacterRole lastAttackActor;
    private bool hasLastAttackTeam;
    private bool hasAdvancedCareerCalendar;
    private bool hasShownTrainingSelection;
    private readonly MatchStats matchStats = new MatchStats();

    private void Start()
    {
        if (careerCalendarManager == null)
        {
            careerCalendarManager = FindFirstObjectByType<CareerCalendarManager>();
        }

        if (careerCalendarManager == null)
        {
            careerCalendarManager = new GameObject("CareerCalendarManager").AddComponent<CareerCalendarManager>();
        }

        if (trainingSelectionUI == null)
        {
            trainingSelectionUI = FindFirstObjectByType<TrainingSelectionUI>();
        }

        if (gameFlowManager == null)
        {
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();
        }

        if (CurrentState == MatchState.None && gameFlowManager == null)
        {
            StartMatch();
        }
    }

    public void StartMatch()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        servingTeam = TeamSide.Player;
        hasAdvancedCareerCalendar = false;
        hasShownTrainingSelection = false;
        lastAttackActor = null;
        matchStats.Reset();
        StartServe();
    }

    public void StartServe()
    {
        CurrentTouchTeam = servingTeam;
        ReceivingTeam = GetOppositeTeam(servingTeam);
        CurrentTouchCount = 0;
        hasLastAttackTeam = false;
        lastAttackActor = null;
        ChangeState(MatchState.ServeReady);
    }

    public bool BallServed(TeamSide team, CharacterRole actor = null)
    {
        if (CurrentState != MatchState.ServeReady || team != servingTeam)
        {
            return false;
        }

        CurrentTouchTeam = team;
        ReceivingTeam = GetOppositeTeam(team);
        CurrentTouchCount = 1;
        matchStats.RecordTouch(actor, BallTouchAction.Serve);
        ChangeState(MatchState.Rally);
        return true;
    }

    public bool RegisterTouch(TeamSide team, BallTouchAction action, CharacterRole actor = null)
    {
        if (action == BallTouchAction.Serve)
        {
            return BallServed(team, actor);
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
        matchStats.RecordTouch(actor, action);

        if (CurrentTouchCount > 3)
        {
            ScorePoint(GetOppositeTeam(team));
            return false;
        }

        if (action == BallTouchAction.Spike)
        {
            lastAttackTeam = team;
            lastAttackActor = actor;
            hasLastAttackTeam = true;
        }

        return true;
    }

    public void RegisterAttackLanding(bool isIn)
    {
        if (CurrentState != MatchState.Rally || !hasLastAttackTeam)
        {
            return;
        }

        matchStats.RecordAttackResult(lastAttackActor, isIn);
        ScorePoint(isIn ? lastAttackTeam : GetOppositeTeam(lastAttackTeam));
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
        bool isMatchOver = IsMatchOver();
        ChangeState(isMatchOver ? MatchState.MatchOver : MatchState.PointScored);
        matchStats.SetFinalScore(PlayerScore, OpponentScore);

        if (isMatchOver && !hasAdvancedCareerCalendar)
        {
            hasAdvancedCareerCalendar = true;
            careerCalendarManager.AdvanceAfterMatch();
        }

        if (isMatchOver && !hasShownTrainingSelection)
        {
            hasShownTrainingSelection = true;

            if (gameFlowManager != null)
            {
                gameFlowManager.OnMatchEnded();
            }
            else if (trainingSelectionUI != null)
            {
                trainingSelectionUI.ShowRandomOptions();
            }
        }
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

        GUILayout.BeginArea(new Rect(10f, 10f, 220f, 290f), GUI.skin.box);
        GUILayout.Label($"State: {CurrentState}");
        GUILayout.Label($"Score: Player {PlayerScore} - {OpponentScore} Opponent");
        GUILayout.Label($"Serving: {servingTeam}");
        GUILayout.Label($"Receiving: {ReceivingTeam}");
        GUILayout.Label($"Touch: {CurrentTouchTeam} x {CurrentTouchCount}");
        GUILayout.Label($"Last Attack: {(hasLastAttackTeam ? lastAttackTeam.ToString() : "None")}");

        if (careerCalendarManager != null)
        {
            GUILayout.Label($"Career: Year {careerCalendarManager.CurrentYear} / Month {careerCalendarManager.CurrentMonth}");
        }

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

        if (GUILayout.Button("Force Match Over"))
        {
            ForceMatchOver();
        }

        if (GUILayout.Button("Restart Match"))
        {
            StartMatch();
        }

        if (careerCalendarManager != null && GUILayout.Button("Reset Career"))
        {
            careerCalendarManager.ResetCareerCalendar();
        }

        GUILayout.EndArea();
    }

    private void ForceMatchOver()
    {
        if (CurrentState == MatchState.MatchOver)
        {
            return;
        }

        if (PlayerScore < targetScore - 1)
        {
            PlayerScore = targetScore - 1;
        }

        ScorePoint(TeamSide.Player);
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
