using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib68ToHeadTransformStream))]
    public class Dlib68ToHeadTransformStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private readonly string[] settingType = new string[] { "Focal length", "FoV & width" };
        private readonly GUIContent[] pointName = new GUIContent[] {
            new GUIContent("Nose"), new GUIContent("Jaw"), new GUIContent("Outer left eye"), new GUIContent("Outer right eye"),
            new GUIContent("Right of mouth"), new GUIContent("Left of mouth"), new GUIContent("Inner left eye"), new GUIContent("Inner right eye")
        };


        private void OnEnable()
        {
            property[nameof(Dlib68ToHeadTransformStream.type)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.type));
            property[nameof(Dlib68ToHeadTransformStream.fov)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.fov));
            property[nameof(Dlib68ToHeadTransformStream.width)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.width));
            property[nameof(Dlib68ToHeadTransformStream.focalLength)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.focalLength));
            property[nameof(Dlib68ToHeadTransformStream.fold)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.fold));
            property[nameof(Dlib68ToHeadTransformStream.realPoint)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.realPoint));
            property[nameof(Dlib68ToHeadTransformStream.moveScale)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.moveScale));
            property[nameof(Dlib68ToHeadTransformStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.sourceIsMirror));
            property[nameof(Dlib68ToHeadTransformStream.wantMirror)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.wantMirror));
            property[nameof(Dlib68ToHeadTransformStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.isDebug));
            property[nameof(Dlib68ToHeadTransformStream.isDebugHead)] = serializedObject.FindProperty(nameof(Dlib68ToHeadTransformStream.isDebugHead));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib68ToHeadTransformStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Camera infomation", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.sourceIsMirror)], new GUIContent("Is source mirror"));
                EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(Dlib68ToHeadTransformStream.type)].intValue = EditorGUILayout.Popup(new GUIContent("Setting type"), property[nameof(Dlib68ToHeadTransformStream.type)].intValue, settingType);
                switch (property[nameof(Dlib68ToHeadTransformStream.type)].intValue)
                {
                    case 0:
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.focalLength)], new GUIContent("Focal length (px)"));
                        property[nameof(Dlib68ToHeadTransformStream.fov)].floatValue = 2 * Mathf.Atan2(property[nameof(Dlib68ToHeadTransformStream.width)].intValue, property[nameof(Dlib68ToHeadTransformStream.focalLength)].floatValue * 2);
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.fov)], new GUIContent("Field of view (deg)"));
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.width)], new GUIContent("Width (px)"));
                        property[nameof(Dlib68ToHeadTransformStream.focalLength)].floatValue = property[nameof(Dlib68ToHeadTransformStream.width)].intValue / 2 / Mathf.Tan(property[nameof(Dlib68ToHeadTransformStream.fov)].floatValue / 2);
                        break;
                }

            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(Dlib68ToHeadTransformStream.moveScale)].floatValue = EditorGUILayout.Slider("Move scale", property[nameof(Dlib68ToHeadTransformStream.moveScale)].floatValue, 0, 1);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("3D infomation");
            using(new EditorGUI.IndentLevelScope())
            {
                property[nameof(Dlib68ToHeadTransformStream.fold)].boolValue = EditorGUILayout.Foldout(property[nameof(Dlib68ToHeadTransformStream.fold)].boolValue, "Landmark positions");
                if (!property[nameof(Dlib68ToHeadTransformStream.fold)].boolValue)
                {
                    for(var i = 0; i < property[nameof(Dlib68ToHeadTransformStream.realPoint)].arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68ToHeadTransformStream.realPoint)].GetArrayElementAtIndex(i), pointName[i]);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
