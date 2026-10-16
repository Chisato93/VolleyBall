using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class MatchSimulationManager : MonoBehaviour
{
    private const string CommentaryResourcePath = "MatchCommentary/commentary_events";
    private const int DefaultStatValue = 20;

    [SerializeField] private MatchManager matchManager;
    [SerializeField] private MatchCommentaryUI commentaryUI;
    [SerializeField] private int pointsToWinSet = 11;
    [SerializeField] private int totalSets = 3;
    [SerializeField] private float minEventDelay = 0.8f;
    [SerializeField] private float maxEventDelay = 2f;
    [SerializeField, Min(1f)] private float fastForwardMultiplier = 4f;
    [SerializeField] private string playerTeamName = "Player";
    [SerializeField] private string opponentTeamName = "Opponent";

    private readonly System.Random timingRandom = new System.Random();
    private readonly Queue<MatchCommentaryMessage> pendingLines = new Queue<MatchCommentaryMessage>();
    private readonly MatchStats simulatedStats = new MatchStats();
    private readonly List<SimPlayer> playerTeam = new List<SimPlayer>();
    private readonly List<SimPlayer> opponentTeam = new List<SimPlayer>();

    private MatchCommentaryCatalog commentaryCatalog = new MatchCommentaryCatalog();
    private TeamSide servingTeam = TeamSide.Player;
    private int currentSet = 1;
    private int playerScore;
    private int opponentScore;
    private int playerSetWins;
    private int opponentSetWins;
    private int totalPlayerPoints;
    private int totalOpponentPoints;
    private int playerServerIndex;
    private int opponentServerIndex;
    private float nextEventTime;
    private bool isFastForward;
    private float currentCommentarySpeed = 1f;
    private bool isFinished;
    private bool shouldFinishAfterPendingLines;

    private void Awake()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (commentaryUI == null)
        {
            commentaryUI = FindFirstObjectByType<MatchCommentaryUI>();
        }
        LoadCommentaryCatalog();
        BuildTeams();
    }

    private void Start()
    {
        simulatedStats.Reset();
        simulatedStats.BeginCommentaryEvaluation();
        if (matchManager != null)
        {
            matchManager.PrepareForTextSimulation();
        }

        if (commentaryUI != null)
        {
            commentaryUI.ResetUi();
            commentaryUI.SetPlaybackSpeed(currentCommentarySpeed);
            commentaryUI.SetScore(currentSet, playerScore, opponentScore, playerSetWins, opponentSetWins);

            if (commentaryUI.SpeedButton != null)
            {
                commentaryUI.SpeedButton.onClick.RemoveListener(ToggleCommentarySpeed);
                commentaryUI.SpeedButton.onClick.AddListener(ToggleCommentarySpeed);
            }

            if (commentaryUI.SkipMatchButton != null)
            {
                commentaryUI.SkipMatchButton.onClick.RemoveListener(SkipMatch);
                commentaryUI.SkipMatchButton.onClick.AddListener(SkipMatch);
            }
        }

        EnqueueSetStart();
        ScheduleNextEvent();
    }

    private void Update()
    {
        if (isFinished)
        {
            return;
        }

        RefreshCommentarySpeed();
        if (Time.time < nextEventTime)
        {
            return;
        }

        if (pendingLines.Count == 0)
        {
            if (shouldFinishAfterPendingLines)
            {
                if (commentaryUI != null && commentaryUI.isActiveAndEnabled && !commentaryUI.IsPlaybackComplete)
                {
                    return;
                }

                FinishMatch();
                return;
            }

            GenerateNextRally();
        }

        if (pendingLines.Count > 0)
        {
            commentaryUI?.AddLine(pendingLines.Dequeue());
        }

        ScheduleNextEvent();
    }

    private void OnDestroy()
    {
        if (commentaryUI != null)
        {
            if (commentaryUI.SpeedButton != null)
            {
                commentaryUI.SpeedButton.onClick.RemoveListener(ToggleCommentarySpeed);
            }

            if (commentaryUI.SkipMatchButton != null)
            {
                commentaryUI.SkipMatchButton.onClick.RemoveListener(SkipMatch);
            }
        }
    }

    private void ToggleCommentarySpeed()
    {
        if (isFinished)
        {
            return;
        }

        isFastForward = !isFastForward;
        RefreshCommentarySpeed();
    }

    private void RefreshCommentarySpeed()
    {
        float speed = isFastForward ? Mathf.Max(1f, fastForwardMultiplier) : 1f;
        if (Mathf.Approximately(speed, currentCommentarySpeed))
        {
            return;
        }

        float remainingDelay = Mathf.Max(0f, nextEventTime - Time.time);
        nextEventTime = Time.time + remainingDelay * currentCommentarySpeed / speed;
        currentCommentarySpeed = speed;
        commentaryUI?.SetPlaybackSpeed(currentCommentarySpeed);
    }

    private void SkipMatch()
    {
        if (isFinished)
        {
            return;
        }

        while (!isFinished)
        {
            if (pendingLines.Count == 0)
            {
                if (shouldFinishAfterPendingLines)
                {
                    FinishMatch();
                    break;
                }

                GenerateNextRally();
            }

            pendingLines.Clear();
        }
    }

    private void GenerateNextRally()
    {
        if (currentSet > totalSets)
        {
            TeamSide matchWinner = playerSetWins > opponentSetWins ? TeamSide.Player : TeamSide.Opponent;
            EnqueuePendingLine(Format("Match", "End", matchWinner, null, null));
            shouldFinishAfterPendingLines = true;
            return;
        }

        SimPlayer server = GetServer(servingTeam);
        TeamSide receivingTeam = GetOppositeTeam(servingTeam);
        SimPlayer receiver = PickPlayer(receivingTeam, POSITION.Libero, POSITION.DefensiveSpecialist, POSITION.OutsideHitter);
        SimPlayer setter = PickPlayer(receivingTeam, POSITION.Setter);
        SimPlayer attacker = PickPlayer(receivingTeam, POSITION.OutsideHitter, POSITION.OppositeHitter, POSITION.MiddleBlocker);
        SimPlayer blocker = PickPlayer(servingTeam, POSITION.MiddleBlocker, POSITION.OppositeHitter);

        simulatedStats.RecordNamedTouch(server.Name, servingTeam, BallTouchAction.Serve);
        EnqueuePendingLine(Format("Serve", "Start", servingTeam, server, null));

        if (!RollAction(server, null, PlayerStatType.Serve, PlayerStatType.Mental, 0.72f))
        {
            EnqueuePendingLine(Format("Serve", "Fail", servingTeam, server, null));
            AwardPoint(receivingTeam, null);
            return;
        }

        simulatedStats.RecordNamedTouch(receiver.Name, receivingTeam, BallTouchAction.Receive);
        EnqueuePendingLine(Format("Receive", "Start", receivingTeam, receiver, server));

        if (!RollAction(receiver, server, PlayerStatType.Receive, PlayerStatType.Speed, 0.68f))
        {
            EnqueuePendingLine(Format("Receive", "Fail", receivingTeam, receiver, server));
            AwardPoint(servingTeam, server);
            return;
        }

        simulatedStats.RecordNamedTouch(setter.Name, receivingTeam, BallTouchAction.Set);
        EnqueuePendingLine(Format("Toss", "Start", receivingTeam, setter, null));

        if (!RollAction(setter, null, PlayerStatType.Set, PlayerStatType.Mental, 0.78f))
        {
            EnqueuePendingLine(Format("Toss", "Fail", receivingTeam, setter, null));
            AwardPoint(servingTeam, null);
            return;
        }

        simulatedStats.RecordNamedTouch(attacker.Name, receivingTeam, BallTouchAction.Spike);
        EnqueuePendingLine(Format("Attack", "Start", receivingTeam, attacker, blocker));

        bool blockSuccess = RollAction(blocker, attacker, PlayerStatType.Block, PlayerStatType.Jump, 0.28f);
        if (blockSuccess)
        {
            simulatedStats.RecordNamedTouch(blocker.Name, servingTeam, BallTouchAction.Block);
            simulatedStats.RecordNamedAttackResult(attacker.Name, receivingTeam, false);
            EnqueuePendingLine(Format("Block", "Success", servingTeam, blocker, attacker));
            AwardPoint(servingTeam, blocker);
            return;
        }

        bool attackSuccess = RollAction(attacker, blocker, PlayerStatType.Power, PlayerStatType.Jump, 0.64f);
        simulatedStats.RecordNamedAttackResult(attacker.Name, receivingTeam, attackSuccess);

        if (attackSuccess)
        {
            EnqueuePendingLine(Format("Attack", "Success", receivingTeam, attacker, blocker));
            AwardPoint(receivingTeam, attacker);
        }
        else
        {
            EnqueuePendingLine(Format("Attack", "Fail", receivingTeam, attacker, blocker));
            AwardPoint(servingTeam, null);
        }
    }

    private void AwardPoint(TeamSide scoringTeam, SimPlayer scorer)
    {
        if (scoringTeam == TeamSide.Player)
        {
            playerScore++;
            totalPlayerPoints++;
        }
        else
        {
            opponentScore++;
            totalOpponentPoints++;
        }

        servingTeam = scoringTeam;
        AdvanceServer(scoringTeam);

        EnqueuePendingLine(Format("Point", "Score", scoringTeam, scorer, null));
        commentaryUI?.SetScore(currentSet, playerScore, opponentScore, playerSetWins, opponentSetWins);

        if (playerScore >= pointsToWinSet || opponentScore >= pointsToWinSet)
        {
            EndCurrentSet();
        }
    }

    private void EndCurrentSet()
    {
        TeamSide setWinner = playerScore > opponentScore ? TeamSide.Player : TeamSide.Opponent;
        if (setWinner == TeamSide.Player)
        {
            playerSetWins++;
        }
        else
        {
            opponentSetWins++;
        }

        EnqueuePendingLine(Format("Set", "End", setWinner, null, null));
        currentSet++;
        playerScore = 0;
        opponentScore = 0;

        if (currentSet > totalSets)
        {
            return;
        }

        EnqueueSetStart();
        commentaryUI?.SetScore(currentSet, playerScore, opponentScore, playerSetWins, opponentSetWins);
    }

    private void FinishMatch()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        commentaryUI?.SetScore(totalSets, playerScore, opponentScore, playerSetWins, opponentSetWins);

        simulatedStats.SetFinalScore(totalPlayerPoints, totalOpponentPoints);
        matchManager?.CompleteTextSimulation(totalPlayerPoints, totalOpponentPoints, simulatedStats);
    }

    private void EnqueueSetStart()
    {
        EnqueuePendingLine(Format("Set", "Start", servingTeam, null, null));
    }

    private void EnqueuePendingLine(MatchCommentaryMessage line)
    {
        pendingLines.Enqueue(line);
    }

    private bool RollAction(SimPlayer actor, SimPlayer opponent, PlayerStatType primaryStat, PlayerStatType secondaryStat, float baseChance)
    {
        float actorValue = GetAverageStat(actor, primaryStat, secondaryStat);
        float opponentValue = opponent != null ? GetAverageStat(opponent, primaryStat, secondaryStat) : DefaultStatValue;
        float chance = Mathf.Clamp(baseChance + (actorValue - opponentValue) * 0.01f, 0.18f, 0.92f);
        return UnityEngine.Random.value <= chance;
    }

    private float GetAverageStat(SimPlayer player, PlayerStatType primaryStat, PlayerStatType secondaryStat)
    {
        if (player == null || player.Stats == null)
        {
            return DefaultStatValue;
        }

        return (player.Stats.GetValue(primaryStat) + player.Stats.GetValue(secondaryStat)) * 0.5f;
    }

    private MatchCommentaryMessage Format(string phase, string result, TeamSide team, SimPlayer actor, SimPlayer target)
    {
        int rollValue = UnityEngine.Random.Range(-10, 11);
        Debug.Log($"roll result => {rollValue}");
        simulatedStats.RecordCommentaryRoll(rollValue);
        MatchCommentaryMessage template = commentaryCatalog.GetRandomTemplate(phase, result, rollValue);
        string teamName = GetTeamName(team);
        string opponentName = GetTeamName(GetOppositeTeam(team));
        string playerName = actor != null ? actor.Name : teamName;
        string targetName = target != null ? target.Name : opponentName;
        string score = $"{playerScore} : {opponentScore}";
        string setScore = $"{playerSetWins} : {opponentSetWins}";
        string teamLabel = team == TeamSide.Player ? "아군" : "상대";
        string opponentLabel = team == TeamSide.Player ? "상대" : "아군";

        string side = team == TeamSide.Player ? "left" : "right";
        string resource = actor != null ? "Character/Portraits/" + actor.Name : null;
        string targetSide = target != null ? (target.TeamSide == TeamSide.Player ? "left" : "right") : null;
        string targetResource = target != null ? "Character/Portraits/" + target.Name : null;

        string Resolve(string value)
        {
            if (value == null)
            {
                return null;
            }

            return value
                .Replace("{teamLabel}", teamLabel)
                .Replace("{opponentLabel}", opponentLabel)
                .Replace("{team}", teamName)
                .Replace("{opponent}", opponentName)
                .Replace("{player}", playerName)
                .Replace("{target}", targetName)
                .Replace("{side}", side ?? string.Empty)
                .Replace("{targetSide}", targetSide ?? string.Empty)
                .Replace("{resource}", resource ?? string.Empty)
                .Replace("{targetResource}", targetResource ?? string.Empty)
                .Replace("{set}", currentSet.ToString())
                .Replace("{score}", score)
                .Replace("{setScore}", setScore);
        }

        string[] parameters = template.parameters != null
            ? Array.ConvertAll(template.parameters, Resolve)
            : Array.Empty<string>();
        string text = Resolve(template.text);
        if (text != null && parameters.Length > 0)
        {
            // Replace in one pass so parameter values cannot become new placeholders.
            text = Regex.Replace(text, @"\{%([1-9][0-9]*)\}", match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int number) && number <= parameters.Length)
                {
                    return parameters[number - 1] ?? string.Empty;
                }

                return match.Value;
            });
        }

        string resolvedSide = Resolve(template.side);
        return new MatchCommentaryMessage
        {
            rollValue = rollValue,
            text = text,
            parameters = parameters,
            character = Resolve(template.character),
            side = string.IsNullOrWhiteSpace(resolvedSide) ? null : resolvedSide,
            resourcePath = Resolve(template.resourcePath)
        };
    }

    private void ScheduleNextEvent()
    {
        // 대기 시간 추첨이 경기와 평가의 랜덤 순서에 영향을 주지 않게 분리합니다.
        float delay = minEventDelay + (maxEventDelay - minEventDelay) * (float)timingRandom.NextDouble();
        nextEventTime = Time.time + delay / currentCommentarySpeed;
    }

    private void LoadCommentaryCatalog()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(CommentaryResourcePath);
        if (textAsset == null)
        {
            Debug.LogWarning($"중계 JSON을 찾지 못했습니다: Resources/{CommentaryResourcePath}.json");
            return;
        }

        commentaryCatalog = JsonUtility.FromJson<MatchCommentaryCatalog>(textAsset.text) ?? new MatchCommentaryCatalog();
    }

    private void BuildTeams()
    {
        playerTeam.Clear();
        opponentTeam.Clear();

        CharacterRole[] roles = FindObjectsByType<CharacterRole>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CharacterRole role in roles)
        {
            SimPlayer player = new SimPlayer(role);
            if (player.TeamSide == TeamSide.Player)
            {
                playerTeam.Add(player);
            }
            else
            {
                opponentTeam.Add(player);
            }
        }
    }

    private SimPlayer GetServer(TeamSide team)
    {
        List<SimPlayer> teamList = GetTeam(team);
        if (teamList.Count == 0)
        {
            return new SimPlayer(team, GetTeamName(team), POSITION.OutsideHitter);
        }

        int index = team == TeamSide.Player ? playerServerIndex : opponentServerIndex;
        return teamList[index % teamList.Count];
    }

    private void AdvanceServer(TeamSide team)
    {
        if (team == TeamSide.Player)
        {
            playerServerIndex++;
        }
        else
        {
            opponentServerIndex++;
        }
    }

    private SimPlayer PickPlayer(TeamSide team, params POSITION[] positions)
    {
        List<SimPlayer> teamList = GetTeam(team);
        foreach (POSITION position in positions)
        {
            List<SimPlayer> matches = teamList.FindAll(player => player.Position == position);
            if (matches.Count > 0)
            {
                return matches[UnityEngine.Random.Range(0, matches.Count)];
            }
        }

        if (teamList.Count > 0)
        {
            return teamList[UnityEngine.Random.Range(0, teamList.Count)];
        }

        return new SimPlayer(team, GetTeamName(team), POSITION.OutsideHitter);
    }

    private List<SimPlayer> GetTeam(TeamSide team)
    {
        return team == TeamSide.Player ? playerTeam : opponentTeam;
    }

    private string GetTeamName(TeamSide team)
    {
        return team == TeamSide.Player ? playerTeamName : opponentTeamName;
    }

    private static TeamSide GetOppositeTeam(TeamSide team)
    {
        return team == TeamSide.Player ? TeamSide.Opponent : TeamSide.Player;
    }

    [Serializable]
    private sealed class MatchCommentaryCatalog
    {
        public MatchCommentaryEntry[] events = Array.Empty<MatchCommentaryEntry>();

        public MatchCommentaryMessage GetRandomTemplate(string phase, string result, int rollValue)
        {
            List<MatchCommentaryMessage> candidates = new List<MatchCommentaryMessage>();
            List<MatchCommentaryMessage> fallbackCandidates = new List<MatchCommentaryMessage>();
            foreach (MatchCommentaryEntry entry in events ?? Array.Empty<MatchCommentaryEntry>())
            {
                if (entry != null && entry.phase == phase && entry.result == result && entry.templates != null)
                {
                    foreach (MatchCommentaryMessage template in entry.templates)
                    {
                        if (template != null && !string.IsNullOrWhiteSpace(template.text))
                        {
                            fallbackCandidates.Add(template);
                            if (template.rollValue == rollValue)
                            {
                                candidates.Add(template);
                            }
                        }
                    }
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"중계 JSON에 {phase}/{result}, 값 {rollValue}의 문구가 없습니다. 같은 상황의 문구로 대체합니다.");
                candidates = fallbackCandidates;
            }

            if (candidates.Count == 0)
            {
                return new MatchCommentaryMessage { text = $"{phase} {result}: {{team}} {{player}}" };
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
    }

    [Serializable]
    private sealed class MatchCommentaryEntry
    {
        public string id;
        public string phase;
        public string result;
        public MatchCommentaryMessage[] templates;
    }

    private sealed class SimPlayer
    {
        public readonly string Name;
        public readonly TeamSide TeamSide;
        public readonly POSITION Position;
        public readonly PlayerStats Stats;

        public SimPlayer(CharacterRole role)
        {
            TeamSide = role.TeamSide;
            Position = role.PlayerData != null ? role.PlayerData.Position : POSITION.OutsideHitter;
            Name = role.PlayerData != null && !string.IsNullOrWhiteSpace(role.PlayerData.PlayerName)
                ? role.PlayerData.PlayerName
                : role.name;
            Stats = role.Stats;
        }

        public SimPlayer(TeamSide teamSide, string name, POSITION position)
        {
            TeamSide = teamSide;
            Name = name;
            Position = position;
            Stats = new PlayerStats
            {
                Power = DefaultStatValue,
                Jump = DefaultStatValue,
                Receive = DefaultStatValue,
                Set = DefaultStatValue,
                Block = DefaultStatValue,
                Serve = DefaultStatValue,
                Speed = DefaultStatValue,
                Stamina = DefaultStatValue,
                Mental = DefaultStatValue
            };
        }
    }
}
