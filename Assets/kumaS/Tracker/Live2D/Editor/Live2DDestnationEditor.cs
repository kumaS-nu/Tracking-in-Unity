using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Live2D.Editor
{
    [CustomEditor(typeof(Live2DDestnation))]
    public class Live2DDestnationEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(Live2DDestnation.model)] = serializedObject.FindProperty(nameof(Live2DDestnation.model));
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
