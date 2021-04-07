using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(MatSource))]
    public class MatSourceEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(MatSource.useUnity)] = serializedObject.FindProperty(nameof(MatSource.useUnity));
            property[nameof(MatSource.isFile)] = serializedObject.FindProperty(nameof(MatSource.isFile));
            property[nameof(MatSource.cameraIndex)] = serializedObject.FindProperty(nameof(MatSource.cameraIndex));
            property[nameof(MatSource.filePath)] = serializedObject.FindProperty(nameof(MatSource.filePath));
            property[nameof(MatSource.pathType)] = serializedObject.FindProperty(nameof(MatSource.pathType));
            property[nameof(MatSource.isDebug)] = serializedObject.FindProperty(nameof(MatSource.isDebug));
            property[nameof(MatSource.isDebugSize)] = serializedObject.FindProperty(nameof(MatSource.isDebugSize));
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

                    EditorGUILayout.LabelField(new GUIContent("LGPL 2.1 or later", "by FFmpeg"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://ffmpeg.org/legal.html");
                    }
                }
            }

            ((MatSource)target).ProcessName = EditorGUILayout.TextField("Process name", ((MatSource)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(MatSource.isDebug)], new GUIContent("Debug"));
                if (((MatSource)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(MatSource.isDebugSize)], new GUIContent("Image size"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Video setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                var filePath = property[nameof(MatSource.filePath)].stringValue;
                var pathType = property[nameof(MatSource.pathType)].intValue;
                filePath = PathUtil.Deserialize(filePath, pathType);

                EditorGUILayout.PropertyField(property[nameof(MatSource.useUnity)], new GUIContent("Use unity system"));
                EditorGUILayout.PropertyField(property[nameof(MatSource.isFile)], new GUIContent("Is file"));
                if (property[nameof(MatSource.isFile)].boolValue)
                {
                    var label = PathUtil.PathHeadLabel.Skip(property[nameof(MatSource.useUnity)].boolValue ? 0 : 1).ToArray();
                    pathType = EditorGUILayout.IntPopup("Path type", pathType, label, label.Select(value => PathUtil.PathHeadLabel.ToList().IndexOf(value)).ToArray());
                    filePath = EditorGUILayout.TextField("File path", filePath);
                    filePath = PathUtil.Serialize(filePath, pathType, property[nameof(MatSource.useUnity)].boolValue);

                    if (filePath == "" || (pathType != 0 && !File.Exists(filePath.Replace("file:", ""))))
                    {
                        EditorGUILayout.HelpBox("動画ファイルが見つかりませんでした。", MessageType.Error);
                    }
                    property[nameof(MatSource.filePath)].stringValue = filePath;
                    property[nameof(MatSource.pathType)].intValue = pathType;
                }
                else
                {
                    EditorGUILayout.PropertyField(property[nameof(MatSource.cameraIndex)], new GUIContent("Camera index"));
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
