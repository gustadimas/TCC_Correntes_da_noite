using UnityEngine;
using System.Collections;
using CorrentesDaNoite.UI;
using CorrentesDaNoite;

namespace CorrentesDaNoite.Teleport
{
    [RequireComponent(typeof(Collider))]
    public class TeleportZone : MonoBehaviour
    {
        [SerializeField] TeleportDestination destination;
        [SerializeField] bool oneTimeUse = true;
        [SerializeField] float triggerDelay = 0.5f;
        [SerializeField] float fadeDuration = 1f;
        [SerializeField] Color fadeColor = Color.black;

        [Header("Animation")]
        [SerializeField] string enterAnimationTrigger = "";
        [SerializeField] string teleportAnimationState = "";
        [SerializeField] string idleAnimationState = "Idle";
        [SerializeField] bool resetAnimatorOnTeleport = true;
        [SerializeField] bool waitForTeleportAnimation = false;

        [Header("Audio")]
        [SerializeField] AudioClip teleportSound;
        [SerializeField] AudioSource audioSource;

        [Header("Debug")]
        [SerializeField] Color gizmoColor = Color.magenta;
        [SerializeField] bool showGizmo = true;
        [Header("Map")]
        [SerializeField] string targetMapId = "";
        [SerializeField] MapActivationManager mapManager;

        bool _hasBeenUsed;
        bool _isTeleporting;

        void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            if (!trigger.isTrigger) trigger.isTrigger = true;

            if (audioSource == null && teleportSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            if (mapManager == null)
                mapManager = FindFirstObjectByType<MapActivationManager>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || (_hasBeenUsed && oneTimeUse) || _isTeleporting || destination == null)
                return;

            StartCoroutine(TeleportSequence(other.gameObject));
        }

        IEnumerator TeleportSequence(GameObject player)
        {
            _isTeleporting = true;

            var playerController = player.GetComponent<Player.PlayerController>();
            var animator = player.GetComponent<Animator>();

            if (playerController != null)
                playerController.StopMovementImmediate();

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("IsRunning");
                animator.SetBool("IsCrouching", false);
                animator.ResetTrigger("IsDeath");
                animator.SetFloat("Speed", 0f);

                if (!string.IsNullOrEmpty(enterAnimationTrigger))
                    animator.SetTrigger(enterAnimationTrigger);
            }

            if (triggerDelay > 0f)
                yield return new WaitForSeconds(triggerDelay);

            if (playerController != null)
                playerController.enabled = false;

            if (teleportSound != null && audioSource != null)
                audioSource.PlayOneShot(teleportSound);

            if (FadeController.Instance == null)
            {
                ExecuteTeleport(player, animator);
                if (playerController != null) playerController.enabled = true;
                _isTeleporting = false;
                yield break;
            }

            FadeController.Instance.SetFadeColor(fadeColor);

            bool fadeOutComplete = false;
            FadeController.Instance.FadeOut(() => fadeOutComplete = true, fadeDuration);
            yield return new WaitUntil(() => fadeOutComplete);

            ExecuteTeleport(player, animator);
            yield return new WaitForSeconds(0.1f);

            bool fadeInComplete = false;
            FadeController.Instance.FadeIn(() => fadeInComplete = true, fadeDuration);
            yield return new WaitUntil(() => fadeInComplete);

            if (animator != null && waitForTeleportAnimation && !string.IsNullOrEmpty(teleportAnimationState))
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForSeconds(stateInfo.length);
            }

            if (animator != null && !string.IsNullOrEmpty(idleAnimationState))
                animator.Play(idleAnimationState, 0, 0f);

            if (playerController != null)
                playerController.enabled = true;

            if (oneTimeUse)
                _hasBeenUsed = true;

            _isTeleporting = false;
        }

        void ExecuteTeleport(GameObject player, Animator animator)
        {
            if (destination == null) return;

            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                player.transform.position = destination.Position;
                player.transform.rotation = destination.Rotation;
                charController.enabled = true;
            }
            else
            {
                player.transform.position = destination.Position;
                player.transform.rotation = destination.Rotation;
            }

            destination.ActivateCamera(player.transform);
            if (mapManager != null && !string.IsNullOrEmpty(targetMapId))
                mapManager.ActivateMap(targetMapId);

            if (animator != null)
            {
                if (resetAnimatorOnTeleport)
                {
                    animator.Rebind();
                    animator.Update(0f);
                }

                if (!string.IsNullOrEmpty(teleportAnimationState))
                    animator.Play(teleportAnimationState, 0, 0f);
            }
        }

        public void ResetTrigger() => _hasBeenUsed = false;

        public void TeleportPlayer(GameObject player)
        {
            if (!_isTeleporting && destination != null)
                StartCoroutine(TeleportSequence(player));
        }

        public void SetEnterAnimation(string triggerName) => enterAnimationTrigger = triggerName;

        public void SetTeleportAnimation(string stateName) => teleportAnimationState = stateName;

        public void SetIdleAnimation(string stateName) => idleAnimationState = stateName;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Color color = (_hasBeenUsed && oneTimeUse) ? Color.gray : gizmoColor;
            Gizmos.color = color;

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);

            if (destination != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, destination.Position);
            }

            string status = (_hasBeenUsed && oneTimeUse) ? "[USADO]" : "[ATIVO]";
            string destName = destination != null ? destination.DestinationName : "SEM DESTINO";

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"Teleport: {gameObject.name} {status}\nDestino: {destName}",
                new GUIStyle()
                {
                    normal = { textColor = color },
                    fontSize = 11,
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

            Color color = (_hasBeenUsed && oneTimeUse) ? Color.gray : gizmoColor;
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
        }
#endif
    }
}