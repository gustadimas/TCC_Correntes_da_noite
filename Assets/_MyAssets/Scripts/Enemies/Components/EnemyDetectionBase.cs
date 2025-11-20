using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public abstract class EnemyDetectionBase : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected LayerMask targetLayer;
        [SerializeField] protected LayerMask obstacleLayer;
        [SerializeField] protected bool detectionEnabled = true;

        protected Transform detectedTarget;
        protected EnemyController enemyController;

        public float DetectionRadius => detectionRadius;
        public Transform DetectedTarget => detectedTarget;
        public bool HasDetectedTarget => detectedTarget != null;
        public bool DetectionEnabled => detectionEnabled;

        protected virtual void Awake()
        {
            enemyController = GetComponentInParent<EnemyController>();
            if (enemyController == null)
                enemyController = GetComponent<EnemyController>();
        }

        public virtual bool CheckForTarget()
        {
            return false;
        }

        public virtual void ClearDetection() => detectedTarget = null;

        public abstract void EnableDetection(bool enable);
    }
}