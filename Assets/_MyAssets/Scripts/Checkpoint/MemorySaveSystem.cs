namespace CorrentesDaNoite.Checkpoint
{
    public class MemorySaveSystem : ISaveSystem
    {
        string _savedCheckpointId;

        public void SaveCheckpoint(string checkpointId) => _savedCheckpointId = checkpointId;

        public string LoadCheckpoint()
        {
            return _savedCheckpointId;
        }

        public bool HasSaveData()
        {
            return !string.IsNullOrEmpty(_savedCheckpointId);
        }

        public void ClearSaveData() => _savedCheckpointId = null;
    }
}