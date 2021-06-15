using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(EyeRotationToPMDStream))]
    public class EyeRotationToPMDStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(EyeRotationToPMDStream.isDebug)] = serializedObject.FindProperty(nameof(EyeRotationToPMDStream.isDebug));
            property[nameof(EyeRotationToPMDStream.isDebugRotation)] = serializedObject.FindProperty(nameof(EyeRotationToPMDStream.isDebugRotation));
            property[nameof(EyeRotationToPMDStream.forward)] = serializedObject.FindProperty(nameof(EyeRotationToPMDStream.forward));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(EyeRotationToPMDStream.isDebug)], new GUIContent("Debug"));
                if (((EyeRotationToPMDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(EyeRotationToPMDStream.isDebugRotation)], new GUIContent("Rotation"));
                    }

                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(EyeRotationToPMDStream.forward)], new GUIContent("Forward"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
