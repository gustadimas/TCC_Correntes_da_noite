using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class WatchGuardAnimationController : EnemyAnimationController
    {
        [Header("WatchGuard Animation")]
        [SerializeField] protected string watchingBoolParam = "isWatching";
        [SerializeField] protected string lookTriggerParam = "";
        [SerializeField] protected string alertTriggerParam = "";

        public string WatchingBoolParam => watchingBoolParam;
        public string LookTriggerParam => lookTriggerParam;
        public string AlertTriggerParam => alertTriggerParam;

        public virtual void SetWatching(bool watching)
        {
            if (animator == null || string.IsNullOrEmpty(watchingBoolParam))
                return;

            if (HasParameter(watchingBoolParam, AnimatorControllerParameterType.Bool))
                animator.SetBool(watchingBoolParam, watching);
        }

        public virtual void TriggerLook()
        {
            if (animator == null || string.IsNullOrEmpty(lookTriggerParam))
                return;

            if (HasParameter(lookTriggerParam, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(lookTriggerParam);
        }

        public virtual void ResetAlertTrigger()
        {
            if (animator == null || string.IsNullOrEmpty(alertTriggerParam))
                return;

            if (HasParameter(alertTriggerParam, AnimatorControllerParameterType.Trigger))
                animator.ResetTrigger(alertTriggerParam);
        }
    }
}