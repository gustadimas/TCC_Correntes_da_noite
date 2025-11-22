using UnityEngine;
using System.Collections;

namespace CorrentesDaNoite.Chase
{
    public class ChaseStumbleHandler : MonoBehaviour
    {
        [Header("Stumble State")]
        [SerializeField] protected bool isStumbling;
        [SerializeField] protected float currentSpeedMultiplier = 1f;

        [Header("References")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Player.PlayerController playerController;

        [Header("Animation")]
        [SerializeField] protected string runBoolName = "IsRunning";
        [SerializeField] protected string runSpeedFloatName = "Speed";

        [Header("Stumble Effect")]
        [SerializeField] protected float defaultStumbleSpeedMultiplier = 0.3f;
        [SerializeField] protected float defaultStumbleDuration = 2f;

        [Header("Debug")]
        [SerializeField] protected bool debugMode;

        protected Coroutine stumbleCoroutine;
        protected float normalRunSpeed;

        public bool IsStumbling => isStumbling;
        public float CurrentSpeedMultiplier => currentSpeedMultiplier;

        public System.Action<float, float> OnStumbleStarted;
        public System.Action OnStumbleEnded;

        void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (playerController == null)
                playerController = GetComponent<Player.PlayerController>();
        }

        void Start()
        {
            if (playerController != null)
                normalRunSpeed = playerController.CurrentSpeed;
        }

        public void TriggerStumble()
        {
            TriggerStumble(defaultStumbleSpeedMultiplier, defaultStumbleDuration, "Stumble");
        }

        public void TriggerStumble(float speedMultiplier, float duration, string animationTrigger)
        {
            if (isStumbling)
            {
                if (debugMode)
                    Debug.LogWarning("[ChaseStumbleHandler] Ja esta tropecando, ignorando novo trigger");
                return;
            }

            if (stumbleCoroutine != null)
                StopCoroutine(stumbleCoroutine);

            stumbleCoroutine = StartCoroutine(StumbleSequence(speedMultiplier, duration, animationTrigger));
        }

        protected IEnumerator StumbleSequence(float speedMultiplier, float duration, string animationTrigger)
        {
            isStumbling = true;
            currentSpeedMultiplier = speedMultiplier;

            if (animator != null && !string.IsNullOrEmpty(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }

            SetRunningAnimation(false);

            OnStumbleStarted?.Invoke(speedMultiplier, duration);

            if (debugMode)
                Debug.Log($"[ChaseStumbleHandler] Tropeco iniciado - Velocidade: {speedMultiplier * 100:F0}%, Duracao: {duration}s");

            yield return new WaitForSeconds(duration);

            isStumbling = false;
            currentSpeedMultiplier = 1f;
            SetRunningAnimation(true);

            OnStumbleEnded?.Invoke();

            if (debugMode)
                Debug.Log("[ChaseStumbleHandler] Tropeco finalizado - Velocidade restaurada");

            stumbleCoroutine = null;
        }

        public void CancelStumble()
        {
            if (stumbleCoroutine != null)
            {
                StopCoroutine(stumbleCoroutine);
                stumbleCoroutine = null;
            }

            isStumbling = false;
            currentSpeedMultiplier = 1f;
            SetRunningAnimation(true);

            OnStumbleEnded?.Invoke();

            if (debugMode)
                Debug.Log("[ChaseStumbleHandler] Tropeco cancelado");
        }

        public float GetModifiedSpeed(float baseSpeed)
        {
            return baseSpeed * currentSpeedMultiplier;
        }

        void SetRunningAnimation(bool isRunning)
        {
            if (animator == null) return;

            if (!string.IsNullOrEmpty(runBoolName))
                animator.SetBool(runBoolName, isRunning);

            if (!string.IsNullOrEmpty(runSpeedFloatName))
                animator.SetFloat(runSpeedFloatName, isRunning ? 1f : 0f);
        }
    }
}
