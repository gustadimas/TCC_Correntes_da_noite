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
        Animator _grabbed;
        float _grabWeight;

        // meio das maos da animacao antes do ik
        public Vector3 AnimatedHands { get; private set; }

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

        // maos dele vao pro corpo dela na captura
        private void OnAnimatorIK(int layerIndex)
        {
            AnimatedHands = (animator.GetIKPosition(AvatarIKGoal.LeftHand) + animator.GetIKPosition(AvatarIKGoal.RightHand)) * 0.5f;
            if (_grabbed == null || _grabWeight <= 0f) return;

            Vector3 chest = _grabbed.GetBoneTransform(HumanBodyBones.Chest).position;
            float halfWidth = Vector3.Distance(_grabbed.GetBoneTransform(HumanBodyBones.LeftUpperArm).position, _grabbed.GetBoneTransform(HumanBodyBones.RightUpperArm).position) * 0.4f;
            // cada mao de um lado dela visto pelos ombros dele
            Vector3 side = (animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position - animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position).normalized * halfWidth;
            SetHand(AvatarIKGoal.LeftHand, chest + side);
            SetHand(AvatarIKGoal.RightHand, chest - side);
        }

        public void Grab(Animator grabbed, float weight)
        {
            _grabbed = grabbed;
            _grabWeight = weight;
        }

        void SetHand(AvatarIKGoal hand, Vector3 position)
        {
            animator.SetIKPositionWeight(hand, _grabWeight);
            animator.SetIKPosition(hand, position);
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

        // vai direto pro estado de captura de qualquer animacao
        public virtual void SetCapture()
        {
            if (animator == null) return;

            int state = Animator.StringToHash(captureTriggerName);
            if (animator.HasState(0, state)) animator.CrossFadeInFixedTime(state, 0.1f);
            else if (HasParameter(captureTriggerName, AnimatorControllerParameterType.Trigger)) animator.SetTrigger(captureTriggerName);
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