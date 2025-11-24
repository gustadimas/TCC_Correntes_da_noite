using CorrentesDaNoite.Checkpoint;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class MenuSaveIntegration : MonoBehaviour
    {
        [Header("Botoes do Menu")]
        [SerializeField] Button continueButton;
        [SerializeField] Button newGameButton;
        [SerializeField] GameObject continueButtonObject;

        [Header("Configuracoes")]
        [SerializeField] string gameplaySceneName = "Gameplay";
        [SerializeField] string introSceneName = "Introducao";
        [SerializeField] bool usePersistentSave = true;

        [Header("Confirmacao de Novo Jogo")]
        [SerializeField] bool askConfirmationIfHasSave = true;
        [SerializeField] GameObject confirmationPanel;

        [Header("Fade (Opcional)")]
        [SerializeField] FadeController fadeController;
        [SerializeField] float fadeDuration = 1f;

        ISaveSystem _saveSystem;

        private void Start()
        {
            _saveSystem = SaveSystemProvider.Get(usePersistentSave);

            SetupButtons();
            UpdateContinueButtonVisibility();
        }

        void SetupButtons()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueGame);
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }
        }

        void UpdateContinueButtonVisibility()
        {
            bool hasSave = _saveSystem.HasSaveData();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (continueButtonObject != null)
            {
                continueButtonObject.SetActive(hasSave);
            }

            Debug.Log($"[MenuSaveIntegration] Save encontrado: {hasSave}");
        }

        public void ContinueGame()
        {
            if (!_saveSystem.HasSaveData())
            {
                Debug.LogWarning("[MenuSaveIntegration] Tentou continuar mas nao ha save!");
                return;
            }

            string checkpointId = _saveSystem.LoadCheckpoint();
            Debug.Log($"[MenuSaveIntegration] Continuando do checkpoint: {checkpointId}");

            LoadScene(gameplaySceneName);
        }

        void OnNewGameClicked()
        {
            if (askConfirmationIfHasSave && _saveSystem.HasSaveData())
            {
                ShowConfirmationPanel();
            }
            else
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            Debug.Log("[MenuSaveIntegration] Iniciando novo jogo...");

            _saveSystem.ClearSaveData();
            LoadScene(introSceneName);
        }

        void LoadScene(string sceneName)
        {
            if (fadeController != null)
            {
                fadeController.FadeOut(() =>
                {
                    SceneManager.LoadScene(sceneName);
                }, fadeDuration);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        void ShowConfirmationPanel()
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }

        public void HideConfirmationPanel()
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
        }

        public void ConfirmNewGame()
        {
            HideConfirmationPanel();
            StartNewGame();
        }

        public void CancelNewGame()
        {
            HideConfirmationPanel();
        }

        public void DeleteSaveData()
        {
            _saveSystem.ClearSaveData();
            UpdateContinueButtonVisibility();
            Debug.Log("[MenuSaveIntegration] Save deletado!");
        }

        public bool HasSaveData()
        {
            return _saveSystem.HasSaveData();
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool showDebugButtons;

        private void OnGUI()
        {
            if (!showDebugButtons)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.Label("Menu Save Debug:");

            if (GUILayout.Button("Check Save"))
            {
                bool hasSave = _saveSystem.HasSaveData();
                Debug.Log($"Has Save: {hasSave}");
                if (hasSave)
                {
                    Debug.Log($"Checkpoint: {_saveSystem.LoadCheckpoint()}");
                }
            }

            if (GUILayout.Button("Clear Save"))
            {
                DeleteSaveData();
            }

            if (GUILayout.Button("Update UI"))
            {
                UpdateContinueButtonVisibility();
            }

            GUILayout.EndArea();
        }
#endif
    }
}