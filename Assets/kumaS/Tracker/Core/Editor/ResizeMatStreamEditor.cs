using OpenCvSharp;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(ResizeMatStream))]
    public class ResizeMatStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] interpolationLabel;

        private void OnEnable()
        {
            property[nameof(ResizeMatStream.isDebug)] = serializedObject.FindProperty(nameof(ResizeMatStream.isDebug));
            property[nameof(ResizeMatStream.ratio)] = serializedObject.FindProperty(nameof(ResizeMatStream.ratio));
            property[nameof(ResizeMatStream.interpolation)] = serializedObject.FindProperty(nameof(ResizeMatStream.interpolation));
            if (interpolationLabel == null)
            {
                interpolationLabel = new string[6];
                for (var i = 0; i < 6; i++)
                {
                    interpolationLabel[i] = ((InterpolationFlags)i).ToString();
                }
            }
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
                    EditorGUILayout.LabelField(new GUIContent("Apache License 2.0", "by OpenCV, OpenCvSharp"), EditorStyles.linkLabel);
                    UnityEngine.Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/shimat/opencvsharp/blob/master/LICENSE");
                        Help.BrowseURL("https://opencv.org/license/");
                    }
                }
            }

            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(MatSource.isDebug)], new GUIContent("Debug"));
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Resize setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(ResizeMatStream.ratio)].doubleValue = EditorGUILayout.Slider("Resolution ratio", (float)property[nameof(ResizeMatStream.ratio)].doubleValue, 0, 1);
                property[nameof(ResizeMatStream.interpolation)].enumValueIndex = EditorGUILayout.Popup("Interpolation", property[nameof(ResizeMatStream.interpolation)].enumValueIndex, interpolationLabel);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
