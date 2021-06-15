using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.PoseNet.Editor
{
    [CustomEditor(typeof(PoseNetToBodyPointStream))]
    public class PoseNetToBodyPointStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private readonly string[] settingType = new string[] { "Focal length", "FoV & width" };

        private void OnEnable()
        {
            property[nameof(PoseNetToBodyPointStream.type)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.type));
            property[nameof(PoseNetToBodyPointStream.fov)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.fov));
            property[nameof(PoseNetToBodyPointStream.width)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.width));
            property[nameof(PoseNetToBodyPointStream.focalLength)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.focalLength));
            property[nameof(PoseNetToBodyPointStream.fold0)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.fold0));
            property[nameof(PoseNetToBodyPointStream.realDistance)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.realDistance));
            property[nameof(PoseNetToBodyPointStream.fold1)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.fold1));
            property[nameof(PoseNetToBodyPointStream.avatarDistance)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.avatarDistance));
            property[nameof(PoseNetToBodyPointStream.zOffset)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.zOffset));
            property[nameof(PoseNetToBodyPointStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.sourceIsMirror));
            property[nameof(PoseNetToBodyPointStream.wantMirror)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.wantMirror));
            property[nameof(PoseNetToBodyPointStream.isDebug)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.isDebug));
            property[nameof(PoseNetToBodyPointStream.isDebugPosition)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.isDebugPosition));
            property[nameof(PoseNetToBodyPointStream.isDebugRotation)] = serializedObject.FindProperty(nameof(PoseNetToBodyPointStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.isDebug)], new GUIContent("Debug"));
                if (((PoseNetToBodyPointStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.isDebugPosition)], new GUIContent("Position"));
                        EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Camera infomation", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(PoseNetToBodyPointStream.type)].intValue = EditorGUILayout.Popup(new GUIContent("Setting type"), property[nameof(PoseNetToBodyPointStream.type)].intValue, settingType);
                switch (property[nameof(PoseNetToBodyPointStream.type)].intValue)
                {
                    case 0:
                        EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.focalLength)], new GUIContent("Focal length (px)"));
                        property[nameof(PoseNetToBodyPointStream.fov)].floatValue = 2 * Mathf.Atan2(property[nameof(PoseNetToBodyPointStream.width)].intValue, property[nameof(PoseNetToBodyPointStream.focalLength)].floatValue * 2) * Mathf.Rad2Deg;
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.fov)], new GUIContent("Field of view (deg)"));
                        EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.width)], new GUIContent("Width (px)"));
                        property[nameof(PoseNetToBodyPointStream.focalLength)].floatValue = property[nameof(PoseNetToBodyPointStream.width)].intValue / 2 / Mathf.Tan(property[nameof(PoseNetToBodyPointStream.fov)].floatValue / 2 * Mathf.Deg2Rad);
                        break;
                }

            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.zOffset)], new GUIContent("Z offset"));
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Distance infomation", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(PoseNetToBodyPointStream.fold0)].boolValue = EditorGUILayout.Foldout(property[nameof(PoseNetToBodyPointStream.fold0)].boolValue, "Real");
                if (property[nameof(PoseNetToBodyPointStream.fold0)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < PoseNetToBodyPointStream.distanceName.Length; i++)
                        {
                            EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.realDistance)].GetArrayElementAtIndex(i), new GUIContent(PoseNetToBodyPointStream.distanceName[i]));
                        }
                    }
                }
                EditorGUILayout.Space();

                property[nameof(PoseNetToBodyPointStream.fold1)].boolValue = EditorGUILayout.Foldout(property[nameof(PoseNetToBodyPointStream.fold1)].boolValue, "Avatar");
                if (property[nameof(PoseNetToBodyPointStream.fold1)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < PoseNetToBodyPointStream.distanceName.Length; i++)
                        {
                            EditorGUILayout.PropertyField(property[nameof(PoseNetToBodyPointStream.avatarDistance)].GetArrayElementAtIndex(i), new GUIContent(PoseNetToBodyPointStream.distanceName[i]));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
