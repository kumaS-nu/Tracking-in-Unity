using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(EyeCloseValueToPMDStream))]
    public class EyeCloseValueToPMDStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(EyeCloseValueToPMDStream.isDebug)] = serializedObject.FindProperty(nameof(EyeCloseValueToPMDStream.isDebug));
            property[nameof(EyeCloseValueToPMDStream.isDebugValue)] = serializedObject.FindProperty(nameof(EyeCloseValueToPMDStream.isDebugValue));
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
                EditorGUILayout.PropertyField(property[nameof(EyeCloseValueToPMDStream.isDebug)], new GUIContent("Debug"));
                if (((EyeCloseValueToPMDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(EyeCloseValueToPMDStream.isDebugValue)], new GUIContent("Value"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
