using System;
using System.Collections;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class CutsceneFadeController : MonoBehaviour
    {
        [SerializeField] public CanvasGroup canvasGroup;
        [SerializeField] public float fadeInDuration = 0.8f;
        [SerializeField] public float fadeOutDuration = 0.5f;
        [SerializeField] public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] public int sortingOrder = 100;

        protected Coroutine currentFade;

        protected void OnEnable()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvas.sortingOrder = sortingOrder;
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        public void FadeIn(Action onComplete = null)
        {
            StartFade(0f, 1f, fadeInDuration, onComplete);
        }

        public void FadeOut(Action onComplete = null)
        {
            StartFade(1f, 0f, fadeOutDuration, onComplete);
        }

        protected void StartFade(float from, float to, float duration, Action onComplete)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (currentFade != null)
                StopCoroutine(currentFade);

            currentFade = StartCoroutine(FadeRoutine(from, to, duration, onComplete));
        }

        protected IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                float curved = fadeCurve.Evaluate(progress);
                canvasGroup.alpha = Mathf.Lerp(from, to, curved);
                yield return null;
            }

            canvasGroup.alpha = to;
            onComplete?.Invoke();
        }
    }
}
