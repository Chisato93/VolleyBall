using UnityEngine;

public class TeamSetController : MonoBehaviour
{
    [SerializeField] private TeamSide controlledTeam = TeamSide.Opponent;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float trackingRange = 12f;
    [SerializeField] private float setRange = 2f;
    [SerializeField] private float minSetHeight = 1f;
    [SerializeField] private float maxSetHeight = 5f;
    [SerializeField] private float setForwardSpeed = 3f;
    [SerializeField] private float setUpSpeed = 6f;
    [SerializeField] private float setCooldown = 0.75f;
    [SerializeField] private float setNetFrontDistance = 1.5f;

    private MatchManager matchManager;
    private Transform ball;
    private Rigidbody ballBody;
    private CharacterRole setter;
    private float nextSetTime;

    private void Awake()
    {
        EnsureReferences();
    }

    private void Update()
    {
        EnsureReferences();

        if (!CanSetNow())
        {
            setter = null;
            return;
        }

        setter = FindSetter();
        if (setter == null)
        {
            return;
        }

        MoveSetterTowardBall(setter.transform);
        TrySet(setter);
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

    private bool CanSetNow()
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

        return matchManager.CurrentTouchCount == 1;
    }

    private CharacterRole FindSetter()
    {
        CharacterRole[] roles = FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);
        CharacterRole nearest = null;
        CharacterRole nearestSetter = null;
        float nearestDistance = trackingRange;
        float nearestSetterDistance = trackingRange;

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

            if (role.Position == POSITION.Setter && distance <= nearestSetterDistance)
            {
                nearestSetter = role;
                nearestSetterDistance = distance;
            }
        }

        return nearestSetter != null ? nearestSetter : nearest;
    }

    private void MoveSetterTowardBall(Transform setterTransform)
    {
        Vector3 currentPosition = setterTransform.position;
        Vector3 targetPosition = new Vector3(ball.position.x, currentPosition.y, ball.position.z);
        setterTransform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime);

        Vector3 lookDirection = targetPosition - currentPosition;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            setterTransform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }

    private void TrySet(CharacterRole currentSetter)
    {
        if (Time.time < nextSetTime)
        {
            return;
        }

        if (ball.position.y < minSetHeight || ball.position.y > maxSetHeight)
        {
            return;
        }

        if (GetXzDistance(currentSetter.transform.position, ball.position) > setRange)
        {
            return;
        }

        if (!matchManager.RegisterTouch(controlledTeam, BallTouchAction.Set, currentSetter))
        {
            return;
        }

        Vector3 setDirection = GetSetTarget() - ball.position;
        setDirection.y = 0f;
        setDirection = setDirection.sqrMagnitude > 0.001f ? setDirection.normalized : GetReceiveDirection();

        ballBody.linearVelocity = setDirection * setForwardSpeed + Vector3.up * setUpSpeed;
        ballBody.angularVelocity = Vector3.zero;
        nextSetTime = Time.time + setCooldown;
    }

    private Vector3 GetSetTarget()
    {
        float targetZ = controlledTeam == TeamSide.Player ? -setNetFrontDistance : setNetFrontDistance;
        return new Vector3(0f, 0f, targetZ);
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
