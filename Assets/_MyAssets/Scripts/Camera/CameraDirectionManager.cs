using UnityEngine;
using UnityEngine.Events;

namespace CorrentesDaNoite.Camera
{
    public class CameraDirectionManager : MonoBehaviour
    {
        public static CameraDirectionManager Instance { get; private set; }

        [SerializeField] CameraDirection currentDirection = CameraDirection.East;
        public UnityEvent<CameraDirection> OnDirectionChanged;

        public CameraDirection CurrentDirection => currentDirection;
        public float CurrentYRotation => (float)currentDirection;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void OnCameraDirectionChanged(CameraDirection newDirection)
        {
            currentDirection = newDirection;
            OnDirectionChanged?.Invoke(newDirection);
        }

        public void SetDirection(CameraDirection direction) => OnCameraDirectionChanged(direction);
        public Quaternion GetCurrentRotationQuaternion() => Quaternion.Euler(0f, CurrentYRotation, 0f);
        public Vector3 GetCurrentForwardDirection() => GetCurrentRotationQuaternion() * Vector3.forward;
        public Vector3 GetCurrentRightDirection() => GetCurrentRotationQuaternion() * Vector3.right;

        public Vector3 ConvertInputToWorldDirection(Vector2 input, bool invertHorizontal = false)
        {
            Vector3 forward = GetCurrentForwardDirection();
            Vector3 right = GetCurrentRightDirection();

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            float x = invertHorizontal ? -input.x : input.x;
            return (right * x + forward * input.y).normalized;
        }

        public void SetDirectionFromCamera(Transform cameraTransform, bool invertForward = false)
        {
            if (cameraTransform == null) return;

            Vector3 forward = invertForward ? -cameraTransform.forward : cameraTransform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
                return;

            forward.Normalize();
            float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float snapped = Mathf.Round(angle / 90f) * 90f;

            // Normaliza para -180..180
            if (snapped > 180f) snapped -= 360f;
            if (snapped <= -180f) snapped += 360f;

            CameraDirection newDir = CameraDirection.North;
            if (Mathf.Approximately(snapped, 180f) || Mathf.Approximately(snapped, -180f))
                newDir = CameraDirection.South;
            else if (Mathf.Approximately(snapped, 90f))
                newDir = CameraDirection.West;
            else if (Mathf.Approximately(snapped, -90f))
                newDir = CameraDirection.East;
            else
                newDir = CameraDirection.North;

            OnCameraDirectionChanged(newDir);
        }
    }
}
