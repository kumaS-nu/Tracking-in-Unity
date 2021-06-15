using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh.Editor
{
    [CustomEditor(typeof(FaceMeshToEyeRotationStream))]
    public class FaceMeshToEyeRotationStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(FaceMeshToEyeRotationStream.leftCenter)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.leftCenter));
            property[nameof(FaceMeshToEyeRotationStream.rightCenter)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.rightCenter));
            property[nameof(FaceMeshToEyeRotationStream.rotateScale)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.rotateScale));
            property[nameof(FaceMeshToEyeRotationStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.sourceIsMirror));
            property[nameof(FaceMeshToEyeRotationStream.wantMirror)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.wantMirror));
            property[nameof(FaceMeshToEyeRotationStream.isDebug)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.isDebug));
            property[nameof(FaceMeshToEyeRotationStream.isDebugRotation)] = serializedObject.FindProperty(nameof(FaceMeshToEyeRotationStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.isDebug)], new GUIContent("Debug"));
                if (((FaceMeshToEyeRotationStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(FaceMeshToEyeRotationStream.rotateScale)].floatValue = EditorGUILayout.Slider("Rotate scale", property[nameof(FaceMeshToEyeRotationStream.rotateScale)].floatValue, 0, 1);
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.leftCenter)], new GUIContent("Left center"));
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToEyeRotationStream.rightCenter)], new GUIContent("Right center"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
