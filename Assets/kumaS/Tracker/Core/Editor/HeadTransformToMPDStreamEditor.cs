using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    public class HeadTransformToMPDStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(HeadTransformToMPDStream.isDebug)] = serializedObject.FindProperty(nameof(HeadTransformToMPDStream.isDebug));
            property[nameof(HeadTransformToMPDStream.isDebugHead)] = serializedObject.FindProperty(nameof(HeadTransformToMPDStream.isDebugHead));
            property[nameof(HeadTransformToMPDStream.center)] = serializedObject.FindProperty(nameof(HeadTransformToMPDStream.center));
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
                EditorGUILayout.PropertyField(property[nameof(HeadTransformToMPDStream.isDebug)], new GUIContent("Debug"));
                if (((HeadTransformToMPDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(HeadTransformToMPDStream.isDebugHead)], new GUIContent("Head"));
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
