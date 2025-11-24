using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace CorrentesDaNoite.UI
{
    public class MenuGameStarter : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] CanvasGroup menuCanvasGroup;
        [SerializeField] CanvasGroup fadeToBlackPanel;

        [Header("Configuracoes")]
        [SerializeField] string introSceneName = "Introducao";

        [Header("Configuracoes do Fade")]
        [SerializeField] float fadeOutDuration = 1f;
        [SerializeField] float fadeToBlackDuration = 0.5f;
        [SerializeField] AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] AnimationCurve fadeToBlackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Opcoes")]
        [SerializeField] bool deactivateMenuAfterFade = true;
        [SerializeField] bool useFadeToBlack = true;

        [Header("Som (Opcional)")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip startGameSound;

        [Header("Eventos")]
        [SerializeField] UnityEvent onGameStarted;

        bool _isTransitioning;
        bool _gameStarted;

        private void Start()
        {
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 1f;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }

            if (fadeToBlackPanel != null)
            {
                fadeToBlackPanel.alpha = 0f;
                fadeToBlackPanel.blocksRaycasts = false;
            }
        }
        public void StartGame()
        {
            if (_isTransitioning)
            {
                return;
            }

            StartCoroutine(StartGameCoroutine());
        }

        IEnumerator StartGameCoroutine()
        {
            _isTransitioning = true;
            PlayStartGameSound();

            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.interactable = false;
            }

            yield return StartCoroutine(FadeOutMenu());

            if (deactivateMenuAfterFade && menuCanvasGroup != null)
            {
                menuCanvasGroup.gameObject.SetActive(false);
            }

            if (useFadeToBlack && fadeToBlackPanel != null)
            {
                yield return StartCoroutine(FadeToBlack());
            }

            SceneManager.LoadScene(introSceneName);

            _isTransitioning = false;
            OnGameStarted();
        }

        IEnumerator FadeOutMenu()
        {
            if (menuCanvasGroup == null)
            {
                yield break;
            }

            float startAlpha = menuCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;

                float curveValue = fadeOutCurve.Evaluate(progress);
                menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);

                yield return null;
            }

            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.blocksRaycasts = false;
        }

        IEnumerator FadeToBlack()
        {
            if (fadeToBlackPanel == null)
            {
                yield break;
            }

            fadeToBlackPanel.blocksRaycasts = true;

            float elapsed = 0f;

            while (elapsed < fadeToBlackDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeToBlackDuration;

                float curveValue = fadeToBlackCurve.Evaluate(progress);
                fadeToBlackPanel.alpha = Mathf.Lerp(0f, 1f, curveValue);

                yield return null;
            }

            fadeToBlackPanel.alpha = 1f;
        }

        void PlayStartGameSound()
        {
            if (audioSource != null && startGameSound != null)
            {
                audioSource.PlayOneShot(startGameSound);
            }
        }

        void OnGameStarted()
        {
            _gameStarted = true;
            Debug.Log("[MenuGameStarter] Jogo iniciado!");
            onGameStarted?.Invoke();
        }

        public bool HasGameStarted()
        {
            return _gameStarted;
        }

        public void CancelTransition()
        {
            if (_isTransitioning)
            {
                StopAllCoroutines();
                _isTransitioning = false;

                if (menuCanvasGroup != null)
                {
                    menuCanvasGroup.alpha = 1f;
                    menuCanvasGroup.interactable = true;
                    menuCanvasGroup.blocksRaycasts = true;
                }
            }
        }

        public bool IsTransitioning()
        {
            return _isTransitioning;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo;

        private void OnValidate()
        {
            if (fadeOutDuration <= 0)
            {
                fadeOutDuration = 1f;
                Debug.LogWarning("[MenuGameStarter] Duracao do fade deve ser maior que 0!");
            }

            if (showDebugInfo)
            {
                Debug.Log($"[MenuGameStarter] Menu Canvas Group: {(menuCanvasGroup != null ? "Atribuido" : "Nao atribuido")}");
            }
        }
#endif
    }
}