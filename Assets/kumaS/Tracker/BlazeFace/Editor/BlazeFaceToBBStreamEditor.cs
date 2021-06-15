using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.BlazeFace.Editor
{
    [CustomEditor(typeof(BlazeFaceToBBStream))]
    public class BlazeFaceToBBStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private void OnEnable()
        {
            property[nameof(BlazeFaceToBBStream.isDebug)] = serializedObject.FindProperty(nameof(BlazeFaceToBBStream.isDebug));
            property[nameof(BlazeFaceToBBStream.isDebugBox)] = serializedObject.FindProperty(nameof(BlazeFaceToBBStream.isDebugBox));
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
                EditorGUILayout.PropertyField(property[nameof(BlazeFaceToBBStream.isDebug)], new GUIContent("Debug"));
                if (((BlazeFaceToBBStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(BlazeFaceToBBStream.isDebugBox)], new GUIContent("Boundy box"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
