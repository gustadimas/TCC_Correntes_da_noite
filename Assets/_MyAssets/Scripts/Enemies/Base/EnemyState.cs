using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public abstract class EnemyState
    {
        protected EnemyController controller;
        protected EnemyStateMachine stateMachine;

        protected EnemyState(EnemyController controller, EnemyStateMachine stateMachine)
        {
            this.controller = controller;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}