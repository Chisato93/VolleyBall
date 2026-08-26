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
    [SerializeField] private float spikeRange = 2f;
    [SerializeField] private bool showSpikeRange = true;
    [SerializeField] private Color spikeRangeColor = new Color(0f, 0.75f, 1f, 0.7f);
    [SerializeField] private Color spikeReadyColor = new Color(0f, 1f, 0.25f, 0.9f);
    [SerializeField] private SpikeTargetingController spikeTargetingController;

    private CharacterRole characterRole;

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
        if (TryServe())
        {
            return;
        }

        if (TrySpike())
        {
            return;
        }

        if (TryReceive())
        {
            return;
        }

        if (TrySet())
        {
            return;
        }

        TryBlock();
    }

    private bool TryReceive()
    {
        // TODO: 공이 낮으며, 수비 상황일 때 Receive를 실행한다.
        if (!CanUseRallyAction())
        {
            return false;
        }

        return matchManager.RegisterTouch(GetTeamSide(), BallTouchAction.Receive);
    }

    private bool TrySet()
    {
        // TODO: 공이 높으며, 플레이어가 세터일 경우 Set을 실행한다.
        return false;
    }

    private bool TrySpike()
    {
        // TODO: 공격 턴인지 확인한다.
        // TODO: 공이 높은 상태인지 확인한다.
        // TODO: 실제 스파이크 동작은 타격 지점 선택이 끝난 뒤 실행한다.
        EnsureReferences();

        if (ball == null || spikeTargetingController == null)
        {
            return false;
        }

        if (!CanUseRallyAction())
        {
            return false;
        }

        if (!IsBallInSpikeRange())
        {
            return false;
        }

        matchManager.RegisterTouch(GetTeamSide(), BallTouchAction.Spike);
        spikeTargetingController.EnterTargetingMode();
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
        if (matchManager == null || !matchManager.CanServe(currentTeamSide))
        {
            return false;
        }

        Rigidbody ballBody = GetBallRigidbody();
        if (ballBody == null)
        {
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

        return matchManager.RegisterTouch(currentTeamSide, BallTouchAction.Serve);
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

        return targetBall != null && Vector3.Distance(transform.position, targetBall.position) <= spikeRange;
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
}
