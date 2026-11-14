using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CorrentesDaNoite.UI
{
    public class MenuGameStarter : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] CanvasGroup menuCanvasGroup;
        [SerializeField] CanvasGroup fadeToBlackPanel;

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

        bool _isTransitioning;

        private void Start()
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = menuCanvasGroup.blocksRaycasts = true;
            fadeToBlackPanel.alpha = 0f;
            fadeToBlackPanel.blocksRaycasts = false;
        }

        public void StartGame(string sceneName)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            StartCoroutine(StartGameRoutine(sceneName));
        }

        IEnumerator StartGameRoutine(string sceneName)
        {
            if (audioSource != null && startGameSound != null) audioSource.PlayOneShot(startGameSound);

            menuCanvasGroup.interactable = false;
            yield return Fade(menuCanvasGroup, menuCanvasGroup.alpha, 0f, fadeOutCurve, fadeOutDuration);
            menuCanvasGroup.blocksRaycasts = false;
            if (deactivateMenuAfterFade) menuCanvasGroup.gameObject.SetActive(false);

            if (useFadeToBlack)
            {
                fadeToBlackPanel.blocksRaycasts = true;
                yield return Fade(fadeToBlackPanel, 0f, 1f, fadeToBlackCurve, fadeToBlackDuration);
            }

            SceneManager.LoadScene(sceneName);
        }

        IEnumerator Fade(CanvasGroup canvasGroup, float from, float to, AnimationCurve curve, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(from, to, curve.Evaluate(elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = to;
        }
    }
}