using UnityEngine;

namespace CorrentesDaNoite.Chase
{
    [RequireComponent(typeof(Collider))]
    public class ChaseStumbleTrigger : MonoBehaviour
    {
        [Header("Stumble Settings")]
        [Range(0f, 1f)]
        [SerializeField] float speedMultiplierDuringStumble = 0.3f;
        [SerializeField] float stumbleDuration = 2f;

        [Header("Trigger Settings")]
        [SerializeField] bool oneTimeUse = true;
        [SerializeField] bool disableAfterUse = true;

        [Header("Animation")]
        [SerializeField] string stumbleAnimationTrigger = "Stumble";

        [Header("Audio")]
        [SerializeField] AudioClip stumbleSound;
        [SerializeField] AudioSource audioSource;

        [Header("Visual Feedback")]
        [SerializeField] Color gizmoColor = Color.yellow;
        [SerializeField] bool showGizmo = true;

        [Header("State")]
        [SerializeField] bool hasBeenTriggered;

        [Header("Debug")]
        [SerializeField] bool debugMode;

        public bool HasBeenTriggered => hasBeenTriggered;

        void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (!trigger.isTrigger)
                trigger.isTrigger = true;

            if (audioSource == null && stumbleSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (hasBeenTriggered && oneTimeUse)
                return;

            var stumbleHandler = other.GetComponent<ChaseStumbleHandler>();
            if (stumbleHandler == null)
            {
                if (debugMode)
                    Debug.LogWarning("[ChaseStumbleTrigger] Player nao possui ChaseStumbleHandler!");
                return;
            }

            stumbleHandler.TriggerStumble(speedMultiplierDuringStumble, stumbleDuration, stumbleAnimationTrigger);

            if (debugMode)
                Debug.Log($"[ChaseStumbleTrigger] Tropeco acionado: {gameObject.name}");

            if (stumbleSound != null && audioSource != null)
                audioSource.PlayOneShot(stumbleSound);

            hasBeenTriggered = true;

            if (disableAfterUse)
                gameObject.SetActive(false);
        }

        public void ResetTrigger()
        {
            hasBeenTriggered = false;
            gameObject.SetActive(true);

            if (debugMode)
                Debug.Log($"[ChaseStumbleTrigger] Trigger resetado: {gameObject.name}");
        }

        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color color = hasBeenTriggered && oneTimeUse ? Color.gray : gizmoColor;
            Gizmos.color = color;

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color color = hasBeenTriggered && oneTimeUse ? Color.gray : gizmoColor;
            Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
            }

#if UNITY_EDITOR
            Vector3 labelPos = transform.position + Vector3.up * 3f;
            string status = hasBeenTriggered && oneTimeUse ? "[USADO]" : "[ATIVO]";

            UnityEditor.Handles.Label(
                labelPos,
                $"Stumble Trigger {status}\nSlowdown: {speedMultiplierDuringStumble * 100:F0}%\nDuracao: {stumbleDuration}s",
                new GUIStyle()
                {
                    normal = { textColor = color },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
#endif
        }
    }
}
