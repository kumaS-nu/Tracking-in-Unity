using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Live2D.Editor
{
    [CustomEditor(typeof(PMDToLive2DStream))]
    public class PMDToLive2DStreamEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] key = new string[] { "A", "I", "U", "E", "O" };

        private void OnEnable()
        {
            property[nameof(PMDToLive2DStream.center)] = serializedObject.FindProperty(nameof(PMDToLive2DStream.center));
            property[nameof(PMDToLive2DStream.mouthToDefault)] = serializedObject.FindProperty(nameof(PMDToLive2DStream.mouthToDefault));
            property[nameof(PMDToLive2DStream.mouthCoeff)] = serializedObject.FindProperty(nameof(PMDToLive2DStream.mouthCoeff));
            property[nameof(PMDToLive2DStream.isDebug)] = serializedObject.FindProperty(nameof(PMDToLive2DStream.isDebug));
            property[nameof(PMDToLive2DStream.isDebugParameter)] = serializedObject.FindProperty(nameof(PMDToLive2DStream.isDebugParameter));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("License");
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Live2D Licenses", "by Live2D"), EditorStyles.linkLabel);
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://www.live2d.com/download/cubism-sdk/release-license/");
                    }
                }
            }
            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PMDToLive2DStream.isDebug)], new GUIContent("Debug"));
                if (((PMDToLive2DStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(PMDToLive2DStream.isDebugParameter)], new GUIContent("Parameter"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using(new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PMDToLive2DStream.mouthToDefault)], new GUIContent("To default mouth parameter"));
                if (property[nameof(PMDToLive2DStream.mouthToDefault)].boolValue)
                {
                    for(var i = 0; i < property[nameof(PMDToLive2DStream.mouthCoeff)].arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(property[nameof(PMDToLive2DStream.mouthCoeff)].GetArrayElementAtIndex(i), new GUIContent(key[i]));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}