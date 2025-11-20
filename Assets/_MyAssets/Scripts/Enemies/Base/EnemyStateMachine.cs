using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyStateMachine
    {
        EnemyState _currentState;

        public EnemyState CurrentState => _currentState;

        public void Initialize(EnemyState startingState)
        {
            _currentState = startingState;
            _currentState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void Update() => _currentState?.Update();

        public void FixedUpdate() => _currentState?.FixedUpdate();
    }
}