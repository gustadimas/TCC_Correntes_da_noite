using CorrentesDaNoite.Checkpoint;
using UnityEngine;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class MenuSaveIntegration : MonoBehaviour
    {
        [Header("Botoes do Menu")]
        [SerializeField] Button continueButton;
        [SerializeField] Button newGameButton;

        [Header("Configuracoes")]
        [SerializeField] string gameplaySceneName = "Gameplay";
        [SerializeField] string introSceneName = "Introducao";
        [SerializeField] bool usePersistentSave = true;
        [SerializeField] MenuGameStarter _menuStarter;

        ISaveSystem _saveSystem;

        private void Start()
        {
            _saveSystem = SaveSystemProvider.Get(usePersistentSave);
            continueButton.gameObject.SetActive(_saveSystem.HasSaveData());
            continueButton.onClick.AddListener(ContinueGame);
            newGameButton.onClick.AddListener(StartNewGame);
        }

        // o gameplay le o pedido e comeca no inicio da fase do checkpoint salvo
        void ContinueGame()
        {
            PhaseStart.ContinueRequested = true;
            _menuStarter.StartGame(gameplaySceneName);
        }

        void StartNewGame()
        {
            _saveSystem.ClearSaveData();
            _menuStarter.StartGame(introSceneName);
        }
    }
}