using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.VRM;

namespace kumaS.Tracker.VRM.Editor
{
    [CustomEditor(typeof(VRMDestination))]
    public class VRMDestinationEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(VRMDestination.proxy)] = serializedObject.FindProperty(nameof(VRMDestination.proxy));
            property[nameof(VRMDestination.transforms)] = serializedObject.FindProperty(nameof(VRMDestination.transforms));
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
                    EditorGUILayout.LabelField(new GUIContent("MIT License", "by UniVRM"), EditorStyles.linkLabel);
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/vrm-c/UniVRM/blob/master/LICENSE.txt");
                    }
                }
            }

            ((VRMDestination)target).ProcessName = EditorGUILayout.TextField("Process name", ((VRMDestination)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("VRM setting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(property[nameof(VRMDestination.proxy)], new GUIContent("Blend shape proxy"));
            EditorGUILayout.PropertyField(property[nameof(VRMDestination.transforms)], new GUIContent("Target transforms"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
