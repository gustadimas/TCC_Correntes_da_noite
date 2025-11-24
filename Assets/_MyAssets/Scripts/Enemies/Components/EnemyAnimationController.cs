using UnityEngine;

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

        protected virtual void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            _footstepEmitter = GetComponent<EnemyFootstepEmitter>() ?? GetComponentInParent<EnemyFootstepEmitter>();
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