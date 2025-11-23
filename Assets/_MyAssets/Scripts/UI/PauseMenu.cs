using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel Control")]
        [SerializeField] GameObject panelRoot;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] InputActionReference pauseAction;
        [SerializeField] bool useCanvasGroup = true;

        [Header("Click Blocking")]
        [SerializeField] bool blockClicksWhenOpen = true;

        [Header("UI References")]
        [SerializeField] GameObject buttonsContainer;
        [SerializeField] MenuGameStarter menuStarter;

        [Header("Menu Buttons")]
        [SerializeField] Button mainMenuButton;

        [Header("Header")]
        [SerializeField] TextMeshProUGUI headerText;

        [Header("Scene Names")]
        [SerializeField] string mainMenuSceneName = "MainMenu";

        [Header("Input Protection")]
        [SerializeField] float inputDelayAfterResume = 0.15f;

        bool _isPaused;
        bool _isConsumingInput;

        private void Start()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (useCanvasGroup && canvasGroup == null)
            {
                canvasGroup = panelRoot.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    Debug.LogWarning("[PauseMenu] useCanvasGroup esta ativo mas CanvasGroup nao foi encontrado. Desativando useCanvasGroup.");
                    useCanvasGroup = false;
                }
            }

            if (headerText != null && headerText.font == null)
            {
                Debug.LogWarning("[PauseMenu] headerText nao tem Font Asset atribuido! Use Tools > TextMeshPro Diagnostic para corrigir.");
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(LoadMainMenu);
            }

            if (blockClicksWhenOpen)
            {
                SetupClickBlocker();
            }

            if (pauseAction != null)
            {
                pauseAction.action.Enable();
                pauseAction.action.performed += OnPausePressed;
            }
            else
            {
                Debug.LogWarning("[PauseMenu] pauseAction e NULL! Configure no Inspector: Player > Pause");
            }

            HidePanelImmediate();
        }

        private void OnDestroy()
        {
            if (pauseAction != null)
            {
                pauseAction.action.performed -= OnPausePressed;
                pauseAction.action.Disable();
            }
        }

        void OnPausePressed(InputAction.CallbackContext context)
        {
            if (IsInMainMenu())
            {
                return;
            }

            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        bool IsInMainMenu()
        {
            if (menuStarter == null)
            {
                return false;
            }

            if (menuStarter.HasGameStarted())
            {
                return false;
            }

            return true;
        }

        public void Pause()
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;
            Time.timeScale = 0f;
            ShowPanelImmediate();
        }

        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;
            Time.timeScale = 1f;
            HidePanelImmediate();

            StartCoroutine(ResumeWithInputDelay());
        }

        IEnumerator ResumeWithInputDelay()
        {
            _isConsumingInput = true;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            yield return new WaitForSecondsRealtime(inputDelayAfterResume);

            _isConsumingInput = false;
        }

        public bool IsConsumingInput()
        {
            return _isConsumingInput;
        }

        void LoadMainMenu()
        {
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                return;
            }

            Time.timeScale = 1f;
            _isPaused = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        void HidePanelImmediate()
        {
            if (useCanvasGroup && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        void ShowPanelImmediate()
        {
            if (useCanvasGroup && canvasGroup != null)
            {
                panelRoot.SetActive(true);
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        public bool IsPaused()
        {
            return _isPaused;
        }

        void SetupClickBlocker()
        {
            if (panelRoot == null)
            {
                return;
            }

            if (panelRoot.GetComponent<UIClickBlocker>() == null)
            {
                panelRoot.AddComponent<UIClickBlocker>();
            }

            var image = panelRoot.GetComponent<Image>();
            if (image == null)
            {
                image = panelRoot.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0.7f);
                image.raycastTarget = false;
            }
            else
            {
                image.raycastTarget = false;
            }

            var rect = panelRoot.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}