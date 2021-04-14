using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(SmoothingEyeRotationStream))]
    public class SmoothingEyeRotationStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(SmoothingEyeRotationStream.bufferSize)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.bufferSize));
            property[nameof(SmoothingEyeRotationStream.resetKey)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.resetKey));
            property[nameof(SmoothingEyeRotationStream.rotateSpeedLimit)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateSpeedLimit));
            property[nameof(SmoothingEyeRotationStream.rotateRangeLXMin)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeLXMin));
            property[nameof(SmoothingEyeRotationStream.rotateRangeLXMax)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeLXMax));
            property[nameof(SmoothingEyeRotationStream.rotateRangeLYMin)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeLYMin));
            property[nameof(SmoothingEyeRotationStream.rotateRangeLYMax)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeLYMax));
            property[nameof(SmoothingEyeRotationStream.rotateRangeRXMin)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeRXMin));
            property[nameof(SmoothingEyeRotationStream.rotateRangeRXMax)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeRXMax));
            property[nameof(SmoothingEyeRotationStream.rotateRangeRYMin)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeRYMin));
            property[nameof(SmoothingEyeRotationStream.rotateRangeRYMax)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.rotateRangeRYMax));
            property[nameof(SmoothingEyeRotationStream.isDebug)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.isDebug));
            property[nameof(SmoothingEyeRotationStream.isDebugRotation)] = serializedObject.FindProperty(nameof(SmoothingEyeRotationStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(SmoothingEyeRotationStream.isDebug)], new GUIContent("Debug"));
                if (((SmoothingEyeRotationStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingEyeRotationStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Smoothing setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(SmoothingEyeRotationStream.bufferSize)].intValue = EditorGUILayout.IntSlider("Buffer size", property[nameof(SmoothingEyeRotationStream.bufferSize)].intValue, 1, 32);
                property[nameof(SmoothingEyeRotationStream.rotateSpeedLimit)].floatValue = EditorGUILayout.Slider(new GUIContent("Rotate speed limit", "per frame"), property[nameof(SmoothingEyeRotationStream.rotateSpeedLimit)].floatValue, 0, 45);
                ShowMinMaxSlider("Left eye rotate x range", property[nameof(SmoothingEyeRotationStream.rotateRangeLXMin)], property[nameof(SmoothingEyeRotationStream.rotateRangeLXMax)]);
                ShowMinMaxSlider("Left eye rotate y range", property[nameof(SmoothingEyeRotationStream.rotateRangeLYMin)], property[nameof(SmoothingEyeRotationStream.rotateRangeLYMax)]);
                ShowMinMaxSlider("Right eye rotate x range", property[nameof(SmoothingEyeRotationStream.rotateRangeRXMin)], property[nameof(SmoothingEyeRotationStream.rotateRangeRXMax)]);
                ShowMinMaxSlider("Right eye rotate y range", property[nameof(SmoothingEyeRotationStream.rotateRangeRYMin)], property[nameof(SmoothingEyeRotationStream.rotateRangeRYMax)]);
                EditorGUILayout.PropertyField(property[nameof(SmoothingEyeRotationStream.resetKey)], new GUIContent("Reset key"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowMinMaxSlider(string label, SerializedProperty min, SerializedProperty max)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var minVal = min.floatValue;
                var maxVal = max.floatValue;
                EditorGUILayout.LabelField(label);
                EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, -90, 90);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Min", GUILayout.Width(50));
                    minVal = EditorGUILayout.FloatField(minVal);
                    EditorGUILayout.LabelField("Max", GUILayout.Width(50));
                    maxVal = EditorGUILayout.FloatField(maxVal);
                }
                if (minVal < maxVal && minVal >= -90 && maxVal <= 90)
                {
                    min.floatValue = minVal;
                    max.floatValue = maxVal;
                }
            }
        }
    }
}
