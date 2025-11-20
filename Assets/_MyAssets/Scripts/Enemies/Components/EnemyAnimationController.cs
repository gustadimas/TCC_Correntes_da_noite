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

        protected virtual void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        public virtual void SetWalking(bool isWalking)
        {
            if (animator != null)
                animator.SetBool(walkingParameterName, isWalking);
        }

        public virtual void SetSpotted()
        {
            if (animator != null)
                animator.SetTrigger(spottedTriggerName);
        }

        public virtual void SetRunning(bool isRunning)
        {
            if (animator != null)
                animator.SetBool(isRunningParamName, isRunning);
        }

        public virtual void SetCapture()
        {
            if (animator != null)
                animator.SetTrigger(captureTriggerName);
        }

        public virtual void ResetAllTriggers()
        {
            if (animator != null)
            {
                animator.ResetTrigger(spottedTriggerName);
                animator.ResetTrigger(captureTriggerName);
            }
        }

        public virtual void ResetToIdle()
        {
            if (animator != null)
            {
                animator.SetBool(walkingParameterName, false);
                animator.SetBool(isRunningParamName, false);
                animator.ResetTrigger(spottedTriggerName);
                animator.ResetTrigger(captureTriggerName);
                animator.Rebind();
                animator.Update(0f);
            }
        }
    }
}