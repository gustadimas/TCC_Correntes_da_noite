using UnityEngine;

namespace CorrentesDaNoite.Audio.Footsteps
{
    [RequireComponent(typeof(Collider))]
    public class SurfaceArea : MonoBehaviour
    {
        [SerializeField] internal SurfaceType surfaceType = SurfaceType.Default;

        protected void Reset()
        {
            Collider colliderComponent = GetComponent<Collider>();
            if (colliderComponent != null)
                colliderComponent.isTrigger = true;
        }
    }
}