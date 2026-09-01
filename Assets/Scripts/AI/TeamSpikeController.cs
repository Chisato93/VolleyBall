using UnityEngine;

public class TeamSpikeController : MonoBehaviour
{
    [SerializeField] private TeamSide controlledTeam = TeamSide.Opponent;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float trackingRange = 12f;
    [SerializeField] private float spikeRange = 2f;
    [SerializeField] private float minSpikeHeight = 2f;
    [SerializeField] private float maxSpikeHeight = 6f;
    [SerializeField] private float spikeForwardSpeed = 8f;
    [SerializeField] private float spikeDownSpeed = 3f;
    [SerializeField] private float spikeCooldown = 0.75f;
    [SerializeField] private float courtHalfWidth = 5f;
    [SerializeField] private float courtHalfLength = 10f;
    [SerializeField] private float netBuffer = 0.35f;

    private MatchManager matchManager;
    private Transform ball;
    private Rigidbody ballBody;
    private CharacterRole attacker;
    private float nextSpikeTime;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        EnsureReferences();

        if (!CanSpikeNow())
        {
            attacker = null;
            return;
        }

        attacker = FindAttacker();
        if (attacker == null)
        {
            return;
        }

        MoveAttackerTowardBall(attacker.transform);
        TrySpike(attacker);
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

    private bool CanSpikeNow()
    {
        if (matchManager == null || ball == null || ballBody == null)
        {
            return false;
        }

        if (!matchManager.CanPlayRallyAction())
        {
            return false;
        }

        if (matchManager.CurrentTouchTeam != controlledTeam || matchManager.ReceivingTeam != controlledTeam)
        {
            return false;
        }

        return matchManager.CurrentTouchCount == 2;
    }

    private CharacterRole FindAttacker()
    {
        CharacterRole[] roles = FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);
        CharacterRole nearest = null;
        CharacterRole nearestAttacker = null;
        float nearestDistance = trackingRange;
        float nearestAttackerDistance = trackingRange;

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
            if (distance > trackingRange)
            {
                continue;
            }

            if (distance <= nearestDistance)
            {
                nearest = role;
                nearestDistance = distance;
            }

            if (IsAttackPosition(role.Position) && distance <= nearestAttackerDistance)
            {
                nearestAttacker = role;
                nearestAttackerDistance = distance;
            }
        }

        return nearestAttacker != null ? nearestAttacker : nearest;
    }

    private void MoveAttackerTowardBall(Transform attackerTransform)
    {
        Vector3 currentPosition = attackerTransform.position;
        Vector3 targetPosition = new Vector3(ball.position.x, currentPosition.y, ball.position.z);
        Vector3 nextPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime);
        attackerTransform.position = ClampToOwnCourt(nextPosition);

        Vector3 lookDirection = targetPosition - currentPosition;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            attackerTransform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    private void TrySpike(CharacterRole currentAttacker)
    {
        if (Time.time < nextSpikeTime)
        {
            return;
        }

        if (ball.position.y < minSpikeHeight || ball.position.y > maxSpikeHeight)
        {
            return;
        }

        if (GetXzDistance(currentAttacker.transform.position, ball.position) > spikeRange)
        {
            return;
        }

        if (!matchManager.RegisterTouch(controlledTeam, BallTouchAction.Spike, currentAttacker))
        {
            return;
        }

        ballBody.linearVelocity = GetAttackDirection() * spikeForwardSpeed + Vector3.down * spikeDownSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextSpikeTime = Time.time + spikeCooldown;
    }

    private Vector3 GetAttackDirection()
    {
        return controlledTeam == TeamSide.Opponent ? Vector3.back : Vector3.forward;
    }

    private static bool IsAttackPosition(POSITION position)
    {
        return position == POSITION.OutsideHitter
            || position == POSITION.OppositeHitter
            || position == POSITION.MiddleBlocker;
    }

    private static float GetXzDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private Vector3 ClampToOwnCourt(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -courtHalfWidth, courtHalfWidth);

        if (controlledTeam == TeamSide.Player)
        {
            position.z = Mathf.Clamp(position.z, -courtHalfLength, -netBuffer);
        }
        else
        {
            position.z = Mathf.Clamp(position.z, netBuffer, courtHalfLength);
        }

        return position;
    }
}
