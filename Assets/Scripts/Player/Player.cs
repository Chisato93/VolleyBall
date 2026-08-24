using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputManager))]
[RequireComponent(typeof(PlayerActionController))]
public class Player : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputManager input;
    private PlayerActionController actionController;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputManager>();
        actionController = GetComponent<PlayerActionController>();

    }

    private void Update()
    {
        movement.Move(input.MoveInput);

        if (input.JumpPressed)
        {
            movement.Jump();
        }

        if (input.ActionQPressed)
        {
            actionController.HandleActionQ();
        }
    }
}
