using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(OpenCvBBStream))]
    public class OpenCvBBStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] pathTypeLabel;
        private int[] pathTypeIndex;

        private void OnEnable()
        {
            property[nameof(OpenCvBBStream.isDebug)] = serializedObject.FindProperty(nameof(OpenCvBBStream.isDebug));
            property[nameof(OpenCvBBStream.isDebugBox)] = serializedObject.FindProperty(nameof(OpenCvBBStream.isDebugBox));
            property[nameof(OpenCvBBStream.filePath)] = serializedObject.FindProperty(nameof(OpenCvBBStream.filePath));
            property[nameof(OpenCvBBStream.pathType)] = serializedObject.FindProperty(nameof(OpenCvBBStream.pathType));
            property[nameof(OpenCvBBStream.interval)] = serializedObject.FindProperty(nameof(OpenCvBBStream.interval));
            pathTypeLabel = PathUtil.PathHeadLabel.Skip(1).ToArray();
            pathTypeIndex = pathTypeLabel.Select(value => PathUtil.PathHeadLabel.ToList().IndexOf(value)).ToArray();
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
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/shimat/opencvsharp/blob/master/LICENSE");
                        Help.BrowseURL("https://opencv.org/license/");
                    }

                    EditorGUILayout.LabelField(new GUIContent("3-clause BSD license", "by model"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/opencv/opencv/blob/master/data/haarcascades/haarcascade_frontalface_default.xml");
                    }
                }
            }

            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(OpenCvBBStream.isDebug)], new GUIContent("Debug"));
                if (((OpenCvBBStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(OpenCvBBStream.isDebugBox)], new GUIContent("Boundy box"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Model setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                var filePath = property[nameof(OpenCvBBStream.filePath)].stringValue;
                var pathType = property[nameof(OpenCvBBStream.pathType)].intValue;
                filePath = PathUtil.Deserialize(filePath, pathType);
                pathType = EditorGUILayout.IntPopup("Path type", pathType, pathTypeLabel, pathTypeIndex);
                filePath = EditorGUILayout.TextField("File path", filePath);
                filePath = PathUtil.Serialize(filePath, pathType, false);

                if (filePath == "" || !File.Exists(filePath))
                {
                    EditorGUILayout.HelpBox("モデルファイルが見つかりませんでした。", MessageType.Error);
                }
                property[nameof(OpenCvBBStream.filePath)].stringValue = filePath;
                property[nameof(OpenCvBBStream.pathType)].intValue = pathType;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Predict setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(OpenCvBBStream.interval)].intValue = EditorGUILayout.IntSlider("Interval", property[nameof(OpenCvBBStream.interval)].intValue, 0, 30);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
