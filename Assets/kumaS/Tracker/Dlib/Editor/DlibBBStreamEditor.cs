﻿using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.Dlib
{
    [CustomEditor(typeof(DlibBBStream))]
    public class DlibBBStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(DlibBBStream.interval)] = serializedObject.FindProperty(nameof(DlibBBStream.interval));
            property[nameof(DlibBBStream.debugInterval)] = serializedObject.FindProperty(nameof(DlibBBStream.debugInterval));
            property[nameof(DlibBBStream.debugImage)] = serializedObject.FindProperty(nameof(DlibBBStream.debugImage));
            property[nameof(DlibBBStream.markColor)] = serializedObject.FindProperty(nameof(DlibBBStream.markColor));
            property[nameof(DlibBBStream.markSize)] = serializedObject.FindProperty(nameof(DlibBBStream.markSize));
            property[nameof(DlibBBStream.isDebug)] = serializedObject.FindProperty(nameof(DlibBBStream.isDebug));
            property[nameof(DlibBBStream.isDebugBox)] = serializedObject.FindProperty(nameof(DlibBBStream.isDebugBox));
            property[nameof(DlibBBStream.isDebugImage)] = serializedObject.FindProperty(nameof(DlibBBStream.isDebugImage));
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
                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.isDebug)], new GUIContent("Debug"));
                if (((DlibBBStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(DlibBBStream.isDebugBox)], new GUIContent("Boundy box"));
                        EditorGUILayout.PropertyField(property[nameof(DlibBBStream.isDebugImage)], new GUIContent("by Image"));
                        if (property[nameof(DlibBBStream.isDebugImage)].boolValue)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.debugImage)], new GUIContent("Image debugger"));
                                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.markColor)], new GUIContent("Line color"));
                                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.markSize)], new GUIContent("Line thickness"));
                                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.debugInterval)], new GUIContent("Interval"));
                            }
                        }
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Predict setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(DlibBBStream.interval)], new GUIContent("Interval"));
                if(property[nameof(DlibBBStream.interval)].intValue < 0)
                {
                    property[nameof(DlibBBStream.interval)].intValue = 0;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
