using System.Collections;
using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class TutorialPromptUI : MonoBehaviour
    {
        public static TutorialPromptUI Instance { get; set; }

        [Header("UI")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI promptText;

        [Header("Timings")]
        [SerializeField] float fadeInDuration = 0.2f;
        [SerializeField] float fadeOutDuration = 0.2f;
        [SerializeField] float defaultHoldTime = 3f;
        [SerializeField, Range(0f, 1f)] float targetAlpha = 1f;

        Coroutine _currentRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (promptText == null)
            {
                promptText = GetComponentInChildren<TextMeshProUGUI>();
            }

            HideImmediate();
        }

        public void ShowPrompt(string message, float? holdTime = null)
        {
            if (promptText == null || canvasGroup == null)
            {
                Debug.LogWarning("[TutorialPromptUI] UI references not set.");
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            promptText.text = message;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            if (_currentRoutine != null)
            {
                StopCoroutine(_currentRoutine);
            }

            _currentRoutine = StartCoroutine(ShowPromptRoutine(holdTime ?? defaultHoldTime));
        }

        IEnumerator ShowPromptRoutine(float holdTime)
        {
            float startAlpha = canvasGroup.alpha;
            float clampedTarget = Mathf.Clamp01(targetAlpha);
            yield return FadeCanvas(startAlpha, clampedTarget, fadeInDuration);
            yield return new WaitForSeconds(holdTime);
            yield return FadeCanvas(canvasGroup.alpha, 0f, fadeOutDuration);
            HideImmediate();
            _currentRoutine = null;
        }

        IEnumerator FadeCanvas(float from, float to, float duration)
        {
            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        public void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}