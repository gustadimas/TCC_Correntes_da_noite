using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace CorrentesDaNoite
{
    public static class ActiveInputDevice
    {
        public static bool UsingGamepad { get; private set; }
        public static bool IsPlayStation => Gamepad.current is DualShockGamepad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            UsingGamepad = false;
            InputSystem.onActionChange -= OnActionChange;
            InputSystem.onActionChange += OnActionChange;
        }

        // a ultima acao feita diz se a pessoa esta no controle ou no teclado e mouse
        static void OnActionChange(object action, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed || !(action is InputAction inputAction) || inputAction.activeControl == null) return;

            bool gamepad = inputAction.activeControl.device is Gamepad;
            if (gamepad == UsingGamepad) return;

            UsingGamepad = gamepad;
            Cursor.visible = !gamepad;
        }
    }
}