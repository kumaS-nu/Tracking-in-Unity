using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(EyePointToEyeRotationStream))]
    public class EyePointToEyeRotationStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(EyePointToEyeRotationStream.leftCenter)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.leftCenter));
            property[nameof(EyePointToEyeRotationStream.rightCenter)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.rightCenter));
            property[nameof(EyePointToEyeRotationStream.rotateScale)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.rotateScale));
            property[nameof(EyePointToEyeRotationStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.sourceIsMirror));
            property[nameof(EyePointToEyeRotationStream.wantMirror)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.wantMirror));
            property[nameof(EyePointToEyeRotationStream.isDebug)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.isDebug));
            property[nameof(EyePointToEyeRotationStream.isDebugRotation)] = serializedObject.FindProperty(nameof(EyePointToEyeRotationStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.isDebug)], new GUIContent("Debug"));
                if (((EyePointToEyeRotationStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(EyePointToEyeRotationStream.rotateScale)].floatValue = EditorGUILayout.Slider("Rotate scale", property[nameof(EyePointToEyeRotationStream.rotateScale)].floatValue, 0, 90);
                EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.leftCenter)], new GUIContent("Left center"));
                EditorGUILayout.PropertyField(property[nameof(EyePointToEyeRotationStream.rightCenter)], new GUIContent("Right center"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
