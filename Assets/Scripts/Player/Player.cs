using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputManager))]
[RequireComponent(typeof(PlayerActionController))]
public class Player : MonoBehaviour
{
    private const float JumpActionSpikeWindow = 1f;

    private PlayerMovement movement;
    private PlayerInputManager input;
    private PlayerActionController actionController;
    private CharacterRole characterRole;
    private MatchManager matchManager;
    private float lastJumpTime = -JumpActionSpikeWindow;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputManager>();
        actionController = GetComponent<PlayerActionController>();
        characterRole = GetComponent<CharacterRole>();
        matchManager = FindFirstObjectByType<MatchManager>();
    }

    private void Update()
    {
        if (CanMove())
        {
            movement.Move(input.MoveInput);
        }
        else if (CanServe())
        {
            movement.Move(Vector2.zero);
            movement.Rotate(input.MoveInput);
        }
        else
        {
            movement.Move(Vector2.zero);
        }

        TryJump();

        if (input.ActionQPressed)
        {
            actionController.HandleActionQ(IsJumpActionSpikeInput());
        }
    }

    private bool CanMove()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        return matchManager == null || matchManager.CanPlayRallyAction();
    }

    private void TryJump()
    {
        if (!input.JumpPressed || !CanJump())
        {
            return;
        }

        if (movement.Jump())
        {
            lastJumpTime = Time.time;
        }
    }

    private bool IsJumpActionSpikeInput()
    {
        return !movement.IsGroundedNow && Time.time <= lastJumpTime + JumpActionSpikeWindow;
    }

    private bool CanJump()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        if (matchManager == null || matchManager.CanPlayRallyAction())
        {
            return true;
        }

        return CanServe();
    }

    private bool CanServe()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        return characterRole != null && matchManager != null && matchManager.CanServe(characterRole.TeamSide, characterRole);
    }
}
