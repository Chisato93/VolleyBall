using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 MoveInput
    {
        get
        {
            var input = Vector2.zero;

            if (Input.GetKey(KeyCode.A))
            {
                input.x -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input.x += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                input.y -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                input.y += 1f;
            }

            return input.sqrMagnitude > 1f ? input.normalized : input;
        }
    }

    public bool JumpPressed => Input.GetKeyDown(KeyCode.Space);
    public bool ActionQPressed => Input.GetKeyDown(KeyCode.Q);
    public bool Action1Pressed => Input.GetKeyDown(KeyCode.Alpha1);
    public bool Action2Pressed => Input.GetKeyDown(KeyCode.Alpha2);
    public bool Action3Pressed => Input.GetKeyDown(KeyCode.Alpha3);
    public bool Action4Pressed => Input.GetKeyDown(KeyCode.Alpha4);
    public bool ActionOPressed => Input.GetKeyDown(KeyCode.O);
}
