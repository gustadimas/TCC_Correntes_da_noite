using UnityEngine;
using UnityEngine.AI;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected string walkingParameterName = "isWalking";
        [SerializeField] protected string spottedTriggerName = "Spotted";
        [SerializeField] protected string isRunningParamName = "isRunning";
        [SerializeField] protected string captureTriggerName = "Capture";

        EnemyFootstepEmitter _footstepEmitter;
        NavMeshAgent _agent;
        bool _hasLocomotionSpeed;

        static readonly int WalkSpeedHash = Animator.StringToHash("WalkSpeed");
        static readonly int RunSpeedHash = Animator.StringToHash("RunSpeed");
        // quanto os clips de andar e correr andam por segundo com escala 1 e speed 1
        const float WALK_CLIP_SPEED = 0.36f;
        const float RUN_CLIP_SPEED = 3.24f;

        protected virtual void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            _footstepEmitter = GetComponent<EnemyFootstepEmitter>() ?? GetComponentInParent<EnemyFootstepEmitter>();
            _agent = GetComponent<NavMeshAgent>();
            _hasLocomotionSpeed = animator != null && HasParameter("WalkSpeed", AnimatorControllerParameterType.Float) && HasParameter("RunSpeed", AnimatorControllerParameterType.Float);
        }

        // a perna acompanha a velocidade real pra nao deslizar
        private void Update()
        {
            if (!_hasLocomotionSpeed || _agent == null) return;

            float speed = _agent.velocity.magnitude / transform.lossyScale.y;
            animator.SetFloat(WalkSpeedHash, Mathf.Max(1f, speed / WALK_CLIP_SPEED));
            animator.SetFloat(RunSpeedHash, Mathf.Max(1f, speed / RUN_CLIP_SPEED));
        }

        public virtual void SetWalking(bool isWalking)
        {
            if (animator != null && HasParameter(walkingParameterName, AnimatorControllerParameterType.Bool))
                animator.SetBool(walkingParameterName, isWalking);
        }

        public virtual void SetSpotted()
        {
            if (animator != null && HasParameter(spottedTriggerName, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(spottedTriggerName);
        }

        public virtual void SetRunning(bool isRunning)
        {
            if (animator != null && HasParameter(isRunningParamName, AnimatorControllerParameterType.Bool))
                animator.SetBool(isRunningParamName, isRunning);
        }

        public virtual void SetCapture()
        {
            if (animator != null && HasParameter(captureTriggerName, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(captureTriggerName);
        }

        public virtual void ResetAllTriggers()
        {
            if (animator != null)
            {
                if (HasParameter(spottedTriggerName, AnimatorControllerParameterType.Trigger))
                    animator.ResetTrigger(spottedTriggerName);
                if (HasParameter(captureTriggerName, AnimatorControllerParameterType.Trigger))
                    animator.ResetTrigger(captureTriggerName);
            }
        }

        public virtual void ResetToIdle()
        {
            if (animator != null)
            {
                if (HasParameter(walkingParameterName, AnimatorControllerParameterType.Bool))
                    animator.SetBool(walkingParameterName, false);
                if (HasParameter(isRunningParamName, AnimatorControllerParameterType.Bool))
                    animator.SetBool(isRunningParamName, false);
                if (HasParameter(spottedTriggerName, AnimatorControllerParameterType.Trigger))
                    animator.ResetTrigger(spottedTriggerName);
                if (HasParameter(captureTriggerName, AnimatorControllerParameterType.Trigger))
                    animator.ResetTrigger(captureTriggerName);
                animator.Rebind();
                animator.Update(0f);
            }
        }

        protected bool HasParameter(string paramName, AnimatorControllerParameterType type)
        {
            if (animator == null || string.IsNullOrEmpty(paramName))
                return false;

            foreach (var param in animator.parameters)
            {
                if (param.name == paramName && param.type == type)
                    return true;
            }

            return false;
        }

        public void PlayStepAnimationEvent()
        {
            if (_footstepEmitter == null)
                _footstepEmitter = GetComponent<EnemyFootstepEmitter>() ?? GetComponentInParent<EnemyFootstepEmitter>();

            _footstepEmitter?.PlayFootstep();
        }

        public void NewEvent() { }
    }
}