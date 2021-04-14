using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(BBToHeadTransformStream))]
    public class BBToHeadTransformStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(BBToHeadTransformStream.moveScale)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.moveScale));
            property[nameof(BBToHeadTransformStream.isEnableDepth)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.isEnableDepth));
            property[nameof(BBToHeadTransformStream.depthCenter)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.depthCenter));
            property[nameof(BBToHeadTransformStream.sourceIsMirror)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.sourceIsMirror));
            property[nameof(BBToHeadTransformStream.wantMirror)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.wantMirror));
            property[nameof(BBToHeadTransformStream.isDebug)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.isDebug));
            property[nameof(BBToHeadTransformStream.isDebugHead)] = serializedObject.FindProperty(nameof(BBToHeadTransformStream.isDebugHead));
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
                EditorGUILayout.PropertyField(property[nameof(BBToHeadTransformStream.isDebug)], new GUIContent("Debug"));
                if (((BBToHeadTransformStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(BBToHeadTransformStream.isDebugHead)], new GUIContent("Head"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(BBToHeadTransformStream.sourceIsMirror)], new GUIContent("Is source mirror"));
                EditorGUILayout.PropertyField(property[nameof(BBToHeadTransformStream.wantMirror)], new GUIContent("Is mirror"));
                property[nameof(BBToHeadTransformStream.moveScale)].floatValue = EditorGUILayout.Slider("Move scale", property[nameof(BBToHeadTransformStream.moveScale)].floatValue, 0, 1);
                EditorGUILayout.PropertyField(property[nameof(BBToHeadTransformStream.isEnableDepth)], new GUIContent("Enable depth"));
                if (property[nameof(BBToHeadTransformStream.isEnableDepth)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        property[nameof(BBToHeadTransformStream.depthCenter)].floatValue = EditorGUILayout.Slider("Depth center", property[nameof(BBToHeadTransformStream.depthCenter)].floatValue, 0, 1);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
