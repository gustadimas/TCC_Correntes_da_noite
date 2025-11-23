using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class FadeController : MonoBehaviour
    {
        public static FadeController Instance { get; private set; }

        [SerializeField] Image fadeImage;
        [SerializeField] float fadeDuration = 1f;
        [SerializeField] Color fadeColor = Color.black;
        [SerializeField] Canvas fadeCanvas;

        bool _isFading;
        public bool IsFading => _isFading;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }

        void SetupFadeCanvas()
        {
            if (fadeCanvas == null)
            {
                GameObject canvasObj = new GameObject("FadeCanvas");
                canvasObj.transform.SetParent(transform);
                fadeCanvas = canvasObj.AddComponent<Canvas>();
                fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                fadeCanvas.sortingOrder = 9999;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
            else
            {
                GraphicRaycaster raycaster = fadeCanvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    Destroy(raycaster);
                }
            }

            if (fadeImage == null)
            {
                GameObject imageObj = new GameObject("FadeImage");
                imageObj.transform.SetParent(fadeCanvas.transform, false);

                fadeImage = imageObj.AddComponent<Image>();
                fadeImage.color = fadeColor;
                fadeImage.raycastTarget = false;

                RectTransform rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }
            else
            {
                fadeImage.raycastTarget = false;
            }

            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        public void FadeInOut(System.Action onFadeOutComplete = null, float? customDuration = null)
        {
            StartCoroutine(FadeInOutCoroutine(onFadeOutComplete, customDuration ?? fadeDuration));
        }

        public void FadeOut(System.Action onComplete = null, float? customDuration = null)
        {
            StartCoroutine(FadeCoroutine(0f, 1f, customDuration ?? fadeDuration, onComplete));
        }

        public void FadeIn(System.Action onComplete = null, float? customDuration = null)
        {
            StartCoroutine(FadeCoroutine(1f, 0f, customDuration ?? fadeDuration, onComplete));
        }

        IEnumerator FadeInOutCoroutine(System.Action onFadeOutComplete, float duration)
        {
            if (_isFading)
            {
                yield break;
            }

            _isFading = true;
            yield return FadeCoroutine(0f, 1f, duration, null);
            onFadeOutComplete?.Invoke();
            yield return new WaitForSeconds(0.1f);
            yield return FadeCoroutine(1f, 0f, duration, null);
            _isFading = false;
        }

        IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            _isFading = true;

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
            }

            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                t = t * t * (3f - 2f * t);

                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                fadeImage.color = color;

                yield return null;
            }

            color.a = targetAlpha;
            fadeImage.color = color;

            onComplete?.Invoke();
            _isFading = false;
        }

        public void SetFadeColor(Color color)
        {
            fadeColor = color;
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                fadeColor.a = c.a;
                fadeImage.color = fadeColor;
                fadeImage.raycastTarget = false;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = Mathf.Clamp01(alpha);
                fadeImage.color = c;
                fadeImage.raycastTarget = false;
            }
        }
    }
}