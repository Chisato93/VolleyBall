using UnityEngine;

public class LandingSurface : MonoBehaviour
{
    [SerializeField] private LandingSurfaceType surfaceType = LandingSurfaceType.In;

    public LandingSurfaceType SurfaceType => surfaceType;
}
