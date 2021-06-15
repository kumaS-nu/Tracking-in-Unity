using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.VMCProtocol.Editor
{
    [CustomEditor(typeof(VMTDestination))]
    internal class VMTDestinationEditor : UnityEditor.Editor
    {

        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(VMTDestination.adress)] = serializedObject.FindProperty(nameof(VMTDestination.adress));
            property[nameof(VMTDestination.sendRate)] = serializedObject.FindProperty(nameof(VMTDestination.sendRate));
            property[nameof(VMTDestination.fold0)] = serializedObject.FindProperty(nameof(VMTDestination.fold0));
            property[nameof(VMTDestination.trackerLabel)] = serializedObject.FindProperty(nameof(VMTDestination.trackerLabel));
            property[nameof(VMTDestination.trackerIndex)] = serializedObject.FindProperty(nameof(VMTDestination.trackerIndex));
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
                    EditorGUILayout.LabelField(new GUIContent("MIT License", "by uOSC"), EditorStyles.linkLabel);
                    Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/hecomi/uOSC");
                    }
                }
            }

            ((VMTDestination)target).ProcessName = EditorGUILayout.TextField("Process name", ((VMTDestination)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("VMC protocol setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(VMTDestination.adress)], new GUIContent("Adress"));
                EditorGUILayout.PropertyField(property[nameof(VMTDestination.sendRate)], new GUIContent("Send rate"));
                if (property[nameof(VMTDestination.sendRate)].intValue <= 0)
                {
                    EditorGUILayout.HelpBox("無効な値です", MessageType.Error);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Send data setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(VMTDestination.fold0)].boolValue = EditorGUILayout.Foldout(property[nameof(VMTDestination.fold0)].boolValue, "Tracker");
                if (property[nameof(VMTDestination.fold0)].boolValue)
                {
                    var arraySize = EditorGUILayout.DelayedIntField("Size", property[nameof(VMTDestination.trackerLabel)].arraySize);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Label name");
                        EditorGUILayout.LabelField("Index");
                    }
                    Color defaultColor = GUI.backgroundColor;
                    var invalid = Enumerable.Range(0, property[nameof(VMTDestination.trackerLabel)].arraySize).Select(i => property[nameof(VMTDestination.trackerIndex)].GetArrayElementAtIndex(i).intValue)
                        .GroupBy(i => i).Where(g => g.Count() > 1 || g.Key < 0 || g.Key > 57).Select(g => g.Key).ToArray();
                    for (var i = 0; i < property[nameof(VMTDestination.trackerLabel)].arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            property[nameof(VMTDestination.trackerLabel)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMTDestination.trackerLabel)].GetArrayElementAtIndex(i).stringValue);

                            if (invalid.Contains(property[nameof(VMTDestination.trackerIndex)].GetArrayElementAtIndex(i).intValue))
                            {
                                GUI.backgroundColor = Color.red;
                            }
                            property[nameof(VMTDestination.trackerIndex)].GetArrayElementAtIndex(i).intValue = EditorGUILayout.IntField(property[nameof(VMTDestination.trackerIndex)].GetArrayElementAtIndex(i).intValue);
                            GUI.backgroundColor = defaultColor;
                        }
                    }
                    if (invalid.Any())
                    {
                        EditorGUILayout.HelpBox("無効なインデックスがあります。", MessageType.Error);
                    }

                    property[nameof(VMTDestination.trackerLabel)].arraySize = arraySize;
                    property[nameof(VMTDestination.trackerIndex)].arraySize = arraySize;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}

