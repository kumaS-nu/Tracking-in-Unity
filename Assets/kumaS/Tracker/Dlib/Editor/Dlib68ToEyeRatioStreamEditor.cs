using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib68ToEyeRatioStream))]
    public class Dlib68ToEyeRatioStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Dlib68ToEyeRatioStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRatioStream.sourceIsMirror));
            property[nameof(Dlib68ToEyeRatioStream.wantMirror)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRatioStream.wantMirror));
            property[nameof(Dlib68ToEyeRatioStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRatioStream.isDebug));
            property[nameof(Dlib68ToEyeRatioStream.isDebugRatio)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRatioStream.isDebugRatio));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRatioStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib68ToEyeRatioStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRatioStream.isDebugRatio)], new GUIContent("Ratio"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRatioStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRatioStream.wantMirror)], new GUIContent("Is mirror"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
