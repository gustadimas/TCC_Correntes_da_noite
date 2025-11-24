using System.Collections.Generic;
using UnityEngine;

namespace CorrentesDaNoite.Audio.Footsteps
{
    public class FootstepSurfaceTracker : MonoBehaviour
    {
        [SerializeField] internal SurfaceType defaultSurface = SurfaceType.Default;
        [SerializeField] internal bool useRaycastFallback = true;
        [SerializeField] internal float raycastDistance = 1f;
        [SerializeField] internal LayerMask groundLayer = ~0;
        [SerializeField] internal bool useTerrainDetection;
        [SerializeField] internal TerrainTextureDetector terrainTextureDetector;

        protected readonly List<SurfaceType> areaStack = new List<SurfaceType>();

        public SurfaceType CurrentSurface => areaStack.Count > 0 ? areaStack[areaStack.Count - 1] : defaultSurface;

        public void SetDefaultSurface(SurfaceType surface) => defaultSurface = surface;

        protected void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out SurfaceArea area))
                areaStack.Add(area.surfaceType);
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out SurfaceArea area))
                areaStack.Remove(area.surfaceType);
        }

        public SurfaceType DetectByRaycast(Transform origin)
        {
            SurfaceType terrainSurface = DetectTerrainSurface(origin);
            if (terrainSurface != SurfaceType.Default)
                return terrainSurface;

            if (!useRaycastFallback || origin == null)
                return CurrentSurface;

            if (Physics.Raycast(origin.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                if (hit.collider.TryGetComponent(out SurfaceArea area))
                    return area.surfaceType;
            }

            return CurrentSurface;
        }

        SurfaceType DetectTerrainSurface(Transform origin)
        {
            if (!useTerrainDetection || terrainTextureDetector == null || origin == null)
                return SurfaceType.Default;

            return terrainTextureDetector.GetSurfaceAtPosition(origin.position);
        }
    }
}