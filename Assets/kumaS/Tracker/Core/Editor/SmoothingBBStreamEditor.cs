using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(SmoothingBBStream))]
    public class SmoothingBBStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(SmoothingBBStream.isDebug)] = serializedObject.FindProperty(nameof(SmoothingBBStream.isDebug));
            property[nameof(SmoothingBBStream.isDebugBox)] = serializedObject.FindProperty(nameof(SmoothingBBStream.isDebugBox));
            property[nameof(SmoothingBBStream.bufferSize)] = serializedObject.FindProperty(nameof(SmoothingBBStream.bufferSize));
            property[nameof(SmoothingBBStream.speedLimit)] = serializedObject.FindProperty(nameof(SmoothingBBStream.speedLimit));
            property[nameof(SmoothingBBStream.resetKey)] = serializedObject.FindProperty(nameof(SmoothingBBStream.resetKey));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            ((SmoothingBBStream)target).ProcessName = EditorGUILayout.TextField("Process name", ((SmoothingBBStream)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(SmoothingBBStream.isDebug)], new GUIContent("Debug"));
                if (((SmoothingBBStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBBStream.isDebugBox)], new GUIContent("Boundy box"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Smoothing setting", EditorStyles.boldLabel);
            using(new EditorGUI.IndentLevelScope())
            {
                property[nameof(SmoothingBBStream.bufferSize)].intValue = EditorGUILayout.IntSlider("Buffer size", property[nameof(SmoothingBBStream.bufferSize)].intValue, 1, 32);
                property[nameof(SmoothingBBStream.speedLimit)].floatValue = EditorGUILayout.Slider(new GUIContent("Speed limit", "per frame"), property[nameof(SmoothingBBStream.speedLimit)].floatValue, 0, 1);
                EditorGUILayout.PropertyField(property[nameof(SmoothingBBStream.resetKey)], new GUIContent("Reset key"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
