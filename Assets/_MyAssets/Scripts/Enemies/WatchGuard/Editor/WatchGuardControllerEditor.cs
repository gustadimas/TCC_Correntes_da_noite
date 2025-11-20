#if UNITY_EDITOR
using UnityEditor;

namespace CorrentesDaNoite.Enemies
{
    [CustomEditor(typeof(WatchGuardController))]
    [CanEditMultipleObjects]
    public class WatchGuardControllerEditor : Editor
    {
        SerializedProperty movement;
        SerializedProperty animationController;
        SerializedProperty guardPoints;
        SerializedProperty lookTargets;
        SerializedProperty guardMoveSpeed;
        SerializedProperty arrivalThreshold;
        SerializedProperty lookRotationSpeed;
        SerializedProperty lookDuration;
        SerializedProperty lookAlignmentTolerance;
        SerializedProperty loopGuardPoints;

        void OnEnable()
        {
            if (target == null || serializedObject == null || serializedObject.targetObject == null)
                return;

            movement = serializedObject.FindProperty("movement");
            animationController = serializedObject.FindProperty("animationController");
            guardPoints = serializedObject.FindProperty("guardPoints");
            lookTargets = serializedObject.FindProperty("lookTargets");
            guardMoveSpeed = serializedObject.FindProperty("guardMoveSpeed");
            arrivalThreshold = serializedObject.FindProperty("arrivalThreshold");
            lookRotationSpeed = serializedObject.FindProperty("lookRotationSpeed");
            lookDuration = serializedObject.FindProperty("lookDuration");
            lookAlignmentTolerance = serializedObject.FindProperty("lookAlignmentTolerance");
            loopGuardPoints = serializedObject.FindProperty("loopGuardPoints");
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null || serializedObject.targetObject == null)
                return;

            serializedObject.Update();

            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movement);
            EditorGUILayout.PropertyField(animationController);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Guard Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(guardPoints, true);
            EditorGUILayout.PropertyField(lookTargets, true);
            EditorGUILayout.PropertyField(guardMoveSpeed);
            EditorGUILayout.PropertyField(arrivalThreshold);
            EditorGUILayout.PropertyField(loopGuardPoints);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Looking", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(lookRotationSpeed);
            EditorGUILayout.PropertyField(lookDuration);
            EditorGUILayout.PropertyField(lookAlignmentTolerance);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif