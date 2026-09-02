using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float courtHalfWidth = 5f;
    [SerializeField] private float courtHalfLength = 10f;
    [SerializeField] private float netBuffer = 0.35f;

    private Rigidbody body;
    private CharacterRole characterRole;
    private Vector2 moveInput;

    public bool IsGroundedNow => IsGrounded();

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        characterRole = GetComponent<CharacterRole>();
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        var moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float currentMoveSpeed = CharacterActionStatCalculator.GetMoveSpeed(characterRole, moveSpeed);
        Vector3 horizontalVelocity = GetCourtLimitedVelocity(moveDirection * currentMoveSpeed);
        body.linearVelocity = new Vector3(horizontalVelocity.x, body.linearVelocity.y, horizontalVelocity.z);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }

    public void Move(Vector2 input)
    {
        moveInput = input;
    }

    public void Rotate(Vector2 input)
    {
        Vector3 lookDirection = new Vector3(input.x, 0f, input.y);
        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        if (lookDirection.sqrMagnitude > 1f)
        {
            lookDirection.Normalize();
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }

    public bool Jump()
    {
        if (!IsGrounded())
        {
            return false;
        }

        float currentJumpForce = CharacterActionStatCalculator.GetJumpForce(characterRole, jumpForce);
        body.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        return true;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f, groundMask);
        }

        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetCourtLimitedVelocity(Vector3 horizontalVelocity)
    {
        if (characterRole == null)
        {
            return horizontalVelocity;
        }

        Vector3 nextPosition = body.position + horizontalVelocity * Time.fixedDeltaTime;
        Vector3 clampedPosition = ClampToOwnCourt(nextPosition, characterRole.TeamSide);
        Vector3 limitedVelocity = (clampedPosition - body.position) / Time.fixedDeltaTime;
        return new Vector3(limitedVelocity.x, 0f, limitedVelocity.z);
    }

    private Vector3 ClampToOwnCourt(Vector3 position, TeamSide teamSide)
    {
        position.x = Mathf.Clamp(position.x, -courtHalfWidth, courtHalfWidth);

        if (teamSide == TeamSide.Player)
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
