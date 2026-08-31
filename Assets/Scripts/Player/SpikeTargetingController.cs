using UnityEngine;

public class SpikeTargetingController : MonoBehaviour
{
    [SerializeField] private float targetingCameraDuration = 0.8f;
    [SerializeField] private PlayerCameraController playerCameraController;

    private void Awake()
    {
        if (playerCameraController == null)
        {
            playerCameraController = PlayerCameraController.FindSceneController(transform);
        }
    }

    public void EnterTargetingMode()
    {
        if (playerCameraController != null)
        {
            playerCameraController.UseCloseUpCamera(targetingCameraDuration);
        }

        // TODO: 타격 지점 선택 중에는 플레이어 이동/일반 액션 입력을 잠근다.
        // TODO: 선택 지점 표시와 선택 확정 로직을 연결한다.
    }
}
