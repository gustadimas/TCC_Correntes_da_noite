using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CorrentesDaNoite.UI
{
    public class SplashScreenController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] CanvasGroup splashCanvasGroup;
        [SerializeField] CanvasGroup logoGroup;
        [SerializeField] TextMeshProUGUI pressAnyKeyText;
        [SerializeField] MenuTransitionManager menuManager;

        [Header("Timing")]
        [SerializeField] float initialDelay = 1.0f;
        [SerializeField] float logoFadeInDuration = 1.5f;
        [SerializeField] float holdBeforeInput = 0.5f;
        [SerializeField] float textBlinkSpeed = 1.5f;
        [SerializeField] float fadeOutDuration = 1.0f;

        [Header("Settings")]
        [SerializeField] string nextMenuName = "MainMenuScreen";
        [SerializeField] AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Audio (Opcional)")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip splashSound;

        bool _canSkip;
        bool _isSkipping;

        private void Awake()
        {
#if UNITY_EDITOR
            if (autoSkipInEditor && Application.isEditor)
            {
                enabled = false;
                if (splashCanvasGroup != null)
                {
                    splashCanvasGroup.gameObject.SetActive(false);
                }

                if (menuManager == null)
                {
                    menuManager = FindFirstObjectByType<MenuTransitionManager>();
                }

                if (menuManager != null)
                {
                    menuManager.ShowMenu(nextMenuName, false);
                }

                return;
            }
#endif

            if (splashCanvasGroup == null)
            {
                splashCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (menuManager == null)
            {
                menuManager = FindFirstObjectByType<MenuTransitionManager>();
            }

            SetupInitialState();
        }

        private void Start()
        {
            StartCoroutine(SplashSequence());
        }

        private void Update()
        {
            if (_canSkip && !_isSkipping && CheckForInput())
            {
                Skip();
            }
        }

        void SetupInitialState()
        {
            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 1f;
                splashCanvasGroup.interactable = false;
                splashCanvasGroup.blocksRaycasts = false;
            }

            if (logoGroup != null)
            {
                logoGroup.alpha = 0f;
            }

            if (pressAnyKeyText != null)
            {
                Color textColor = pressAnyKeyText.color;
                textColor.a = 0f;
                pressAnyKeyText.color = textColor;
            }
        }

        IEnumerator SplashSequence()
        {
            yield return new WaitForSeconds(initialDelay);

            PlaySplashSound();

            yield return StartCoroutine(FadeInLogo());

            yield return new WaitForSeconds(holdBeforeInput);

            _canSkip = true;

            StartCoroutine(BlinkPressAnyKey());
        }

        IEnumerator FadeInLogo()
        {
            if (logoGroup == null)
            {
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < logoFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / logoFadeInDuration;
                logoGroup.alpha = fadeInCurve.Evaluate(progress);
                yield return null;
            }

            logoGroup.alpha = 1f;
        }

        IEnumerator BlinkPressAnyKey()
        {
            if (pressAnyKeyText == null)
            {
                yield break;
            }

            while (!_isSkipping)
            {
                float alpha = (Mathf.Sin(Time.time * textBlinkSpeed) + 1f) / 2f;
                alpha = Mathf.Clamp(alpha, 0.3f, 1f);

                Color textColor = pressAnyKeyText.color;
                textColor.a = alpha;
                pressAnyKeyText.color = textColor;

                yield return null;
            }
        }

        bool CheckForInput()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        void Skip()
        {
            if (_isSkipping)
            {
                return;
            }

            _isSkipping = true;
            StopAllCoroutines();
            StartCoroutine(FadeOutAndTransition());
        }

        IEnumerator FadeOutAndTransition()
        {
            if (pressAnyKeyText != null)
            {
                const float fadeTextDuration = 0.3f;
                float elapsed = 0f;
                Color startColor = pressAnyKeyText.color;

                while (elapsed < fadeTextDuration)
                {
                    elapsed += Time.deltaTime;
                    Color textColor = pressAnyKeyText.color;
                    textColor.a = Mathf.Lerp(startColor.a, 0f, elapsed / fadeTextDuration);
                    pressAnyKeyText.color = textColor;
                    yield return null;
                }
            }

            if (splashCanvasGroup != null)
            {
                float elapsed = 0f;

                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / fadeOutDuration;
                    splashCanvasGroup.alpha = fadeOutCurve.Evaluate(progress);
                    yield return null;
                }

                splashCanvasGroup.alpha = 0f;
                splashCanvasGroup.gameObject.SetActive(false);
            }

            if (menuManager != null && !string.IsNullOrEmpty(nextMenuName))
            {
                menuManager.ShowMenu(nextMenuName);
            }
        }

        void PlaySplashSound()
        {
            if (audioSource != null && splashSound != null)
            {
                audioSource.PlayOneShot(splashSound);
            }
        }

        public void ForceSkip()
        {
            if (!_isSkipping)
            {
                Skip();
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool autoSkipInEditor;

        private void OnValidate()
        {
            if (logoFadeInDuration <= 0)
            {
                logoFadeInDuration = 1.5f;
            }

            if (fadeOutDuration <= 0)
            {
                fadeOutDuration = 1.0f;
            }
        }
#endif
    }
}