#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public static class EnemyConfigMenu
    {
        [MenuItem("Assets/Create/CorrentesDaNoite/Enemy Config", priority = 2000)]
        public static void CreateEnemyConfig()
        {
            var asset = ScriptableObject.CreateInstance<EnemyConfig>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MyAssets/Data/Enemies/EnemyConfig.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        [MenuItem("Assets/Create/CorrentesDaNoite/WatchGuard Config", priority = 2001)]
        public static void CreateWatchGuardConfig()
        {
            var asset = ScriptableObject.CreateInstance<WatchGuardConfig>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MyAssets/Data/Enemies/WatchGuardConfig.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }

        [MenuItem("Assets/Create/CorrentesDaNoite/Sleeping Enemy Config", priority = 2002)]
        public static void CreateSleepingEnemyConfig()
        {
            var asset = ScriptableObject.CreateInstance<SleepingEnemyConfig>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_MyAssets/Data/Enemies/SleepingEnemyConfig.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}
#endif
