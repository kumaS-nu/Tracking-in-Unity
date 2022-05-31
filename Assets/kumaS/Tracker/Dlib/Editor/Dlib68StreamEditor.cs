using kumaS.Tracker.Core;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib68Stream))]
    public class Dlib68StreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] pathTypeLabel;
        private int[] pathTypeIndex;

        private void OnEnable()
        {
            property[nameof(Dlib68Stream.filePath)] = serializedObject.FindProperty(nameof(Dlib68Stream.filePath));
            property[nameof(Dlib68Stream.pathType)] = serializedObject.FindProperty(nameof(Dlib68Stream.pathType));
            property[nameof(Dlib68Stream.debugImage)] = serializedObject.FindProperty(nameof(Dlib68Stream.debugImage));
            property[nameof(Dlib68Stream.interval)] = serializedObject.FindProperty(nameof(Dlib68Stream.interval));
            property[nameof(Dlib68Stream.markColor)] = serializedObject.FindProperty(nameof(Dlib68Stream.markColor));
            property[nameof(Dlib68Stream.markSize)] = serializedObject.FindProperty(nameof(Dlib68Stream.markSize));
            property[nameof(Dlib68Stream.fontScale)] = serializedObject.FindProperty(nameof(Dlib68Stream.fontScale));
            property[nameof(Dlib68Stream.isDebug)] = serializedObject.FindProperty(nameof(Dlib68Stream.isDebug));
            property[nameof(Dlib68Stream.isDebugPoint)] = serializedObject.FindProperty(nameof(Dlib68Stream.isDebugPoint));
            property[nameof(Dlib68Stream.isDebugImage)] = serializedObject.FindProperty(nameof(Dlib68Stream.isDebugImage));
            property[nameof(Dlib68Stream.isDebugIndex)] = serializedObject.FindProperty(nameof(Dlib68Stream.isDebugIndex));
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
                    EditorGUILayout.LabelField(new GUIContent("Boost Software License", "by Dlib"), EditorStyles.linkLabel);
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("http://dlib.net/license.html");
                    }

                    EditorGUILayout.LabelField(new GUIContent("MIT License", "by DlibDotNet"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/takuya-takeuchi/DlibDotNet/blob/master/LICENSE.txt");
                    }

                    EditorGUILayout.LabelField(new GUIContent("giflib License", "by giflib"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("http://giflib.sourceforge.net/");
                    }

                    EditorGUILayout.LabelField(new GUIContent("Independent JPEG Group's License", "by libjpeg"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("http://www.ijg.org/");
                    }

                    EditorGUILayout.LabelField(new GUIContent("libpng License", "by libpng"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("http://libpng.org/pub/png/libpng.html");
                    }

                    EditorGUILayout.LabelField(new GUIContent("zlib License", "by zlib"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://zlib.net/");
                    }

                    EditorGUILayout.LabelField(new GUIContent("ibug 300-W", "by model"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://ibug.doc.ic.ac.uk/resources/300-W/");
                    }
                }
            }
            EditorGUILayout.HelpBox("研究目的のみ使用が許可されています。", MessageType.Warning);

            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.isDebug)], new GUIContent("Debug"));
                if (((Dlib68Stream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.isDebugPoint)], new GUIContent("Point"));
                        EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.isDebugImage)], new GUIContent("by Image"));
                        if (property[nameof(Dlib68Stream.isDebugImage)].boolValue)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.debugImage)], new GUIContent("Image debugger"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.markColor)], new GUIContent("Mark color"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.markSize)], new GUIContent("Mark size"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.interval)], new GUIContent("Interval"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.isDebugIndex)], new GUIContent("with Index"));
                                if (property[nameof(Dlib68Stream.isDebugIndex)].boolValue)
                                {
                                    EditorGUILayout.PropertyField(property[nameof(Dlib68Stream.fontScale)], new GUIContent("Font scale"));
                                }

                            }
                        }
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Model setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                var filePath = property[nameof(Dlib68Stream.filePath)].stringValue;
                var pathType = property[nameof(Dlib68Stream.pathType)].intValue;
                filePath = PathUtil.Deserialize(filePath, pathType);
                pathType = EditorGUILayout.IntPopup("Path type", pathType, pathTypeLabel, pathTypeIndex);
                filePath = EditorGUILayout.TextField("File path", filePath);
                filePath = PathUtil.Serialize(filePath, pathType, false);

                if (filePath == "" || !File.Exists(filePath))
                {
                    EditorGUILayout.HelpBox("モデルファイルが見つかりませんでした。", MessageType.Error);
                }
                property[nameof(Dlib68Stream.filePath)].stringValue = filePath;
                property[nameof(Dlib68Stream.pathType)].intValue = pathType;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
