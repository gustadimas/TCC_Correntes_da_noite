using UnityEngine;

namespace CorrentesDaNoite.Chase
{
    [RequireComponent(typeof(Collider))]
    public class ChaseEndZone : MonoBehaviour
    {
        [Header("End Zone Settings")]
        [Tooltip("Referência ao controlador da sequência")]
        [SerializeField] protected JungleChaseSequenceController sequenceController;

        [Tooltip("Finalizar automaticamente ao entrar na zona")]
        [SerializeField] protected bool autoTriggerOnEnter = true;

        [Header("Transition Settings")]
        [Tooltip("Delay antes de acionar transição (segundos)")]
        [SerializeField] protected float transitionDelay = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] protected Color gizmoColor = Color.green;
        [SerializeField] protected bool showGizmo = true;

        [Header("State")]
        [SerializeField] protected bool hasBeenTriggered;

        [Header("Debug")]
        [SerializeField] protected bool debugMode;

        public bool HasBeenTriggered => hasBeenTriggered;

        public System.Action OnZoneEntered;

        void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (!trigger.isTrigger)
                trigger.isTrigger = true;

            if (sequenceController == null)
                sequenceController = FindFirstObjectByType<JungleChaseSequenceController>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (hasBeenTriggered)
                return;

            hasBeenTriggered = true;

            if (debugMode)
                Debug.Log($"[ChaseEndZone] Player entrou na zona final: {gameObject.name}");

            OnZoneEntered?.Invoke();

            if (autoTriggerOnEnter && sequenceController != null)
            {
                Invoke(nameof(TriggerEndSequence), transitionDelay);
            }
        }

        protected void TriggerEndSequence()
        {
            if (sequenceController != null)
            {
                sequenceController.TriggerEndSequence();

                if (debugMode)
                    Debug.Log("[ChaseEndZone] Sequência de fim acionada");
            }
            else if (debugMode)
            {
                Debug.LogWarning("[ChaseEndZone] Sequence Controller não encontrado!");
            }
        }

        public void ResetZone()
        {
            hasBeenTriggered = false;
            CancelInvoke(nameof(TriggerEndSequence));

            if (debugMode)
                Debug.Log("[ChaseEndZone] Zona resetada");
        }

        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color color = hasBeenTriggered ? Color.gray : gizmoColor;
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

            Gizmos.color = Color.green;
            Vector3 flagPos = transform.position + Vector3.up * 3f;
            Gizmos.DrawLine(transform.position, flagPos);
            Gizmos.DrawWireSphere(flagPos, 0.5f);
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color color = hasBeenTriggered ? Color.gray : gizmoColor;
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
            Vector3 labelPos = transform.position + Vector3.up * 5f;
            string status = hasBeenTriggered ? "[ACIONADO]" : "[AGUARDANDO]";

            UnityEditor.Handles.Label(
                labelPos,
                $"CHASE END ZONE {status}\nDelay: {transitionDelay}s",
                new GUIStyle()
                {
                    normal = { textColor = color },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
#endif
        }
    }
}
