using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(EyeRotationToMPDStream))]
    public class EyeRotationToMPDStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(EyeRotationToMPDStream.isDebug)] = serializedObject.FindProperty(nameof(EyeRotationToMPDStream.isDebug));
            property[nameof(EyeRotationToMPDStream.isDebugRotation)] = serializedObject.FindProperty(nameof(EyeRotationToMPDStream.isDebugRotation));
            property[nameof(EyeRotationToMPDStream.forward)] = serializedObject.FindProperty(nameof(EyeRotationToMPDStream.forward));
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
                EditorGUILayout.PropertyField(property[nameof(EyeRotationToMPDStream.isDebug)], new GUIContent("Debug"));
                if (((EyeRotationToMPDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(EyeRotationToMPDStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(EyeRotationToMPDStream.forward)], new GUIContent("Forward"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
