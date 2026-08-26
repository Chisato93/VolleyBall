using UnityEngine;

public class TeamReceiveController : MonoBehaviour
{
    [SerializeField] private TeamSide controlledTeam = TeamSide.Opponent;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float trackingRange = 12f;
    [SerializeField] private float receiveRange = 1.5f;
    [SerializeField] private float maxReceiveHeight = 2.5f;
    [SerializeField] private float receiveForwardSpeed = 5f;
    [SerializeField] private float receiveUpSpeed = 4f;
    [SerializeField] private float receiveCooldown = 0.75f;

    private MatchManager matchManager;
    private Transform ball;
    private Rigidbody ballBody;
    private CharacterRole receiver;
    private float nextReceiveTime;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        EnsureReferences();

        if (!CanReceiveNow())
        {
            receiver = null;
            return;
        }

        receiver = FindNearestReceiver();
        if (receiver == null)
        {
            return;
        }

        MoveReceiverTowardBall(receiver.transform);
        TryReceive(receiver);
    }

    private void EnsureReferences()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (ball == null)
        {
            GameObject ballObject = GameObject.Find("volleyball");
            ball = ballObject != null ? ballObject.transform : null;
        }

        if (ballBody == null && ball != null)
        {
            ballBody = ball.GetComponent<Rigidbody>();
        }
    }

    private bool CanReceiveNow()
    {
        if (matchManager == null || ball == null || ballBody == null)
        {
            return false;
        }

        if (!matchManager.CanPlayRallyAction())
        {
            return false;
        }

        if (matchManager.CurrentTouchTeam == controlledTeam)
        {
            return false;
        }

        return matchManager.ReceivingTeam == controlledTeam;
    }

    private CharacterRole FindNearestReceiver()
    {
        CharacterRole[] roles = FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);
        CharacterRole nearest = null;
        float nearestDistance = trackingRange;

        foreach (CharacterRole role in roles)
        {
            if (role.TeamSide != controlledTeam)
            {
                continue;
            }

            if (controlledTeam == TeamSide.Player && role.GetComponent<Player>() != null)
            {
                continue;
            }

            float distance = GetXzDistance(role.transform.position, ball.position);
            if (distance <= nearestDistance)
            {
                nearest = role;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void MoveReceiverTowardBall(Transform receiverTransform)
    {
        Vector3 currentPosition = receiverTransform.position;
        Vector3 targetPosition = new Vector3(ball.position.x, currentPosition.y, ball.position.z);
        receiverTransform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime);

        Vector3 lookDirection = targetPosition - currentPosition;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            receiverTransform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    private void TryReceive(CharacterRole currentReceiver)
    {
        if (Time.time < nextReceiveTime)
        {
            return;
        }

        if (ball.position.y > maxReceiveHeight)
        {
            return;
        }

        if (GetXzDistance(currentReceiver.transform.position, ball.position) > receiveRange)
        {
            return;
        }

        if (!matchManager.RegisterTouch(controlledTeam, BallTouchAction.Receive))
        {
            return;
        }

        ballBody.linearVelocity = GetReceiveDirection() * receiveForwardSpeed + Vector3.up * receiveUpSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextReceiveTime = Time.time + receiveCooldown;
    }

    private Vector3 GetReceiveDirection()
    {
        return controlledTeam == TeamSide.Opponent ? Vector3.back : Vector3.forward;
    }

    private static float GetXzDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
