using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh.Editor
{
    [CustomEditor(typeof(FaceMeshToHeadTransformStream))]
    public class FaceMeshToHeadTransformStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private readonly string[] settingType = new string[] { "Focal length", "FoV & width" };
        private readonly GUIContent[] pointName = new GUIContent[] {
            new GUIContent("Nose"), new GUIContent("Jaw"), new GUIContent("Outer left eye"), new GUIContent("Outer right eye"),
            new GUIContent("Left of mouth"), new GUIContent("Right of mouth"), new GUIContent("Inner left eye"), new GUIContent("Inner right eye")
        };


        private void OnEnable()
        {
            property[nameof(FaceMeshToHeadTransformStream.type)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.type));
            property[nameof(FaceMeshToHeadTransformStream.fov)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.fov));
            property[nameof(FaceMeshToHeadTransformStream.width)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.width));
            property[nameof(FaceMeshToHeadTransformStream.focalLength)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.focalLength));
            property[nameof(FaceMeshToHeadTransformStream.fold)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.fold));
            property[nameof(FaceMeshToHeadTransformStream.realPoint)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.realPoint));
            property[nameof(FaceMeshToHeadTransformStream.moveScale)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.moveScale));
            property[nameof(FaceMeshToHeadTransformStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.sourceIsMirror));
            property[nameof(FaceMeshToHeadTransformStream.wantMirror)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.wantMirror));
            property[nameof(FaceMeshToHeadTransformStream.isDebug)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.isDebug));
            property[nameof(FaceMeshToHeadTransformStream.isDebugHead)] = serializedObject.FindProperty(nameof(FaceMeshToHeadTransformStream.isDebugHead));
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
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.isDebug)], new GUIContent("Debug"));
                if (((FaceMeshToHeadTransformStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Camera infomation", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.sourceIsMirror)], new GUIContent("Source is mirror"));
                EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(FaceMeshToHeadTransformStream.type)].intValue = EditorGUILayout.Popup(new GUIContent("Setting type"), property[nameof(FaceMeshToHeadTransformStream.type)].intValue, settingType);
                switch (property[nameof(FaceMeshToHeadTransformStream.type)].intValue)
                {
                    case 0:
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.focalLength)], new GUIContent("Focal length (px)"));
                        property[nameof(FaceMeshToHeadTransformStream.fov)].floatValue = 2 * Mathf.Atan2(property[nameof(FaceMeshToHeadTransformStream.width)].intValue, property[nameof(FaceMeshToHeadTransformStream.focalLength)].floatValue * 2);
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.fov)], new GUIContent("Field of view (deg)"));
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.width)], new GUIContent("Width (px)"));
                        property[nameof(FaceMeshToHeadTransformStream.focalLength)].floatValue = property[nameof(FaceMeshToHeadTransformStream.width)].intValue / 2 / Mathf.Tan(property[nameof(FaceMeshToHeadTransformStream.fov)].floatValue / 2);
                        break;
                }

            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(FaceMeshToHeadTransformStream.moveScale)].floatValue = EditorGUILayout.Slider("Move scale", property[nameof(FaceMeshToHeadTransformStream.moveScale)].floatValue, 0, 1);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("3D infomation");
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(FaceMeshToHeadTransformStream.fold)].boolValue = EditorGUILayout.Foldout(property[nameof(FaceMeshToHeadTransformStream.fold)].boolValue, "Landmark positions");
                if (property[nameof(FaceMeshToHeadTransformStream.fold)].boolValue)
                {
                    for (var i = 0; i < property[nameof(FaceMeshToHeadTransformStream.realPoint)].arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshToHeadTransformStream.realPoint)].GetArrayElementAtIndex(i), pointName[i]);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
