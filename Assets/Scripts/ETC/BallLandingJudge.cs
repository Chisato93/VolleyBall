using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BallLandingJudge : MonoBehaviour
{
    private bool hasLanded;

    public void ResetJudge()
    {
        hasLanded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded)
        {
            return;
        }

        LandingSurface landingSurface = collision.collider.GetComponentInChildren<LandingSurface>();
        if (landingSurface == null)
        {
            return;
        }

        hasLanded = true;
        Debug.Log(landingSurface.SurfaceType == LandingSurfaceType.In ? "In" : "Out");
    }
}
