using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class SleepingEnemyAnimationController : EnemyAnimationController
    {
        public void SetSleeping(bool sleeping, string sleepingParam)
        {
            if (animator == null || string.IsNullOrEmpty(sleepingParam)) return;
            if (HasParameter(sleepingParam, AnimatorControllerParameterType.Bool))
                animator.SetBool(sleepingParam, sleeping);
        }

        public void TriggerStartled(string triggerParam)
        {
            if (animator == null || string.IsNullOrEmpty(triggerParam)) return;
            if (HasParameter(triggerParam, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(triggerParam);
        }

        public void SetIdleReady(bool value, string idleReadyParam)
        {
            if (animator == null || string.IsNullOrEmpty(idleReadyParam)) return;
            if (HasParameter(idleReadyParam, AnimatorControllerParameterType.Bool))
                animator.SetBool(idleReadyParam, value);
        }
    }
}