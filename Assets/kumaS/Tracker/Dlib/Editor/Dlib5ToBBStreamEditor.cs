using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib5ToBBStream))]
    public class Dlib5ToBBStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Dlib5ToBBStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib5ToBBStream.isDebug));
            property[nameof(Dlib5ToBBStream.isDebugBox)] = serializedObject.FindProperty(nameof(Dlib5ToBBStream.isDebugBox));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib5ToBBStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib5ToBBStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib5ToBBStream.isDebugBox)], new GUIContent("Boundy box"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
