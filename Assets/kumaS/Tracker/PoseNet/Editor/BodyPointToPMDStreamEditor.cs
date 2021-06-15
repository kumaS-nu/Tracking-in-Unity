using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.PoseNet.Editor
{
    [CustomEditor(typeof(BodyPointToPMDStream))]
    public class BodyPointToPMDStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(BodyPointToPMDStream.isDebug)] = serializedObject.FindProperty(nameof(BodyPointToPMDStream.isDebug));
            property[nameof(BodyPointToPMDStream.isDebugPosition)] = serializedObject.FindProperty(nameof(BodyPointToPMDStream.isDebugPosition));
            property[nameof(BodyPointToPMDStream.isDebugRotation)] = serializedObject.FindProperty(nameof(BodyPointToPMDStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(BodyPointToPMDStream.isDebug)], new GUIContent("Debug"));
                if (((BodyPointToPMDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(BodyPointToPMDStream.isDebugPosition)], new GUIContent("Position"));
                        EditorGUILayout.PropertyField(property[nameof(BodyPointToPMDStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
