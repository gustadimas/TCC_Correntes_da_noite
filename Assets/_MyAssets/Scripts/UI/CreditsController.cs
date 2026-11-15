using System.Collections;
using CorrentesDaNoite.Checkpoint;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CorrentesDaNoite.UI
{
    public class CreditsController : MonoBehaviour
    {
        public string menuSceneName = "Menu";
        public bool useScreenFadeOnExit = true;
        public float exitFadeDuration = 1f;
        public Color exitFadeColor = Color.black;
        public bool fadeInAfterLoad = true;
        public float fadeInDuration = 1f;
        [SerializeField] TMP_Text _statsText;

        private void Start()
        {
            int seconds = Mathf.FloorToInt(EscapeStats.PlayTime);
            if (_statsText != null) _statsText.text = $"Tempo de fuga: {seconds / 60}:{seconds % 60:00}      Capturas: {EscapeStats.Captures}";
        }

        public void GoToMenu()
        {
            if (string.IsNullOrEmpty(menuSceneName))
                return;

            if (useScreenFadeOnExit && FadeController.Instance != null)
            {
                FadeController.Instance.SetFadeColor(exitFadeColor);
                FadeController.Instance.SetAlpha(0f);
                FadeController.Instance.FadeOut(() =>
                {
                    SceneManager.sceneLoaded += OnMenuLoaded;
                    SceneManager.LoadScene(menuSceneName);
                }, exitFadeDuration);
            }
            else
            {
                SceneManager.LoadScene(menuSceneName);
            }
        }

        void OnMenuLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnMenuLoaded;

            if (!fadeInAfterLoad)
                return;

            if (FadeController.Instance != null)
            {
                FadeController.Instance.SetFadeColor(exitFadeColor);
                FadeController.Instance.SetAlpha(1f);
                FadeController.Instance.FadeIn(null, fadeInDuration);
            }
        }
    }
}