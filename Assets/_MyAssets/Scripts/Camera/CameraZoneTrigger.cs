using UnityEngine;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Camera
{
    [RequireComponent(typeof(Collider))]
    public class CameraZoneTrigger : MonoBehaviour
    {
        [Header("Cinemachine Camera")]
        [SerializeField] CinemachineCamera virtualCamera;

        [Header("Priority Settings")]
        [SerializeField] int activePriority = 15;
        [SerializeField] int inactivePriority = 0;

        [Header("Debug")]
        [SerializeField] Color gizmoColor = Color.cyan;
        [SerializeField] bool showGizmo = true;

        Collider _trigger;
        bool _isPlayerInside;

        public bool IsPlayerInside => _isPlayerInside;

        void Awake()
        {
            _trigger = GetComponent<Collider>();

            if (!_trigger.isTrigger)
            {
                _trigger.isTrigger = true;
                Debug.LogWarning($"CameraZoneTrigger: Collider em {gameObject.name} não era Trigger. Corrigido.");
            }

            if (virtualCamera == null)
                Debug.LogError($"CameraZoneTrigger: Virtual Camera não definida em {gameObject.name}!");
            else
                virtualCamera.Priority = inactivePriority;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ActivateCamera();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) DeactivateCamera();
        }

        public void ActivateCamera()
        {
            if (virtualCamera == null) return;
            virtualCamera.Priority = activePriority;
            _isPlayerInside = true;
        }

        public void DeactivateCamera()
        {
            if (virtualCamera == null) return;
            virtualCamera.Priority = inactivePriority;
            _isPlayerInside = false;
        }

        public void ForceActivate() => ActivateCamera();

        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = _isPlayerInside ? Color.green : gizmoColor;

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"📹 {gameObject.name}\n{(virtualCamera != null ? virtualCamera.name : "NO CAMERA")}",
                new GUIStyle() { normal = { textColor = gizmoColor }, fontSize = 10, fontStyle = FontStyle.Bold }
            );
#endif
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
        }
    }
}