using System.Collections;
using System.Collections.Generic;
using CorrentesDaNoite.Checkpoint;
using CorrentesDaNoite.Chase;
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
        [SerializeField] InputActionReference escapeAction;
        [SerializeField] bool useCanvasGroup = true;

        [Header("Click Blocking")]
        [SerializeField] bool blockClicksWhenOpen = true;

        [Header("UI References")]
        [SerializeField] GameObject buttonsContainer;
        [SerializeField] Image logoImage;
        [SerializeField] MenuTransitionManager menuManager;
        [SerializeField] MenuGameStarter menuStarter;
        [SerializeField] GameObject playerObject;
        [SerializeField] GameStartSequence gameStartSequence;
        [SerializeField] JungleChaseSequenceController jungleChaseSequence;

        [Header("Menu Buttons")]
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartCheckpointButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button exitToMainMenuButton;
        [SerializeField] Button exitToDesktopButton;
        [SerializeField] Button defaultSelectedButton;

        [Header("Scene Names")]
        [SerializeField] string mainMenuSceneName = "MainMenu";
        [SerializeField] string settingsMenuName = "SettingsScreen";

        [Header("Input Protection")]
        [SerializeField] float inputDelayAfterResume = 0.15f;

        bool _isPaused;
        bool _isConsumingInput;
        readonly HashSet<InputAction> _enabledActions = new HashSet<InputAction>();

        private void Start()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (menuManager == null)
            {
                menuManager = FindFirstObjectByType<MenuTransitionManager>();
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

            SetupButtons();

            if (blockClicksWhenOpen)
            {
                SetupClickBlocker();
            }

            EnableInputAction(pauseAction, "[PauseMenu] pauseAction e NULL! Configure no Inspector: Player > Pause");
            EnableInputAction(escapeAction, null);

            HidePanelImmediate();
        }

        private void OnDestroy()
        {
            DisableInputAction(pauseAction);
            DisableInputAction(escapeAction);
        }

        void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        void TogglePause()
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

        void EnableInputAction(InputActionReference actionRef, string warningMessage)
        {
            if (actionRef == null || actionRef.action == null)
            {
                if (!string.IsNullOrEmpty(warningMessage))
                {
                    Debug.LogWarning(warningMessage);
                }
                return;
            }

            if (_enabledActions.Contains(actionRef.action))
            {
                return;
            }

            actionRef.action.Enable();
            actionRef.action.performed += OnPausePerformed;
            _enabledActions.Add(actionRef.action);
        }

        void DisableInputAction(InputActionReference actionRef)
        {
            if (actionRef == null || actionRef.action == null)
            {
                return;
            }

            if (_enabledActions.Remove(actionRef.action))
            {
                actionRef.action.performed -= OnPausePerformed;
            }

            actionRef.action.Disable();
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
            UpdateRestartButtonVisibility();
            FocusDefaultButton();
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
            HidePanelImmediate();
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

            if (panelRoot != null)
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

            UpdateRestartButtonVisibility();
        }

        public bool IsPaused()
        {
            return _isPaused;
        }

        void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (restartCheckpointButton != null)
                restartCheckpointButton.onClick.AddListener(RestartFromCheckpoint);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettingsMenu);

            if (exitToMainMenuButton != null)
                exitToMainMenuButton.onClick.AddListener(LoadMainMenu);

            if (exitToDesktopButton != null)
                exitToDesktopButton.onClick.AddListener(ExitToDesktop);
        }

        void FocusDefaultButton()
        {
            if (EventSystem.current == null)
                return;

            var target = defaultSelectedButton != null ? defaultSelectedButton.gameObject : (resumeButton != null ? resumeButton.gameObject : null);
            if (target != null)
                EventSystem.current.SetSelectedGameObject(target);
        }

        void RestartFromCheckpoint()
        {
            var checkpointManager = CheckpointManager.Instance;
            var player = GetPlayerObject();

            if (IsBlockingRespawn())
            {
                if (restartCheckpointButton != null)
                {
                    restartCheckpointButton.gameObject.SetActive(false);
                }
                Debug.LogWarning("[PauseMenu] Restart do checkpoint bloqueado durante sequencias (GameStart ou JungleChase).");
                return;
            }

            if (checkpointManager == null || player == null)
            {
                Debug.LogWarning("[PauseMenu] Nao foi possivel reiniciar do checkpoint (CheckpointManager ou Player nao encontrados). Recarregando cena.");
                ReloadCurrentScene();
                return;
            }

            Time.timeScale = 1f;
            _isPaused = false;
            HidePanelImmediate();
            checkpointManager.RespawnPlayer(player, true);
            FocusDefaultButton();
        }

        void OpenSettingsMenu()
        {
            if (!string.IsNullOrEmpty(settingsMenuName))
            {
                var targetManager = menuManager != null ? menuManager : FindFirstObjectByType<MenuTransitionManager>();
                if (targetManager != null)
                {
                    targetManager.ShowMenu(settingsMenuName);
                    HidePanelImmediate();
                }
                else
                {
                    Debug.LogWarning("[PauseMenu] MenuTransitionManager nao encontrado; nao foi possivel abrir configuracoes.");
                }
            }
            else
            {
                Debug.LogWarning("[PauseMenu] settingsMenuName nao definido; nao foi possivel abrir configuracoes.");
            }
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

        void ExitToDesktop()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void UpdateRestartButtonVisibility()
        {
            if (restartCheckpointButton == null)
            {
                return;
            }

            bool canRestart = !IsBlockingRespawn();
            restartCheckpointButton.gameObject.SetActive(canRestart);
        }

        bool IsBlockingRespawn()
        {
            if (gameStartSequence != null && gameStartSequence.isActiveAndEnabled)
            {
                return true;
            }

            if (jungleChaseSequence != null && jungleChaseSequence.isActiveAndEnabled)
            {
                return true;
            }

            return false;
        }

        GameObject GetPlayerObject()
        {
            if (playerObject != null)
            {
                return playerObject;
            }

            var playerController = FindFirstObjectByType<Player.PlayerController>();
            if (playerController != null)
            {
                playerObject = playerController.gameObject;
                return playerObject;
            }

            playerObject = GameObject.FindGameObjectWithTag("Player");
            return playerObject;
        }

        private void ReloadCurrentScene()
        {
            HidePanelImmediate();
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
