using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(EyeCloseValueToMPDStream))]
    public class EyeCloseValueToMPDStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(EyeCloseValueToMPDStream.isDebug)] = serializedObject.FindProperty(nameof(EyeCloseValueToMPDStream.isDebug));
            property[nameof(EyeCloseValueToMPDStream.isDebugValue)] = serializedObject.FindProperty(nameof(EyeCloseValueToMPDStream.isDebugValue));
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
                EditorGUILayout.PropertyField(property[nameof(EyeCloseValueToMPDStream.isDebug)], new GUIContent("Debug"));
                if (((EyeCloseValueToMPDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(EyeCloseValueToMPDStream.isDebugValue)], new GUIContent("Value"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
