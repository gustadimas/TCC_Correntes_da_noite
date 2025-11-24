namespace CorrentesDaNoite.Checkpoint
{
    /// <summary>
    /// Centraliza a escolha e o compartilhamento do ISaveSystem para evitar instâncias divergentes.
    /// </summary>
    public static class SaveSystemProvider
    {
        static ISaveSystem _persistent;
        static ISaveSystem _memory;

        public static ISaveSystem Get(bool usePersistent)
        {
            if (usePersistent)
                return _persistent ??= new PlayerPrefsSaveSystem();

            return _memory ??= new MemorySaveSystem();
        }

        public static void ClearAll()
        {
            _persistent?.ClearSaveData();
            _memory?.ClearSaveData();
        }
    }
}
