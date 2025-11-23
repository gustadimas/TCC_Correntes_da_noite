using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class MenuSetup : MonoBehaviour
    {
        [SerializeField] MenuTransitionManager menuManager;

        private void Start()
        {
            if (menuManager == null)
            {
                menuManager = FindFirstObjectByType<MenuTransitionManager>();
                if (menuManager == null)
                {
                    Debug.LogWarning("MenuTransitionManager not found");
                }
            }
        }

        private void ShowMenuSafe(string menuName)
        {
            if (menuManager == null)
            {
                Debug.LogWarning($"[MenuSetup] MenuTransitionManager nao encontrado! Nao e possivel ir para '{menuName}'");
                return;
            }

            menuManager.ShowMenu(menuName);
        }

        public void GoToMainMenu() => ShowMenuSafe("MainMenuScreen");
        public void GoToSettings() => ShowMenuSafe("SettingsScreen");
        public void GoToCredits() => ShowMenuSafe("CreditsScreen");
        public void GoToCutsceneMenu() => ShowMenuSafe("CutsceneScreen");
        public void GoToLevelSelectionMenu() => ShowMenuSafe("LevelSelectionScreen");
        public void GoToLoadingMenu() => ShowMenuSafe("LoadingScreen");
        public void ExitToMainMenu() => ShowMenuSafe("MainMenuScreen");

        public void HideAllMenus()
        {
            if (menuManager != null)
            {
                menuManager.HideCurrentMenu();
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            Debug.Log("Game is quitting");
        }
    }
}