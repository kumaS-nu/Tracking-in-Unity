using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(HeadTransformToPMDStream))]
    public class HeadTransformToPMDStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(HeadTransformToPMDStream.isDebug)] = serializedObject.FindProperty(nameof(HeadTransformToPMDStream.isDebug));
            property[nameof(HeadTransformToPMDStream.isDebugHead)] = serializedObject.FindProperty(nameof(HeadTransformToPMDStream.isDebugHead));
            property[nameof(HeadTransformToPMDStream.center)] = serializedObject.FindProperty(nameof(HeadTransformToPMDStream.center));
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
                EditorGUILayout.PropertyField(property[nameof(HeadTransformToPMDStream.isDebug)], new GUIContent("Debug"));
                if (((HeadTransformToPMDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(HeadTransformToPMDStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(HeadTransformToPMDStream.center)], new GUIContent("Center"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
