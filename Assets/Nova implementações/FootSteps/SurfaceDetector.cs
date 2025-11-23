using UnityEngine;

public class SurfaceDetector : MonoBehaviour
{
    [SerializeField] SurfaceType surfaceType = SurfaceType.Normal;
    
    public SurfaceType GetSurfaceType() => surfaceType;
}
