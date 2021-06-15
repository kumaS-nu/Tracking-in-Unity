using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(SmoothingHeadTransformStream))]
    public class SmoothingHeadTransformStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(SmoothingHeadTransformStream.isDebug)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.isDebug));
            property[nameof(SmoothingHeadTransformStream.isDebugHead)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.isDebugHead));
            property[nameof(SmoothingHeadTransformStream.bufferSize)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.bufferSize));
            property[nameof(SmoothingHeadTransformStream.moveSpeedLimit)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.moveSpeedLimit));
            property[nameof(SmoothingHeadTransformStream.moveRange)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.moveRange));
            property[nameof(SmoothingHeadTransformStream.rotateSpeedLimit)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.rotateSpeedLimit));
            property[nameof(SmoothingHeadTransformStream.rotateRange)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.rotateRange));
            property[nameof(SmoothingHeadTransformStream.resetKey)] = serializedObject.FindProperty(nameof(SmoothingHeadTransformStream.resetKey));
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
                EditorGUILayout.PropertyField(property[nameof(SmoothingHeadTransformStream.isDebug)], new GUIContent("Debug"));
                if (((SmoothingHeadTransformStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingHeadTransformStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Smoothing setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(SmoothingHeadTransformStream.bufferSize)].intValue = EditorGUILayout.IntSlider("Buffer size", property[nameof(SmoothingHeadTransformStream.bufferSize)].intValue, 1, 32);
                property[nameof(SmoothingHeadTransformStream.moveSpeedLimit)].floatValue = EditorGUILayout.Slider(new GUIContent("Move speed limit", "per frame"), property[nameof(SmoothingHeadTransformStream.moveSpeedLimit)].floatValue, 0, 1);
                property[nameof(SmoothingHeadTransformStream.moveRange)].floatValue = EditorGUILayout.Slider(new GUIContent("Move range"), property[nameof(SmoothingHeadTransformStream.moveRange)].floatValue, 0, 1);
                property[nameof(SmoothingHeadTransformStream.rotateSpeedLimit)].floatValue = EditorGUILayout.Slider(new GUIContent("Rotate speed limit", "per frame"), property[nameof(SmoothingHeadTransformStream.rotateSpeedLimit)].floatValue, 0, 90);
                property[nameof(SmoothingHeadTransformStream.rotateRange)].floatValue = EditorGUILayout.Slider(new GUIContent("Rotate range"), property[nameof(SmoothingHeadTransformStream.rotateRange)].floatValue, 0, 180);
                EditorGUILayout.PropertyField(property[nameof(SmoothingHeadTransformStream.resetKey)], new GUIContent("Reset key"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
