using UnityEngine;

namespace CorrentesDaNoite
{
    /// <summary>
    /// Utilitario de debug para iniciar a cena ja teletransportado para um destino.
    /// Coloque este componente em qualquer GameObject da cena e preencha a lista de destinos.
    /// Util para testar rapidamente diferentes pontos sem percorrer o mapa.
    /// </summary>
    public class DebugTeleportStarter : MonoBehaviour
    {
        [Header("Destinos")]
        [SerializeField] Teleport.TeleportDestination[] destinations;
        [SerializeField, Tooltip("Opcional: MapId correspondente para cada destino (mesmo indice). Deixe vazio se nao quiser alterar mapa.")]
        string[] destinationMapIds;
        [SerializeField] int destinationIndex;

        [Header("Execucao")]
        [SerializeField] bool autoTeleportOnStart = true;
        [SerializeField] bool enableHotkey = true;
        [SerializeField] KeyCode teleportKey = KeyCode.F5;

        [Header("Map Activation (opcional)")]
        [SerializeField] string targetMapId = "";
        [SerializeField] MapActivationManager mapManager;

        void Awake()
        {
            if (mapManager == null)
                mapManager = FindFirstObjectByType<MapActivationManager>();

            ClampDestinationIndex();
        }

        void Start()
        {
            if (autoTeleportOnStart)
                TeleportNow();
        }

        void Update()
        {
            if (enableHotkey && Input.GetKeyDown(teleportKey))
                TeleportNow();
        }

        public void TeleportNow()
        {
            if (!TryGetDestination(destinationIndex, out var dest))
            {
                Debug.LogWarning("[DebugTeleportStarter] Destino invalido ou lista vazia.");
                return;
            }

            if (!TryGetPlayer(out GameObject player, out CharacterController controller))
            {
                Debug.LogWarning("[DebugTeleportStarter] Player nao encontrado (tag Player).");
                return;
            }

            bool hadController = controller != null && controller.enabled;
            if (hadController) controller.enabled = false;

            player.transform.SetPositionAndRotation(dest.Position, dest.Rotation);

            if (hadController) controller.enabled = true;

            dest.ActivateCamera(player.transform);

            string mapIdToUse = GetMapIdForIndex(destinationIndex);
            if (mapManager != null && !string.IsNullOrEmpty(mapIdToUse))
                mapManager.ActivateMap(mapIdToUse);
            else if (mapManager == null && !string.IsNullOrEmpty(mapIdToUse))
                Debug.LogWarning("[DebugTeleportStarter] MapActivationManager nao encontrado para ativar o mapa.");

            Debug.Log($"[DebugTeleportStarter] Teleportado para: {dest.DestinationName} (indice {destinationIndex}).");
        }

        bool TryGetDestination(int index, out Teleport.TeleportDestination destination)
        {
            destination = null;
            if (destinations == null || destinations.Length == 0)
                return false;

            int clampedIndex = Mathf.Clamp(index, 0, destinations.Length - 1);
            destination = destinations[clampedIndex];
            destinationIndex = clampedIndex;
            return destination != null;
        }

        bool TryGetPlayer(out GameObject player, out CharacterController controller)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            controller = null;

            if (player == null)
                return false;

            controller = player.GetComponent<CharacterController>();
            return true;
        }

        string GetMapIdForIndex(int index)
        {
            if (destinationMapIds != null && index >= 0 && index < destinationMapIds.Length)
            {
                if (!string.IsNullOrEmpty(destinationMapIds[index]))
                    return destinationMapIds[index];
            }

            return targetMapId;
        }

        void OnValidate()
        {
            ClampDestinationIndex();
        }

        void ClampDestinationIndex()
        {
            if (destinations == null || destinations.Length == 0)
            {
                destinationIndex = 0;
                return;
            }

            destinationIndex = Mathf.Clamp(destinationIndex, 0, destinations.Length - 1);
        }
    }
}
