using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh.Editor
{
    [CustomEditor(typeof(FaceMeshToEyeRatioStream))]
    public class FaceMeshToEyeRatioStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(FaceMeshToEyeRatioStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRatioStream.sourceIsMirror));
            property[nameof(FaceMeshToEyeRatioStream.wantMirror)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRatioStream.wantMirror));
            property[nameof(FaceMeshToEyeRatioStream.isDebug)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRatioStream.isDebug));
            property[nameof(FaceMeshToEyeRatioStream.isDebugRatio)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRatioStream.isDebugRatio));
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
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRatioStream.isDebug)], new GUIContent("Debug"));
                if (((FaceMeshToEyeRatioStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRatioStream.isDebugRatio)], new GUIContent("Ratio"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRatioStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRatioStream.wantMirror)], new GUIContent("Is mirror"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
