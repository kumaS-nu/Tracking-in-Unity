using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib5ToHeadTransformStream))]
    public class Dlib5ToHeadTransformStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Dlib5ToHeadTransformStream.depthCenter)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.depthCenter));
            property[nameof(Dlib5ToHeadTransformStream.moveScale)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.moveScale));
            property[nameof(Dlib5ToHeadTransformStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.sourceIsMirror));
            property[nameof(Dlib5ToHeadTransformStream.wantMirror)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.wantMirror));
            property[nameof(Dlib5ToHeadTransformStream.isDebug)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.isDebug));
            property[nameof(Dlib5ToHeadTransformStream.isDebugHead)] = serializedObject.FindProperty(nameof(Dlib5ToHeadTransformStream.isDebugHead));
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
                EditorGUILayout.PropertyField(property[nameof(Dlib5ToHeadTransformStream.isDebug)], new GUIContent("Debug"));
                if (((Dlib5ToHeadTransformStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib5ToHeadTransformStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib5ToHeadTransformStream.sourceIsMirror)], new GUIContent("Is source mirror"));
                EditorGUILayout.PropertyField(property[nameof(Dlib5ToHeadTransformStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(Dlib5ToHeadTransformStream.moveScale)].floatValue = EditorGUILayout.Slider("Move scale", property[nameof(Dlib5ToHeadTransformStream.moveScale)].floatValue, 0, 1);
                property[nameof(Dlib5ToHeadTransformStream.depthCenter)].floatValue = EditorGUILayout.Slider("Depth center", property[nameof(Dlib5ToHeadTransformStream.depthCenter)].floatValue, 0, 1);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
