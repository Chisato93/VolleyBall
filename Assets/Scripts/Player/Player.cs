using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputManager))]
public class Player : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputManager input;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputManager>();
    }

    private void Update()
    {
        movement.Move(input.MoveInput);

        if (input.JumpPressed)
        {
            movement.Jump();
        }
    }
}
