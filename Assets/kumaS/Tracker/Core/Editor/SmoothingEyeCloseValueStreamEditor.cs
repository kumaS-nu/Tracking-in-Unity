using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(SmoothingEyeCloseValueStream))]
    public class SmoothingEyeCloseValueStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(SmoothingEyeCloseValueStream.bufferSize)] = serializedObject.FindProperty(nameof(SmoothingEyeCloseValueStream.bufferSize));
            property[nameof(SmoothingEyeCloseValueStream.resetKey)] = serializedObject.FindProperty(nameof(SmoothingEyeCloseValueStream.resetKey));
            property[nameof(SmoothingEyeCloseValueStream.isDebug)] = serializedObject.FindProperty(nameof(SmoothingEyeCloseValueStream.isDebug));
            property[nameof(SmoothingEyeCloseValueStream.isDebugValue)] = serializedObject.FindProperty(nameof(SmoothingEyeCloseValueStream.isDebugValue));
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
                EditorGUILayout.PropertyField(property[nameof(SmoothingEyeCloseValueStream.isDebug)], new GUIContent("Debug"));
                if (((SmoothingEyeCloseValueStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingEyeCloseValueStream.isDebugValue)], new GUIContent("Value"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Smoothing setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(SmoothingEyeCloseValueStream.bufferSize)].intValue = EditorGUILayout.IntSlider("Buffer size", property[nameof(SmoothingEyeCloseValueStream.bufferSize)].intValue, 1, 32);
                EditorGUILayout.PropertyField(property[nameof(SmoothingEyeCloseValueStream.resetKey)], new GUIContent("Reset key"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
