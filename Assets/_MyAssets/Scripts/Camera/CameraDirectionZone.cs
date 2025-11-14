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
        [SerializeField] Color gizmoColor = Color.cyan;
        [SerializeField] bool showGizmo = true;

        bool _isPlayerInside;

        public bool IsPlayerInside => _isPlayerInside;
        float TargetYRotation => (float)cameraDirection;

        void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (!trigger.isTrigger) trigger.isTrigger = true;

            if (virtualCamera != null)
            {
                virtualCamera.Priority = inactivePriority;
                SetupCamera();
            }
        }

        void SetupCamera()
        {
            virtualCamera.transform.rotation = Quaternion.Euler(15f, TargetYRotation, 0f);

            var follow = virtualCamera.GetComponent<CinemachineFollow>();
            if (follow != null) follow.FollowOffset = followOffset;

            virtualCamera.GetComponent<CameraPendulum>()?.UpdateInitialRotation();
            virtualCamera.GetComponent<CameraMouseLook>()?.UpdateBaseRotation();
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

            SetupCamera();
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