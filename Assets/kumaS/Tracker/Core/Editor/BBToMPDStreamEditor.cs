using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(BBToMPDStream))]
    public class BBToMPDStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(BBToMPDStream.moveScale)] = serializedObject.FindProperty(nameof(BBToMPDStream.moveScale));
            property[nameof(BBToMPDStream.center)] = serializedObject.FindProperty(nameof(BBToMPDStream.center));
            property[nameof(BBToMPDStream.isEnableDepth)] = serializedObject.FindProperty(nameof(BBToMPDStream.isEnableDepth));
            property[nameof(BBToMPDStream.depthCenter)] = serializedObject.FindProperty(nameof(BBToMPDStream.depthCenter));
            property[nameof(BBToMPDStream.isDebug)] = serializedObject.FindProperty(nameof(BBToMPDStream.isDebug));
            property[nameof(BBToMPDStream.isDebugBB)] = serializedObject.FindProperty(nameof(BBToMPDStream.isDebugBB));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            ((BBToMPDStream)target).ProcessName = EditorGUILayout.TextField("Process name", ((BBToMPDStream)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(BBToMPDStream.isDebug)], new GUIContent("Debug"));
                if (((BBToMPDStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(BBToMPDStream.isDebugBB)], new GUIContent("Boundy box"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(BBToMPDStream.moveScale)].floatValue = EditorGUILayout.Slider("Move scale", property[nameof(BBToMPDStream.moveScale)].floatValue, 0, 1);
                EditorGUILayout.PropertyField(property[nameof(BBToMPDStream.center)], new GUIContent("Center"));
                EditorGUILayout.PropertyField(property[nameof(BBToMPDStream.isEnableDepth)], new GUIContent("Enable depth"));
                if (property[nameof(BBToMPDStream.isEnableDepth)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        property[nameof(BBToMPDStream.depthCenter)].floatValue = EditorGUILayout.Slider("Depth center", property[nameof(BBToMPDStream.depthCenter)].floatValue, 0, 1);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
