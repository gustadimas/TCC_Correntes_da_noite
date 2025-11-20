using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public abstract class EnemyState
    {
        protected EnemyController _controller;
        protected EnemyStateMachine _stateMachine;

        protected EnemyState(EnemyController controller, EnemyStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}