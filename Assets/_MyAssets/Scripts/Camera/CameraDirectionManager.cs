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
            if (Instance == this)
                Instance = null;
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

        public Vector3 ConvertInputToWorldDirection(Vector2 input)
        {
            Vector3 forward = GetCurrentForwardDirection();
            Vector3 right = GetCurrentRightDirection();

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (right * input.x + forward * input.y).normalized;
        }
    }
}