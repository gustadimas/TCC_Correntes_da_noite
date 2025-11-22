using UnityEngine;

namespace CorrentesDaNoite
{
    public class MapActivationManager : MonoBehaviour
    {
        [System.Serializable]
        public class MapChunk
        {
            public string mapId;
            public GameObject[] roots;
        }

        [SerializeField] string initialMapId;
        [SerializeField] MapChunk[] maps;

        string _currentMapId;

        void Start()
        {
            if (!string.IsNullOrEmpty(initialMapId))
                ActivateMap(initialMapId);
        }

        public void ActivateMap(string mapId)
        {
            if (string.IsNullOrEmpty(mapId) || maps == null || maps.Length == 0)
                return;

            if (_currentMapId == mapId)
                return;

            _currentMapId = mapId;

            for (int i = 0; i < maps.Length; i++)
            {
                bool active = maps[i].mapId == mapId;
                SetChunkActive(maps[i], active);
            }
        }

        void SetChunkActive(MapChunk chunk, bool active)
        {
            if (chunk == null || chunk.roots == null) return;

            for (int i = 0; i < chunk.roots.Length; i++)
            {
                if (chunk.roots[i] == null) continue;
                chunk.roots[i].SetActive(active);
            }
        }

        public string CurrentMapId => _currentMapId;
    }
}
