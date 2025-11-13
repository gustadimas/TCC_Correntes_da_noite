using UnityEngine;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Camera
{
    public enum CameraDirection
    {
        North = 0,
        West = 90,
        South = 180,
        East = -90
    }

    [RequireComponent(typeof(Collider))]
    public class CameraDirectionZone : MonoBehaviour
    {
        [SerializeField] CinemachineCamera virtualCamera;
        [SerializeField] CameraDirection cameraDirection = CameraDirection.East;
        [SerializeField] int activePriority = 15;
        [SerializeField] int inactivePriority = 0;
        [SerializeField] Vector3 followOffset = new Vector3(0f, 3f, -10f);

        [Header("Debug")]
        [SerializeField] Color gizmoColor = Color.cyan;
        [SerializeField] bool showGizmo = true;

        Collider _trigger;
        bool _isPlayerInside;

        public bool IsPlayerInside => _isPlayerInside;
        float TargetYRotation => (float)cameraDirection;

        void Awake()
        {
            _trigger = GetComponent<Collider>();
            if (!_trigger.isTrigger)
            {
                _trigger.isTrigger = true;
                Debug.LogWarning($"Collider em {gameObject.name} não era Trigger.");
            }

            if (virtualCamera != null)
            {
                virtualCamera.Priority = inactivePriority;
                SetupCamera();
            }
            else
            {
                Debug.LogError($"Virtual Camera não definida em {gameObject.name}!");
            }
        }

        void SetupCamera()
        {
            virtualCamera.transform.rotation = Quaternion.Euler(15f, TargetYRotation, 0f);
            var follow = virtualCamera.GetComponent<CinemachineFollow>();
            if (follow != null)
                follow.FollowOffset = followOffset;

            var pendulum = virtualCamera.GetComponent<CameraPendulum>();
            if (pendulum != null)
                pendulum.UpdateInitialRotation();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                ActivateCamera();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                DeactivateCamera();
        }

        public void ActivateCamera()
        {
            if (virtualCamera == null) return;

            virtualCamera.transform.rotation = Quaternion.Euler(15f, TargetYRotation, 0f);

            var follow = virtualCamera.GetComponent<CinemachineFollow>();
            if (follow != null)
                follow.FollowOffset = followOffset;

            var pendulum = virtualCamera.GetComponent<CameraPendulum>();
            if (pendulum != null)
                pendulum.UpdateInitialRotation();

            virtualCamera.Priority = activePriority;
            _isPlayerInside = true;

            CameraDirectionManager.Instance?.OnCameraDirectionChanged(cameraDirection);
        }

        public void DeactivateCamera()
        {
            if (virtualCamera == null) return;
            virtualCamera.Priority = inactivePriority;
            _isPlayerInside = false;
        }

#if UNITY_EDITOR
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

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"📹 {gameObject.name}\n{cameraDirection} ({(int)cameraDirection}°)",
                new GUIStyle()
                {
                    normal = { textColor = gizmoColor },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
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

            if (virtualCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(virtualCamera.transform.position, 0.3f);
                Gizmos.DrawLine(transform.position, virtualCamera.transform.position);
            }
        }
#endif
    }
}
