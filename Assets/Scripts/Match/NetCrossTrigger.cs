using UnityEngine;

public class NetCrossTrigger : MonoBehaviour
{
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private string ballName = "volleyball";

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsBall(other))
        {
            return;
        }

        EnsureReferences();
        if (matchManager == null)
        {
            return;
        }

        Rigidbody ballBody = other.attachedRigidbody;
        float zVelocity = ballBody != null ? ballBody.linearVelocity.z : 0f;

        if (zVelocity > 0f)
        {
            matchManager.RegisterNetCross(TeamSide.Player, TeamSide.Opponent);
        }
        else if (zVelocity < 0f)
        {
            matchManager.RegisterNetCross(TeamSide.Opponent, TeamSide.Player);
        }
    }

    private void EnsureReferences()
    {
        if (matchManager == null)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
        }
    }

    private bool IsBall(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            return other.attachedRigidbody.name == ballName;
        }

        return other.name == ballName;
    }
}
