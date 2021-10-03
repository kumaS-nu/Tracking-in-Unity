using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib68ToEyeRotationStream))]
    public class Dlib68ToEyeRotationStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Dlib68ToEyeRotationStream.leftCenter)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.leftCenter));
            property[nameof(Dlib68ToEyeRotationStream.rightCenter)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.rightCenter));
            property[nameof(Dlib68ToEyeRotationStream.rotateScale)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.rotateScale));
            property[nameof(Dlib68ToEyeRotationStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.sourceIsMirror));
            property[nameof(Dlib68ToEyeRotationStream.wantMirror)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.wantMirror));
            property[nameof(Dlib68ToEyeRotationStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.isDebug));
            property[nameof(Dlib68ToEyeRotationStream.isDebugRotation)] = serializedObject.FindProperty(nameof(Dlib68ToEyeRotationStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib68ToEyeRotationStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(Dlib68ToEyeRotationStream.rotateScale)].floatValue = EditorGUILayout.Slider("Rotate scale", property[nameof(Dlib68ToEyeRotationStream.rotateScale)].floatValue, 0, 90);
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.leftCenter)], new GUIContent("Left center"));
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToEyeRotationStream.rightCenter)], new GUIContent("Right center"));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
