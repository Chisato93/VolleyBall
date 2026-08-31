using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    private const float MinBlendDuration = 0.01f;
    private const int InactivePriority = 0;
    private const int ActivePriority = 20;

    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private CinemachineCamera tacticalCamera;
    [SerializeField] private CinemachineCamera actionCamera;
    [SerializeField] private CinemachineCamera closeUpCamera;
    [SerializeField] private float defaultActionDuration = 0.6f;

    private CameraMode currentMode = CameraMode.Tactical;
    private float actionModeEndTime;

    private enum CameraMode
    {
        Tactical,
        Action,
        CloseUp
    }

    public static PlayerCameraController FindSceneController(Transform playerTarget)
    {
        PlayerCameraController controller = FindFirstObjectByType<PlayerCameraController>();
        if (controller == null)
        {
            return null;
        }

        controller.EnsureReferences();
        controller.SetPlayerTarget(playerTarget);
        return controller;
    }

    public void SetPlayerTarget(Transform target)
    {
        if (target != null)
        {
            playerTarget = target;
            AssignTargets();
        }
    }

    public void UseTacticalCamera()
    {
        currentMode = CameraMode.Tactical;
        actionModeEndTime = 0f;
        ApplyCameraPriorities();
    }

    public void UseActionCamera()
    {
        UseActionCamera(defaultActionDuration);
    }

    public void UseActionCamera(float duration)
    {
        currentMode = CameraMode.Action;
        actionModeEndTime = Time.time + Mathf.Max(MinBlendDuration, duration);
        ApplyCameraPriorities();
    }

    public void UseCloseUpCamera(float duration)
    {
        currentMode = CameraMode.CloseUp;
        actionModeEndTime = Time.time + Mathf.Max(MinBlendDuration, duration);
        ApplyCameraPriorities();
    }

    private void Awake()
    {
        EnsureReferences();
        AssignTargets();
        ApplyCameraPriorities();
    }

    private void LateUpdate()
    {
        EnsureReferences();

        if (playerTarget == null)
        {
            return;
        }

        AssignTargets();

        if (currentMode != CameraMode.Tactical && Time.time >= actionModeEndTime)
        {
            UseTacticalCamera();
        }
    }

    private void EnsureReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (playerTarget == null)
        {
            Player player = FindFirstObjectByType<Player>();
            playerTarget = player != null ? player.transform : null;
        }

        if (targetCamera != null && targetCamera.GetComponent<CinemachineBrain>() == null)
        {
            targetCamera.gameObject.AddComponent<CinemachineBrain>();
        }

        if (tacticalCamera == null)
        {
            tacticalCamera = FindChildCamera("TacticalCamera");
        }

        if (actionCamera == null)
        {
            actionCamera = FindChildCamera("ActionCamera");
        }

        if (closeUpCamera == null)
        {
            closeUpCamera = FindChildCamera("CloseUpCamera");
        }
    }

    private CinemachineCamera FindChildCamera(string cameraName)
    {
        Transform searchRoot = transform.parent != null ? transform.parent : transform;
        Transform child = searchRoot.Find(cameraName);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<CinemachineCamera>();
    }

    private void AssignTargets()
    {
        if (playerTarget == null)
        {
            return;
        }

        AssignTarget(tacticalCamera);
        AssignTarget(actionCamera);
        AssignTarget(closeUpCamera);
    }

    private void AssignTarget(CinemachineCamera camera)
    {
        if (camera != null)
        {
            camera.Follow = playerTarget;
            camera.LookAt = playerTarget;
        }
    }

    private void ApplyCameraPriorities()
    {
        SetPriority(tacticalCamera, currentMode == CameraMode.Tactical);
        SetPriority(actionCamera, currentMode == CameraMode.Action);
        SetPriority(closeUpCamera, currentMode == CameraMode.CloseUp);
    }

    private static void SetPriority(CinemachineCamera camera, bool isActive)
    {
        if (camera != null)
        {
            camera.Priority.Value = isActive ? ActivePriority : InactivePriority;
        }
    }
}
