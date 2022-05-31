using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib68ToEyePointStream))]
    public class Dlib68ToEyePointStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Dlib68ToEyePointStream.interval)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.interval));
            property[nameof(Dlib68ToEyePointStream.debugImage)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.debugImage));
            property[nameof(Dlib68ToEyePointStream.markColor)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.markColor));
            property[nameof(Dlib68ToEyePointStream.markSize)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.markSize));
            property[nameof(Dlib68ToEyePointStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.isDebug));
            property[nameof(Dlib68ToEyePointStream.isDebugPoint)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.isDebugPoint));
            property[nameof(Dlib68ToEyePointStream.isDebugImage)] = serializedObject.FindProperty(nameof(Dlib68ToEyePointStream.isDebugImage));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib68ToEyePointStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.isDebugPoint)], new GUIContent("Point"));
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.isDebugImage)], new GUIContent("by Image"));
                        if (property[nameof(Dlib68ToEyePointStream.isDebugImage)].boolValue)
                        {
                            using(new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.debugImage)], new GUIContent("Image debugger"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.markColor)], new GUIContent("Mark color"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.markSize)], new GUIContent("Mark size"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyePointStream.interval)], new GUIContent("Interval"));
                            }
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
