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

        protected Transform _detectedTarget;
        protected EnemyController _enemyController;

        public float DetectionRadius => detectionRadius;
        public Transform DetectedTarget => _detectedTarget;
        public bool HasDetectedTarget => _detectedTarget != null;
        public bool DetectionEnabled => detectionEnabled;

        protected virtual void Awake()
        {
            _enemyController = GetComponentInParent<EnemyController>();
            if (_enemyController == null)
                _enemyController = GetComponent<EnemyController>();
        }

        public virtual bool CheckForTarget()
        {
            return false;
        }

        public virtual void ClearDetection() => _detectedTarget = null;

        public abstract void EnableDetection(bool enable);
    }
}