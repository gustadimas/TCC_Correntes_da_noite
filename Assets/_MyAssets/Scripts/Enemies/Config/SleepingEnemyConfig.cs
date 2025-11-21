using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    [CreateAssetMenu(menuName = "CorrentesDaNoite/Enemies/Sleeping Enemy Config", fileName = "SleepingEnemyConfig")]
    public class SleepingEnemyConfig : EnemyConfig
    {
        [Header("Sleeping Specific")]
        public float startledDuration = 1.5f;
        public float extraHearingRadius = 2f;
        public float sensitivityMultiplier = 1.2f;
        public float silenceTimeToSleep = 1f;
    }
}
