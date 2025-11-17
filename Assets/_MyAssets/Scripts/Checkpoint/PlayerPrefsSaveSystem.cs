using UnityEngine;

namespace CorrentesDaNoite.Checkpoint
{
    public class PlayerPrefsSaveSystem : ISaveSystem
    {
        const string CHECKPOINT_KEY = "SavedCheckpoint";

        public void SaveCheckpoint(string checkpointId)
        {
            PlayerPrefs.SetString(CHECKPOINT_KEY, checkpointId);
            PlayerPrefs.Save();
        }

        public string LoadCheckpoint()
        {
            return PlayerPrefs.GetString(CHECKPOINT_KEY, string.Empty);
        }

        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(CHECKPOINT_KEY);
        }

        public void ClearSaveData()
        {
            PlayerPrefs.DeleteKey(CHECKPOINT_KEY);
            PlayerPrefs.Save();
        }
    }
}