using System.Collections;
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