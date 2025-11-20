using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyStateMachine
    {
        EnemyState currentState;

        public EnemyState CurrentState => currentState;

        public void Initialize(EnemyState startingState)
        {
            currentState = startingState;
            currentState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        public void Update() => currentState?.Update();

        public void FixedUpdate() => currentState?.FixedUpdate();
    }
}