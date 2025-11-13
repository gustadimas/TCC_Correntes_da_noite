using UnityEngine;

namespace CorrentesDaNoite.Camera
{
    /// <summary>
    /// Helper para testar e ajustar os ângulos de câmera
    /// </summary>
    public class CameraDirectionDebugger : MonoBehaviour
    {
        [Header("Testing")]
        [SerializeField] CameraDirection testDirection = CameraDirection.East;
        [SerializeField] bool applyToManager = false;

        [Header("Visual Debug")]
        [SerializeField] bool showGizmos = true;
        [SerializeField] float arrowLength = 3f;
        [SerializeField] Transform playerTransform;

        void Update()
        {
            if (applyToManager && CameraDirectionManager.Instance != null)
            {
                CameraDirectionManager.Instance.SetDirection(testDirection);
                applyToManager = false;
            }
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Vector3 origin = playerTransform != null ? playerTransform.position : transform.position;
            origin += Vector3.up * 0.1f;

            // Calcula vetores baseados na direção de teste
            float angle = (float)testDirection;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;

            // Remove Y
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Desenha RIGHT (usado por input.x = A/D)
            Gizmos.color = Color.red;
            DrawArrow(origin, right, arrowLength, "A/D (right)");

            // Desenha FORWARD (usado por input.y = W/S)
            Gizmos.color = Color.blue;
            DrawArrow(origin, forward, arrowLength, "W/S (forward)");

            // Desenha indicador da direção da câmera
            Gizmos.color = Color.yellow;
            Vector3 cameraIndicator = origin + forward * arrowLength * 0.5f + right * arrowLength * 0.7f;
            Gizmos.DrawWireSphere(cameraIndicator, 0.5f);

#if UNITY_EDITOR
            // Label principal
            UnityEditor.Handles.Label(
                origin + Vector3.up * 2f,
                $"{testDirection} ({angle}°)\n" +
                $"Right (A/D): {right}\n" +
                $"Forward (W/S): {forward}",
                new GUIStyle()
                {
                    normal = { textColor = Color.white },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );

            // Explicação dos controles
            UnityEditor.Handles.Label(
                origin + right * arrowLength + Vector3.up * 0.5f,
                "D →",
                new GUIStyle()
                {
                    normal = { textColor = Color.red },
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                }
            );

            UnityEditor.Handles.Label(
                origin + forward * arrowLength + Vector3.up * 0.5f,
                "W ↑",
                new GUIStyle()
                {
                    normal = { textColor = Color.blue },
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                }
            );

            UnityEditor.Handles.Label(
                cameraIndicator + Vector3.up * 0.5f,
                "📹",
                new GUIStyle()
                {
                    normal = { textColor = Color.yellow },
                    fontSize = 16
                }
            );
#endif
        }

        void DrawArrow(Vector3 origin, Vector3 direction, float length, string label)
        {
            Vector3 end = origin + direction * length;
            Gizmos.DrawLine(origin, end);

            // Pontas da seta
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized * 0.3f;
            Vector3 arrowTip1 = end - direction * 0.3f + right;
            Vector3 arrowTip2 = end - direction * 0.3f - right;

            Gizmos.DrawLine(end, arrowTip1);
            Gizmos.DrawLine(end, arrowTip2);
        }

#if UNITY_EDITOR
        [ContextMenu("Test North (0°)")]
        void TestNorth() => testDirection = CameraDirection.North;

        [ContextMenu("Test East (90°)")]
        void TestEast() => testDirection = CameraDirection.East;

        [ContextMenu("Test South (180°)")]
        void TestSouth() => testDirection = CameraDirection.South;

        [ContextMenu("Test West (270°)")]
        void TestWest() => testDirection = CameraDirection.West;
#endif
    }
}
