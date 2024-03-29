﻿using kumaS.Tracker.Core;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib.Editor
{
    [CustomEditor(typeof(Dlib5Stream))]
    public class Dlib5StreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] pathTypeLabel;
        private int[] pathTypeIndex;

        private void OnEnable()
        {
            property[nameof(Dlib5Stream.filePath)] = serializedObject.FindProperty(nameof(Dlib5Stream.filePath));
            property[nameof(Dlib5Stream.pathType)] = serializedObject.FindProperty(nameof(Dlib5Stream.pathType));
            property[nameof(Dlib5Stream.interval)] = serializedObject.FindProperty(nameof(Dlib5Stream.interval));
            property[nameof(Dlib5Stream.debugInterval)] = serializedObject.FindProperty(nameof(Dlib5Stream.debugInterval));
            property[nameof(Dlib5Stream.debugImage)] = serializedObject.FindProperty(nameof(Dlib5Stream.debugImage));
            property[nameof(Dlib5Stream.markColor)] = serializedObject.FindProperty(nameof(Dlib5Stream.markColor));
            property[nameof(Dlib5Stream.markSize)] = serializedObject.FindProperty(nameof(Dlib5Stream.markSize));
            property[nameof(Dlib5Stream.fontScale)] = serializedObject.FindProperty(nameof(Dlib5Stream.fontScale));
            property[nameof(Dlib5Stream.isDebug)] = serializedObject.FindProperty(nameof(Dlib5Stream.isDebug));
            property[nameof(Dlib5Stream.isDebugPoint)] = serializedObject.FindProperty(nameof(Dlib5Stream.isDebugPoint));
            property[nameof(Dlib5Stream.isDebugImage)] = serializedObject.FindProperty(nameof(Dlib5Stream.isDebugImage));
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
                }
            }
            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.isDebug)], new GUIContent("Debug"));
                if (((Dlib5Stream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.isDebugPoint)], new GUIContent("Point"));
                        EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.isDebugImage)], new GUIContent("by Image"));
                        if (property[nameof(Dlib5Stream.isDebugImage)].boolValue)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.debugImage)], new GUIContent("Image debugger"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.markColor)], new GUIContent("Mark color"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.markSize)], new GUIContent("Mark size"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.debugInterval)], new GUIContent("Interval"));
                                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.isDebugIndex)], new GUIContent("with Index"));
                                if (property[nameof(Dlib5Stream.isDebugIndex)].boolValue)
                                {
                                    EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.fontScale)], new GUIContent("Font scale"));
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
                var filePath = property[nameof(Dlib5Stream.filePath)].stringValue;
                var pathType = property[nameof(Dlib5Stream.pathType)].intValue;
                filePath = PathUtil.Deserialize(filePath, pathType);
                pathType = EditorGUILayout.IntPopup("Path type", pathType, pathTypeLabel, pathTypeIndex);
                filePath = EditorGUILayout.TextField("File path", filePath);
                filePath = PathUtil.Serialize(filePath, pathType, false);

                if (filePath == "" || !File.Exists(filePath))
                {
                    EditorGUILayout.HelpBox("モデルファイルが見つかりませんでした。", MessageType.Error);
                }
                property[nameof(Dlib5Stream.filePath)].stringValue = filePath;
                property[nameof(Dlib5Stream.pathType)].intValue = pathType;
                EditorGUILayout.PropertyField(property[nameof(Dlib5Stream.interval)], new GUIContent("Interval"));
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
