using UnityEngine;
using CorrentesDaNoite.Camera;

namespace CorrentesDaNoite.Checkpoint
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] string checkpointId = "checkpoint_01";
        [SerializeField] string checkpointName = "Checkpoint";
        [SerializeField] CameraDirection cameraDirection = CameraDirection.East;
        [SerializeField] bool useCustomPlayerRotation = false;
        [SerializeField] float customPlayerRotationY = 0f;
        [SerializeField] Unity.Cinemachine.CinemachineCamera targetCamera;
        [SerializeField] int cameraPriority = 15;
        [SerializeField] Vector3 followOffset = new Vector3(0f, 4f, 0f);
        [SerializeField] bool activateOnEnter = true;
        [SerializeField] Color gizmoColor = Color.yellow;
        [SerializeField] bool showGizmo = true;

        bool _isActive;

        public string CheckpointId => checkpointId;
        public string CheckpointName => checkpointName;
        public CameraDirection Direction => cameraDirection;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => useCustomPlayerRotation
            ? Quaternion.Euler(0f, customPlayerRotationY, 0f)
            : Quaternion.Euler(0f, (float)cameraDirection, 0f);
        public bool IsActive => _isActive;

        void OnTriggerEnter(Collider other)
        {
            if (activateOnEnter && other.CompareTag("Player"))
                CheckpointManager.Instance?.SetCheckpoint(this);
        }

        public void ActivateCamera()
        {
            if (targetCamera == null) return;

            targetCamera.Priority = 0;
            targetCamera.transform.rotation = Quaternion.Euler(15f, (float)cameraDirection, 0f);

            var follow = targetCamera.GetComponent<Unity.Cinemachine.CinemachineFollow>();
            if (follow != null) follow.FollowOffset = followOffset;

            targetCamera.GetComponent<CameraPendulum>()?.UpdateInitialRotation();
            targetCamera.GetComponent<CameraMouseLook>()?.UpdateBaseRotation();

            targetCamera.enabled = false;
            targetCamera.enabled = true;
            targetCamera.Priority = cameraPriority;

            CameraDirectionManager.Instance?.SetDirection(cameraDirection);
        }

        public void SetActive(bool active) => _isActive = active;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Gizmos.color = _isActive ? Color.green : gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.3f);

            Vector3 direction = Rotation * Vector3.forward;
            Gizmos.color = Color.yellow;
            DrawArrow(transform.position, direction, 2f);

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"💾 {checkpointName}\n[{checkpointId}]\n{cameraDirection} ({(int)cameraDirection}°)",
                new GUIStyle()
                {
                    normal = { textColor = _isActive ? Color.green : gizmoColor },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }

        void OnDrawGizmosSelected()
        {
            if (!showGizmo) return;

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, 1f);

            if (targetCamera != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, targetCamera.transform.position);
                Gizmos.DrawWireSphere(targetCamera.transform.position, 0.3f);
            }
        }

        void DrawArrow(Vector3 position, Vector3 direction, float length)
        {
            Vector3 endPoint = position + direction * length;
            Gizmos.DrawRay(position, direction * length);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 135, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -135, 0) * Vector3.forward;

            Gizmos.DrawRay(endPoint, right * 0.5f);
            Gizmos.DrawRay(endPoint, left * 0.5f);
        }
#endif
    }
}