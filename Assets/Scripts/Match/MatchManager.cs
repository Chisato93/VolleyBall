using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private int targetScore = 11;
    [SerializeField] private string ballName = "volleyball";
    [SerializeField] private float autoServeDelay = 1.2f;
    [SerializeField] private float autoServeForwardSpeed = 8f;
    [SerializeField] private float autoServeUpSpeed = 4f;
    [SerializeField] private Vector3 autoServeSpawnOffset = new Vector3(0f, 1.2f, 1f);
    [SerializeField] private float nextServeDelay = 1.2f;
#if UNITY_EDITOR
    [SerializeField] private bool showDebugGui = true;
    [SerializeField] private PlayerActionController playerActionController;
#endif
    [SerializeField] private CareerCalendarManager careerCalendarManager;
    [SerializeField] private TrainingSelectionUI trainingSelectionUI;
    [SerializeField] private GameFlowManager gameFlowManager;

    public MatchState CurrentState { get; private set; } = MatchState.None;
    public TeamSide ServingTeam => score.ServingTeam;
    public TeamSide ReceivingTeam => rally.ReceivingTeam;
    public TeamSide CurrentTouchTeam => rally.CurrentTouchTeam;
    public int CurrentTouchCount => rally.CurrentTouchCount;
    public int PlayerScore => score.PlayerScore;
    public int OpponentScore => score.OpponentScore;
    public int TargetScore => targetScore;
    public MatchStats CurrentMatchStats => matchStats;
    public CharacterRole CurrentServer => rotations.CurrentServer;

    private readonly MatchEndState matchEnd = new MatchEndState();
    private readonly MatchScoreState score = new MatchScoreState();
    private readonly RallyState rally = new RallyState();
    private readonly MatchStats matchStats = new MatchStats();
    private readonly MatchRotationState rotations = new MatchRotationState();

    private void Awake()
    {
        score.Reset(targetScore, TeamSide.Player);
    }

    private void Start()
    {
        EnsureReferences();
        StartMatchIfNeeded();
    }

    private void EnsureReferences()
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

#if UNITY_EDITOR
        if (playerActionController == null)
        {
            playerActionController = FindFirstObjectByType<PlayerActionController>();
        }
#endif
    }

    private void StartMatchIfNeeded()
    {
        if (CurrentState == MatchState.None && gameFlowManager == null)
        {
            StartMatch();
        }
    }

    public void StartMatch()
    {
        CancelInvoke(nameof(ResetRally));
        CancelInvoke(nameof(AutoServeCurrentServer));
        score.Reset(targetScore, TeamSide.Player);
        rotations.Reset(score.ServingTeam);
        matchEnd.Reset();
        matchStats.Reset();
        StartServe();
    }

    public void StartServe()
    {
        CancelInvoke(nameof(ResetRally));
        CancelInvoke(nameof(AutoServeCurrentServer));
        rally.ResetForServe(ServingTeam, GetOppositeTeam(ServingTeam));
        ChangeState(MatchState.ServeReady);
        ScheduleAutoServeIfNeeded();
    }

    public bool BallServed(TeamSide team, CharacterRole actor = null)
    {
        if (CurrentState != MatchState.ServeReady || !rotations.CanServe(team, actor))
        {
            return false;
        }

        rally.RecordServe(team, GetOppositeTeam(team));
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

        rally.RecordTouch(team);
        matchStats.RecordTouch(actor, action);

        if (rally.CurrentTouchCount > 3)
        {
            ScorePoint(GetOppositeTeam(team));
            return false;
        }

        if (action == BallTouchAction.Spike)
        {
            rally.RecordAttack(team, actor);
        }

        return true;
    }

    public void RegisterBallLanding(bool isIn, TeamSide landingTeam)
    {
        if (CurrentState != MatchState.Rally)
        {
            return;
        }

        TeamSide scoringTeam = GetLandingScoringTeam(isIn, landingTeam);
        RecordAttackResultIfNeeded(scoringTeam);
        ScorePoint(scoringTeam);
    }

    public void RegisterNetCross(TeamSide fromTeam, TeamSide toTeam)
    {
        if (CurrentState != MatchState.Rally)
        {
            return;
        }

        if (rally.CurrentTouchTeam != fromTeam)
        {
            return;
        }

        rally.SetReceivingTeam(toTeam);
    }

    public void ScorePoint(TeamSide scoringTeam)
    {
        if (CurrentState == MatchState.MatchOver)
        {
            return;
        }

        TeamSide previousServingTeam = ServingTeam;
        score.ScorePoint(scoringTeam);
        rotations.ApplyPointResult(previousServingTeam, ServingTeam);
        rotations.RestoreFormation();
        bool isMatchOver = IsMatchOver();
        UpdateStateAfterPoint(isMatchOver);
        StopBall();
        SaveFinalScore();

        if (isMatchOver)
        {
            HandleMatchOver();
            return;
        }

        Invoke(nameof(ResetRally), nextServeDelay);
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
        return CurrentState == MatchState.ServeReady && team == ServingTeam;
    }

    public bool CanServe(TeamSide team, CharacterRole actor)
    {
        return CurrentState == MatchState.ServeReady && rotations.CanServe(team, actor);
    }

    public bool CanPlayRallyAction()
    {
        return CurrentState == MatchState.Rally;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!showDebugGui)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(10f, 10f, 260f, 315f), GUI.skin.box);
        GUILayout.Label($"State: {CurrentState}");
        GUILayout.Label($"Score: Player {PlayerScore} - {OpponentScore} Opponent");
        GUILayout.Label($"Serving: {ServingTeam}");
        GUILayout.Label($"Server: {GetServerDebugName()}");
        GUILayout.Label($"Receiving: {ReceivingTeam}");
        GUILayout.Label($"Touch: {CurrentTouchTeam} x {CurrentTouchCount}");
        GUILayout.Label($"Last Attack: {(rally.HasLastAttackTeam ? rally.LastAttackTeam.ToString() : "None")}");
        GUILayout.Label($"Last Action Failure: {GetLastActionFailureDebugText()}");

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

        if (PlayerScore < TargetScore - 1)
        {
            score.SetPlayerScore(TargetScore - 1);
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
                BallServed(ServingTeam);
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

    private string GetServerDebugName()
    {
        CharacterRole server = CurrentServer;
        if (server == null)
        {
            return "None";
        }

        if (server.PlayerData != null && !string.IsNullOrWhiteSpace(server.PlayerData.PlayerName))
        {
            return server.PlayerData.PlayerName;
        }

        return server.name;
    }

    private string GetLastActionFailureDebugText()
    {
        if (playerActionController == null)
        {
            playerActionController = FindFirstObjectByType<PlayerActionController>();
        }

        if (playerActionController == null || string.IsNullOrWhiteSpace(playerActionController.LastActionFailure))
        {
            return "None";
        }

        return playerActionController.LastActionFailure;
    }
#endif

    private bool IsMatchOver()
    {
        return score.IsMatchOver();
    }

    private TeamSide GetLandingScoringTeam(bool isIn, TeamSide landingTeam)
    {
        if (!isIn)
        {
            return GetOppositeTeam(CurrentTouchTeam);
        }

        return landingTeam == ReceivingTeam ? CurrentTouchTeam : GetOppositeTeam(CurrentTouchTeam);
    }

    private void RecordAttackResultIfNeeded(TeamSide scoringTeam)
    {
        if (!rally.HasLastAttackTeam)
        {
            return;
        }

        matchStats.RecordAttackResult(rally.LastAttackActor, scoringTeam == rally.LastAttackTeam);
    }

    private void UpdateStateAfterPoint(bool isMatchOver)
    {
        ChangeState(isMatchOver ? MatchState.MatchOver : MatchState.PointScored);
    }

    private void SaveFinalScore()
    {
        matchStats.SetFinalScore(PlayerScore, OpponentScore);
    }

    private void HandleMatchOver()
    {
        CancelInvoke(nameof(ResetRally));
        CancelInvoke(nameof(AutoServeCurrentServer));
        AdvanceCareerCalendarAfterMatch();
        ShowMatchEndResult();
    }

    private void StopBall()
    {
        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null)
        {
            return;
        }

        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;
        ballBody.Sleep();
    }

    private void ScheduleAutoServeIfNeeded()
    {
        CharacterRole server = CurrentServer;
        if (server == null || server.GetComponent<Player>() != null)
        {
            return;
        }

        Invoke(nameof(AutoServeCurrentServer), autoServeDelay);
    }

    private void AutoServeCurrentServer()
    {
        CharacterRole server = CurrentServer;
        if (server == null || CurrentState != MatchState.ServeReady)
        {
            return;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null)
        {
            return;
        }

        Vector3 serveDirection = ServingTeam == TeamSide.Player ? Vector3.forward : Vector3.back;
        Vector3 spawnPosition = server.transform.position
            + server.transform.right * autoServeSpawnOffset.x
            + Vector3.up * autoServeSpawnOffset.y
            + serveDirection * autoServeSpawnOffset.z;

        ballBody.position = spawnPosition;
        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;
        ballBody.linearVelocity = serveDirection * autoServeForwardSpeed + Vector3.up * autoServeUpSpeed;
        BallServed(ServingTeam, server);
    }

    private Rigidbody GetBallRigidbody()
    {
        GameObject ballObject = GameObject.Find(ballName);
        return ballObject != null ? ballObject.GetComponent<Rigidbody>() : null;
    }

    private void AdvanceCareerCalendarAfterMatch()
    {
        if (!matchEnd.TryMarkCareerCalendarAdvanced())
        {
            return;
        }

        if (careerCalendarManager != null)
        {
            careerCalendarManager.AdvanceAfterMatch();
        }
    }

    private void ShowMatchEndResult()
    {
        if (!matchEnd.TryMarkTrainingSelectionShown())
        {
            return;
        }

        if (gameFlowManager != null)
        {
            gameFlowManager.OnMatchEnded();
        }
        else if (trainingSelectionUI != null)
        {
            trainingSelectionUI.ShowRandomOptions();
        }
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
