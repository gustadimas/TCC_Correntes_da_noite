using UnityEngine;

namespace CorrentesDaNoite
{
    [RequireComponent(typeof(Collider))]
    public class DeathZone : MonoBehaviour
    {
        [SerializeField] Color gizmoColor = Color.red;
        [SerializeField] bool showGizmo = true;

        void Awake()
        {
            Collider trigger = GetComponent<Collider>();

            if (!trigger.isTrigger) trigger.isTrigger = true;
            gameObject.tag = "DeathZone";
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerDeath = other.GetComponent<Player.PlayerDeath>();
                playerDeath?.Die();
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = gizmoColor;

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);

            UnityEditor.Handles.Label(
                transform.position + Vector3.up,
                "☠️ DEATH ZONE",
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
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
        }
#endif
    }
}