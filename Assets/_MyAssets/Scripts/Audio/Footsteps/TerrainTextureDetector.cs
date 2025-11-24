using UnityEngine;

namespace CorrentesDaNoite.Audio.Footsteps
{
    public class TerrainTextureDetector : MonoBehaviour
    {
        [SerializeField] internal Terrain terrain;
        [SerializeField] internal SurfaceType[] surfaceByLayer;

        void Awake()
        {
            if (terrain == null)
                terrain = FindFirstObjectByType<Terrain>();

            EnsureArraySize();
        }

        void OnValidate()
        {
            EnsureArraySize();
        }

        void EnsureArraySize()
        {
            if (terrain == null || terrain.terrainData == null)
                return;

            int layers = terrain.terrainData.terrainLayers.Length;
            if (surfaceByLayer == null || surfaceByLayer.Length != layers)
                surfaceByLayer = new SurfaceType[layers];
        }

        public SurfaceType GetSurfaceAtPosition(Vector3 worldPosition)
        {
            if (terrain == null || terrain.terrainData == null)
                return SurfaceType.Default;

            TerrainData data = terrain.terrainData;
            Vector3 localPos = worldPosition - terrain.transform.position;

            int mapX = Mathf.Clamp(Mathf.RoundToInt((localPos.x / data.size.x) * data.alphamapWidth), 0, data.alphamapWidth - 1);
            int mapZ = Mathf.Clamp(Mathf.RoundToInt((localPos.z / data.size.z) * data.alphamapHeight), 0, data.alphamapHeight - 1);

            float[,,] alpha = data.GetAlphamaps(mapX, mapZ, 1, 1);

            int bestIndex = 0;
            float bestWeight = 0f;
            for (int i = 0; i < alpha.GetLength(2); i++)
            {
                if (alpha[0, 0, i] > bestWeight)
                {
                    bestWeight = alpha[0, 0, i];
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0 && bestIndex < surfaceByLayer.Length)
                return surfaceByLayer[bestIndex];

            return SurfaceType.Default;
        }
    }
}