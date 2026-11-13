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

        const float RAY_START_HEIGHT = 0.5f;

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

        // o chao vem primeiro pra dentro de casa nao tocar o som do terreno que fica embaixo
        public SurfaceType DetectByRaycast(Transform origin)
        {
            if (origin == null) return CurrentSurface;
            if (!useRaycastFallback || !Physics.Raycast(origin.position + Vector3.up * RAY_START_HEIGHT, Vector3.down, out RaycastHit hit, raycastDistance + RAY_START_HEIGHT, groundLayer, QueryTriggerInteraction.Ignore)) return GetTerrainOrCurrent(origin.position);
            if (hit.collider.TryGetComponent(out SurfaceArea area)) return area.surfaceType;
            return hit.collider is TerrainCollider ? GetTerrainOrCurrent(hit.point) : CurrentSurface;
        }

        SurfaceType GetTerrainOrCurrent(Vector3 position)
        {
            if (!useTerrainDetection || terrainTextureDetector == null) return CurrentSurface;
            SurfaceType surface = terrainTextureDetector.GetSurfaceAtPosition(position);
            return surface != SurfaceType.Default ? surface : CurrentSurface;
        }
    }
}