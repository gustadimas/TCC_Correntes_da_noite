#if UNITY_EDITOR
using UnityEditor;

namespace CorrentesDaNoite.Enemies
{
    [CustomEditor(typeof(SleepingEnemyController))]
    [CanEditMultipleObjects]
    public class SleepingEnemyControllerEditor : Editor
    {
        SerializedProperty movement;
        SerializedProperty animationController;
        SerializedProperty sleepingAnimationController;

        SerializedProperty chaseSpeed;
        SerializedProperty captureDistance;
        SerializedProperty playerHoldPoint;

        SerializedProperty sleepingBoolParam;
        SerializedProperty startledTriggerParam;
        SerializedProperty idleReadyBoolParam;

        SerializedProperty startledDuration;

        void OnEnable()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return;

            movement = serializedObject.FindProperty("movement");
            animationController = serializedObject.FindProperty("animationController");
            sleepingAnimationController = serializedObject.FindProperty("sleepingAnimationController");

            chaseSpeed = serializedObject.FindProperty("chaseSpeed");
            captureDistance = serializedObject.FindProperty("captureDistance");
            playerHoldPoint = serializedObject.FindProperty("playerHoldPoint");

            sleepingBoolParam = serializedObject.FindProperty("sleepingBoolParam");
            startledTriggerParam = serializedObject.FindProperty("startledTriggerParam");
            idleReadyBoolParam = serializedObject.FindProperty("idleReadyBoolParam");

            startledDuration = serializedObject.FindProperty("startledDuration");
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return;

            serializedObject.Update();

            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movement);
            EditorGUILayout.PropertyField(animationController);
            EditorGUILayout.PropertyField(sleepingAnimationController);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chase Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(chaseSpeed);
            EditorGUILayout.PropertyField(captureDistance);
            EditorGUILayout.PropertyField(playerHoldPoint);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sleepingBoolParam);
            EditorGUILayout.PropertyField(startledTriggerParam);
            EditorGUILayout.PropertyField(idleReadyBoolParam);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(startledDuration);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif