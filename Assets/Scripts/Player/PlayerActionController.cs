using UnityEngine;

[RequireComponent(typeof(SpikeTargetingController))]
public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private float serveForwardSpeed = 8f;
    [SerializeField] private float serveUpSpeed = 4f;
    [SerializeField] private Vector3 serveSpawnOffset = new Vector3(0f, 1.2f, 1f);
    [SerializeField] private float spikeRange = 2.5f;
    [SerializeField] private float minSpikeHeight = 2f;
    [SerializeField] private float maxSpikeHeight = 6f;
    [SerializeField] private float spikeForwardSpeed = 8f;
    [SerializeField] private float spikeDownSpeed = 3f;
    [SerializeField] private float spikeCooldown = 0.75f;
    [SerializeField] private float receiveRange = 2.2f;
    [SerializeField] private float maxReceiveHeight = 2.5f;
    [SerializeField] private float receiveForwardSpeed = 5f;
    [SerializeField] private float receiveUpSpeed = 4f;
    [SerializeField] private float receiveCooldown = 0.75f;
    [SerializeField] private float setRange = 2f;
    [SerializeField] private float minSetHeight = 1f;
    [SerializeField] private float maxSetHeight = 5f;
    [SerializeField] private float setForwardSpeed = 3f;
    [SerializeField] private float setUpSpeed = 6f;
    [SerializeField] private float setCooldown = 0.75f;
    [SerializeField] private float setNetFrontDistance = 1.5f;
    [SerializeField] private bool showSpikeRange = true;
    [SerializeField] private Color spikeRangeColor = new Color(0f, 0.75f, 1f, 0.7f);
    [SerializeField] private Color spikeReadyColor = new Color(0f, 1f, 0.25f, 0.9f);
    [SerializeField] private SpikeTargetingController spikeTargetingController;
    [SerializeField] private PlayerCameraController playerCameraController;

    private CharacterRole characterRole;
    private float nextReceiveTime;
    private float nextSetTime;
    private float nextSpikeTime;
    private string lastActionFailure = "";

    public string LastActionFailure => lastActionFailure;

    private void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (spikeTargetingController == null)
        {
            spikeTargetingController = GetComponent<SpikeTargetingController>();
        }

        if (playerCameraController == null)
        {
            playerCameraController = PlayerCameraController.FindSceneController(transform);
        }

        if (characterRole == null)
        {
            characterRole = GetComponent<CharacterRole>();
        }

        if (ball == null)
        {
            GameObject ballObject = GameObject.Find("volleyball");
            ball = ballObject != null ? ballObject.transform : null;
        }

        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }
    }

    public void HandleActionQ()
    {
        lastActionFailure = "";

        if (TryServe())
        {
            ClearActionFailure();
            return;
        }

        if (matchManager != null && matchManager.CurrentState == MatchState.ServeReady)
        {
            return;
        }

        if (TryReceive())
        {
            ClearActionFailure();
            return;
        }

        if (TrySet())
        {
            ClearActionFailure();
            return;
        }

        if (TrySpike())
        {
            ClearActionFailure();
            return;
        }

        TryBlock();
    }

    private bool TryReceive()
    {
        EnsureReferences();

        TeamSide currentTeamSide = GetTeamSide();
        if (!CanUseRallyAction())
        {
            SetActionFailure("Receive failed: rally is not active.");
            return false;
        }

        if (matchManager.CurrentTouchTeam == currentTeamSide)
        {
            SetActionFailure("Receive failed: your team touched last.");
            return false;
        }

        if (matchManager.ReceivingTeam != currentTeamSide && !IsBallOnTeamCourt(currentTeamSide))
        {
            SetActionFailure("Receive failed: ball is not on your court.");
            return false;
        }

        if (Time.time < nextReceiveTime)
        {
            SetActionFailure("Receive failed: cooldown.");
            return false;
        }

        if (ball == null)
        {
            SetActionFailure("Receive failed: ball not found.");
            return false;
        }

        if (ball.position.y > maxReceiveHeight)
        {
            SetActionFailure("Receive failed: ball is too high.");
            return false;
        }

        if (GetXzDistance(transform.position, ball.position) > receiveRange)
        {
            SetActionFailure("Receive failed: ball is too far.");
            return false;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null || !matchManager.RegisterTouch(currentTeamSide, BallTouchAction.Receive, characterRole))
        {
            SetActionFailure("Receive failed: touch was rejected.");
            return false;
        }

        ballBody.linearVelocity = GetReceiveDirection(currentTeamSide) * receiveForwardSpeed + Vector3.up * receiveUpSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextReceiveTime = Time.time + receiveCooldown;
        ShowActionCamera();
        return true;
    }

    private bool TrySet()
    {
        EnsureReferences();

        TeamSide currentTeamSide = GetTeamSide();
        if (!CanUseRallyAction())
        {
            SetActionFailure("Set failed: rally is not active.");
            return false;
        }

        if (matchManager.CurrentTouchTeam != currentTeamSide || matchManager.ReceivingTeam != currentTeamSide)
        {
            SetActionFailure("Set failed: your team cannot set now.");
            return false;
        }

        if (matchManager.CurrentTouchCount != 1)
        {
            SetActionFailure($"Set failed: touch count is {matchManager.CurrentTouchCount}.");
            return false;
        }

        if (Time.time < nextSetTime || ball == null || ball.position.y < minSetHeight || ball.position.y > maxSetHeight)
        {
            SetActionFailure("Set failed: timing or height is wrong.");
            return false;
        }

        if (GetXzDistance(transform.position, ball.position) > setRange)
        {
            SetActionFailure("Set failed: ball is too far.");
            return false;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null || !matchManager.RegisterTouch(currentTeamSide, BallTouchAction.Set, characterRole))
        {
            SetActionFailure("Set failed: touch was rejected.");
            return false;
        }

        Vector3 setDirection = GetSetTarget(currentTeamSide) - ball.position;
        setDirection.y = 0f;
        setDirection = setDirection.sqrMagnitude > 0.001f ? setDirection.normalized : GetReceiveDirection(currentTeamSide);

        ballBody.linearVelocity = setDirection * setForwardSpeed + Vector3.up * setUpSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextSetTime = Time.time + setCooldown;
        ShowActionCamera();
        return true;
    }

    private bool TrySpike()
    {
        EnsureReferences();

        TeamSide currentTeamSide = GetTeamSide();
        if (ball == null)
        {
            SetActionFailure("Spike failed: ball not found.");
            return false;
        }

        if (!CanUseRallyAction())
        {
            SetActionFailure("Spike failed: rally is not active.");
            return false;
        }

        if (matchManager.CurrentTouchTeam != currentTeamSide || matchManager.ReceivingTeam != currentTeamSide)
        {
            SetActionFailure("Spike failed: your team cannot spike now.");
            return false;
        }

        if (matchManager.CurrentTouchCount < 2 || matchManager.CurrentTouchCount > 3)
        {
            SetActionFailure($"Spike failed: touch count is {matchManager.CurrentTouchCount}.");
            return false;
        }

        if (Time.time < nextSpikeTime || ball.position.y < minSpikeHeight || ball.position.y > maxSpikeHeight)
        {
            SetActionFailure("Spike failed: timing or height is wrong.");
            return false;
        }

        if (!IsBallInSpikeRange())
        {
            SetActionFailure("Spike failed: ball is too far.");
            return false;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null || !matchManager.RegisterTouch(currentTeamSide, BallTouchAction.Spike, characterRole))
        {
            SetActionFailure("Spike failed: touch was rejected.");
            return false;
        }

        ballBody.linearVelocity = GetAttackDirection(currentTeamSide) * spikeForwardSpeed + Vector3.down * spikeDownSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextSpikeTime = Time.time + spikeCooldown;
        ShowActionCamera();
        return true;
    }

    private bool TryBlock()
    {
        // TODO: 네트 근처이며, 수비 상황이고, 상대가 공격할 때 Block을 실행한다.
        return false;
    }

    private bool TryServe()
    {
        // TODO: 서브 위치일 때 Serve를 실행한다.
        EnsureReferences();

        TeamSide currentTeamSide = GetTeamSide();
        if (matchManager == null || !matchManager.CanServe(currentTeamSide, characterRole))
        {
            if (matchManager != null && matchManager.CurrentState == MatchState.ServeReady)
            {
                SetActionFailure("Serve failed: this player is not the server.");
            }

            return false;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null)
        {
            SetActionFailure("Serve failed: ball not found.");
            return false;
        }

        Vector3 spawnPosition = transform.position
            + transform.right * serveSpawnOffset.x
            + Vector3.up * serveSpawnOffset.y
            + transform.forward * serveSpawnOffset.z;

        ballBody.position = spawnPosition;
        ballBody.linearVelocity = Vector3.zero;
        ballBody.angularVelocity = Vector3.zero;
        ballBody.linearVelocity = transform.forward * serveForwardSpeed + Vector3.up * serveUpSpeed;

        bool served = matchManager.RegisterTouch(currentTeamSide, BallTouchAction.Serve, characterRole);
        if (served)
        {
            ShowActionCamera();
        }

        return served;
    }

    private void ShowActionCamera()
    {
        if (playerCameraController != null)
        {
            playerCameraController.UseActionCamera();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showSpikeRange)
        {
            return;
        }

        Gizmos.color = IsBallInSpikeRange() ? spikeReadyColor : spikeRangeColor;
        Gizmos.DrawWireSphere(transform.position, spikeRange);
    }

    private bool IsBallInSpikeRange()
    {
        Transform targetBall = ball;
        if (targetBall == null)
        {
            GameObject ballObject = GameObject.Find("volleyball");
            targetBall = ballObject != null ? ballObject.transform : null;
        }

        return targetBall != null && GetXzDistance(transform.position, targetBall.position) <= spikeRange;
    }

    private bool CanUseRallyAction()
    {
        EnsureReferences();
        return matchManager != null && matchManager.CanPlayRallyAction();
    }

    private Rigidbody GetBallRigidbody()
    {
        EnsureReferences();
        return ball != null ? ball.GetComponent<Rigidbody>() : null;
    }

    private TeamSide GetTeamSide()
    {
        return characterRole != null ? characterRole.TeamSide : teamSide;
    }

    private bool IsBallOnTeamCourt(TeamSide currentTeamSide)
    {
        if (ball == null)
        {
            return false;
        }

        return currentTeamSide == TeamSide.Player ? ball.position.z <= 0f : ball.position.z >= 0f;
    }

    private void SetActionFailure(string message)
    {
        lastActionFailure = message;
    }

    private void ClearActionFailure()
    {
        lastActionFailure = "";
    }

    private static Vector3 GetReceiveDirection(TeamSide currentTeamSide)
    {
        return currentTeamSide == TeamSide.Opponent ? Vector3.back : Vector3.forward;
    }

    private static Vector3 GetAttackDirection(TeamSide currentTeamSide)
    {
        return currentTeamSide == TeamSide.Opponent ? Vector3.back : Vector3.forward;
    }

    private static float GetXzDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private Vector3 GetSetTarget(TeamSide currentTeamSide)
    {
        float targetZ = currentTeamSide == TeamSide.Player ? -setNetFrontDistance : setNetFrontDistance;
        return new Vector3(0f, transform.position.y, targetZ);
    }
}
