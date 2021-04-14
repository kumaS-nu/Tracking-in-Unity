using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(RatioToCloseValueStream))]
    public class RatioToCloseValueStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(RatioToCloseValueStream.ratioMax)] = serializedObject.FindProperty(nameof(RatioToCloseValueStream.ratioMax));
            property[nameof(RatioToCloseValueStream.ratioMin)] = serializedObject.FindProperty(nameof(RatioToCloseValueStream.ratioMin));
            property[nameof(RatioToCloseValueStream.isDebug)] = serializedObject.FindProperty(nameof(RatioToCloseValueStream.isDebug));
            property[nameof(RatioToCloseValueStream.isDebugValue)] = serializedObject.FindProperty(nameof(RatioToCloseValueStream.isDebugValue));
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
                EditorGUILayout.PropertyField(property[nameof(RatioToCloseValueStream.isDebug)], new GUIContent("Debug"));
                if (((RatioToCloseValueStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(RatioToCloseValueStream.isDebugValue)], new GUIContent("Value"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                var min = property[nameof(RatioToCloseValueStream.ratioMin)].floatValue;
                var max = property[nameof(RatioToCloseValueStream.ratioMax)].floatValue;
                EditorGUILayout.MinMaxSlider("Eye ratio", ref min, ref max, 0, 1);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Min", GUILayout.Width(50));
                    min = EditorGUILayout.FloatField(min);
                    EditorGUILayout.LabelField("Max", GUILayout.Width(50));
                    max = EditorGUILayout.FloatField(max);
                }
                if(min < max && min >= 0 && max <= 1)
                {
                    property[nameof(RatioToCloseValueStream.ratioMin)].floatValue = min;
                    property[nameof(RatioToCloseValueStream.ratioMax)].floatValue = max;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
