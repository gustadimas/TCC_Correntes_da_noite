using System.Collections;
using CorrentesDaNoite.Checkpoint;
using CorrentesDaNoite.Chase;
using CorrentesDaNoite.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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

        [Header("UI References")]
        [SerializeField] MenuTransitionManager menuManager;
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

        const string PAUSE_MENU_NAME = "PauseMenuScreen";

        bool _isPaused;
        float _timeScaleBeforePause = 1f;
        PlayerInputHandler _playerInput;
        PlayerController _playerController;
        PlayerDeath _playerDeath;
        bool _playerInputWasEnabled;
        Coroutine _inputRoutine;

        private void Start()
        {
            _playerInput = playerObject.GetComponent<PlayerInputHandler>();
            _playerController = playerObject.GetComponent<PlayerController>();
            _playerDeath = playerObject.GetComponent<PlayerDeath>();

            resumeButton.onClick.AddListener(Resume);
            restartCheckpointButton.onClick.AddListener(RestartFromCheckpoint);
            settingsButton.onClick.AddListener(OpenSettings);
            exitToMainMenuButton.onClick.AddListener(LoadMainMenu);
            exitToDesktopButton.onClick.AddListener(ExitToDesktop);

            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
            SetPanelVisible(false);
        }

        private void OnDestroy()
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
            if (_isPaused) Unpause();
        }

        // o B do controle cai no cancel da ui e tem que fazer o mesmo que o esc mas o esc tambem e cancel
        private void Update()
        {
            if (_isPaused && !pauseAction.action.WasPressedThisFrame() && EventSystem.current.currentInputModule is InputSystemUIInputModule module && module.cancel.action.WasPressedThisFrame()) HandlePauseInput();
        }

        void OnPausePerformed(InputAction.CallbackContext context) => HandlePauseInput();

        // esc nas configuracoes volta pro pause e no meio da troca de tela nao faz nada
        void HandlePauseInput()
        {
            if (menuManager.IsTransitioning) return;
            if (!_isPaused) Pause();
            else if (menuManager.CurrentMenuName == settingsMenuName) menuManager.ShowMenu(PAUSE_MENU_NAME);
            else Resume();
        }

        public void Pause()
        {
            if (_isPaused) return;

            _isPaused = true;
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;

            // se pausou de novo antes do input voltar o estado guardado continua valendo
            if (_inputRoutine != null) StopCoroutine(_inputRoutine);
            else _playerInputWasEnabled = _playerInput.enabled;
            _inputRoutine = null;
            _playerInput.enabled = false;

            SetPanelVisible(true);
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
        }

        public void Resume()
        {
            if (!_isPaused) return;

            Unpause();
            SetPanelVisible(false);
            EventSystem.current.SetSelectedGameObject(null);
            _inputRoutine = StartCoroutine(RestorePlayerInput());
        }

        // espera um pouco pro enter ou espaco do botao nao virar pulo
        IEnumerator RestorePlayerInput()
        {
            yield return new WaitForSecondsRealtime(inputDelayAfterResume);
            _playerInput.enabled = _playerInputWasEnabled;
            _inputRoutine = null;
        }

        void Unpause()
        {
            _isPaused = false;
            Time.timeScale = _timeScaleBeforePause;
            AudioListener.pause = false;
        }

        void RestartFromCheckpoint()
        {
            if (IsBlockingRespawn()) return;

            Unpause();
            SetPanelVisible(false);
            _playerInput.enabled = _playerInputWasEnabled;
            CheckpointManager.Instance.RespawnPlayer(playerObject, true);
        }

        void OpenSettings()
        {
            menuManager.ShowMenu(settingsMenuName);
            SetPanelVisible(false);
        }

        void LoadMainMenu()
        {
            Unpause();
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        void ExitToDesktop()
        {
            Unpause();
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void SetPanelVisible(bool visible)
        {
            panelRoot.SetActive(visible);
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            if (visible) restartCheckpointButton.gameObject.SetActive(!IsBlockingRespawn());
        }

        bool IsBlockingRespawn() => gameStartSequence.isActiveAndEnabled || jungleChaseSequence.isActiveAndEnabled || _playerController.IsCaptured || _playerDeath.IsCaptureInProgress;
    }
}