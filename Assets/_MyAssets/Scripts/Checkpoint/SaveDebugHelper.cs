using UnityEngine;

namespace CorrentesDaNoite.Checkpoint
{
    public class SaveDebugHelper : MonoBehaviour
    {
        [Header("Debug Keys")]
        [SerializeField] KeyCode clearSaveKey = KeyCode.F9;
        [SerializeField] KeyCode showSaveInfoKey = KeyCode.F10;

        void Update()
        {
            if (Input.GetKeyDown(clearSaveKey)) ClearSaveData();
            if (Input.GetKeyDown(showSaveInfoKey)) ShowSaveInfo();
        }

        [ContextMenu("Clear Save Data")]
        void ClearSaveData()
        {
            CheckpointManager.Instance?.ClearSaveData();
            Debug.Log("Save data cleared!");
        }

        [ContextMenu("Show Save Info")]
        void ShowSaveInfo()
        {
            if (CheckpointManager.Instance == null)
            {
                Debug.Log("CheckpointManager not found!");
                return;
            }

            var saveSystem = CheckpointManager.Instance.SaveSystem;
            bool hasSave = saveSystem.HasSaveData();
            string savedCheckpoint = hasSave ? saveSystem.LoadCheckpoint() : "None";
            string currentCheckpoint = CheckpointManager.Instance.CurrentCheckpoint != null
                ? CheckpointManager.Instance.CurrentCheckpoint.CheckpointId
                : "None";

            Debug.Log($"=== SAVE INFO ===\n" +
                     $"Has Save: {hasSave}\n" +
                     $"Saved Checkpoint: {savedCheckpoint}\n" +
                     $"Current Checkpoint: {currentCheckpoint}");
        }
    }
}