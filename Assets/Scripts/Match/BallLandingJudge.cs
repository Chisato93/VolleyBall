using UnityEngine;

public class BallLandingJudge : MonoBehaviour
{
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private string ballName = "volleyball";
    [SerializeField] private float courtHalfWidth = 5f;
    [SerializeField] private float courtHalfLength = 10f;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsBall(collision.collider))
        {
            return;
        }

        EnsureReferences();
        if (matchManager == null)
        {
            return;
        }

        Vector3 ballPosition = collision.transform.position;
        bool isIn = Mathf.Abs(ballPosition.x) <= courtHalfWidth
            && Mathf.Abs(ballPosition.z) <= courtHalfLength;

        matchManager.RegisterAttackLanding(isIn);
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
