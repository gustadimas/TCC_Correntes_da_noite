using UnityEngine;
using System.Collections;

namespace CorrentesDaNoite.Chase
{
    public class ChaseLookBackController : MonoBehaviour
    {
        [Header("Look Back Settings")]
        [SerializeField] float minLookBackInterval = 3f;
        [SerializeField] float maxLookBackInterval = 6f;
        [SerializeField] float lookBackDuration = 1.5f;
        [Range(0f, 1f)]
        [SerializeField] float speedMultiplierDuringLookBack = 0.7f;

        [Header("References")]
        [SerializeField] Animator animator;
        [SerializeField] Player.PlayerController playerController;

        [Header("Animation")]
        [SerializeField] string lookBackAnimationTrigger = "LookBack";
        [SerializeField] string lookBackAnimationBool = "IsLookingBack";

        [Header("State")]
        [SerializeField] bool isEnabled;
        [SerializeField] bool isLookingBack;
        [SerializeField] float nextLookBackTime;

        [Header("Debug")]
        [SerializeField] bool debugMode;

        Coroutine lookBackCoroutine;

        public bool IsEnabled => isEnabled;
        public bool IsLookingBack => isLookingBack;

        void Awake()
        {
            animator ??= GetComponent<Animator>();
            playerController ??= GetComponent<Player.PlayerController>();
        }

        void Start()
        {
            CalculateNextLookBackTime();
        }

        void Update()
        {
            if (isEnabled && !isLookingBack && Time.time >= nextLookBackTime)
                TriggerLookBack();
        }

        public void EnableLookBack()
        {
            isEnabled = true;
            CalculateNextLookBackTime();

            if (debugMode)
                Debug.Log($"[ChaseLookBack] Sistema habilitado. Proximo look back em {nextLookBackTime - Time.time:F1}s");
        }

        public void DisableLookBack()
        {
            isEnabled = false;

            if (lookBackCoroutine != null)
            {
                StopCoroutine(lookBackCoroutine);
                lookBackCoroutine = null;
            }

            isLookingBack = false;

            if (animator != null && !string.IsNullOrEmpty(lookBackAnimationBool))
                animator.SetBool(lookBackAnimationBool, false);

            if (debugMode)
                Debug.Log("[ChaseLookBack] Sistema desabilitado");
        }

        public void TriggerLookBack()
        {
            if (isLookingBack) return;

            if (lookBackCoroutine != null)
                StopCoroutine(lookBackCoroutine);

            lookBackCoroutine = StartCoroutine(LookBackSequence());
        }

        IEnumerator LookBackSequence()
        {
            isLookingBack = true;

            if (animator != null)
            {
                if (!string.IsNullOrEmpty(lookBackAnimationTrigger))
                    animator.SetTrigger(lookBackAnimationTrigger);

                if (!string.IsNullOrEmpty(lookBackAnimationBool))
                    animator.SetBool(lookBackAnimationBool, true);
            }

            if (debugMode)
                Debug.Log("[ChaseLookBack] Olhando para tras iniciado");

            yield return new WaitForSeconds(lookBackDuration);

            if (animator != null && !string.IsNullOrEmpty(lookBackAnimationBool))
                animator.SetBool(lookBackAnimationBool, false);

            isLookingBack = false;
            CalculateNextLookBackTime();

            if (debugMode)
                Debug.Log($"[ChaseLookBack] Olhando para tras finalizado. Proximo em {nextLookBackTime - Time.time:F1}s");
        }

        void CalculateNextLookBackTime()
        {
            float interval = Random.Range(minLookBackInterval, maxLookBackInterval);
            nextLookBackTime = Time.time + interval;
        }

        public float GetCurrentSpeedMultiplier()
        {
            return isLookingBack ? speedMultiplierDuringLookBack : 1f;
        }

        public void SetLookBackInterval(float min, float max)
        {
            minLookBackInterval = min;
            maxLookBackInterval = max;
            CalculateNextLookBackTime();
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplierDuringLookBack = Mathf.Clamp01(multiplier);
        }
    }
}
