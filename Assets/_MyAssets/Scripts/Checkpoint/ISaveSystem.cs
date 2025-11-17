namespace CorrentesDaNoite.Checkpoint
{
    public interface ISaveSystem
    {
        void SaveCheckpoint(string checkpointId);
        string LoadCheckpoint();
        bool HasSaveData();
        void ClearSaveData();
    }
}