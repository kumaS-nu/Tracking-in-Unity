using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.VMCProtocol.Editor
{
    [CustomEditor(typeof(VMCProtocolDestination))]
    public class VMCProtocolDestinationEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(VMCProtocolDestination.adress)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.adress));
            property[nameof(VMCProtocolDestination.sendRate)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.sendRate));
            property[nameof(VMCProtocolDestination.fold0)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.fold0));
            property[nameof(VMCProtocolDestination.fold1)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.fold1));
            property[nameof(VMCProtocolDestination.fold2)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.fold2));
            property[nameof(VMCProtocolDestination.fold3)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.fold3));
            property[nameof(VMCProtocolDestination.hmdLabel)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.hmdLabel));
            property[nameof(VMCProtocolDestination.hmdSerial)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.hmdSerial));
            property[nameof(VMCProtocolDestination.trackerLabel)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.trackerLabel));
            property[nameof(VMCProtocolDestination.trackerSerial)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.trackerSerial));
            property[nameof(VMCProtocolDestination.blendShapeLabel)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.blendShapeLabel));
            property[nameof(VMCProtocolDestination.blendShapeSerial)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.blendShapeSerial));
            property[nameof(VMCProtocolDestination.eyeLabel)] = serializedObject.FindProperty(nameof(VMCProtocolDestination.eyeLabel));
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

            ((VMCProtocolDestination)target).ProcessName = EditorGUILayout.TextField("Process name", ((VMCProtocolDestination)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("VMC protocol setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(VMCProtocolDestination.adress)], new GUIContent("Adress"));
                EditorGUILayout.PropertyField(property[nameof(VMCProtocolDestination.sendRate)], new GUIContent("Send rate"));
                if (property[nameof(VMCProtocolDestination.sendRate)].intValue <= 0)
                {
                    EditorGUILayout.HelpBox("無効な値です", MessageType.Error);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Send data setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(VMCProtocolDestination.fold0)].boolValue = EditorGUILayout.Foldout(property[nameof(VMCProtocolDestination.fold0)].boolValue, "HMD");
                if (property[nameof(VMCProtocolDestination.fold0)].boolValue)
                {
                    var arraySize = EditorGUILayout.DelayedIntField("Size", property[nameof(VMCProtocolDestination.hmdLabel)].arraySize);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Label name");
                        EditorGUILayout.LabelField("Serial name");
                    }
                    for (var i = 0; i < property[nameof(VMCProtocolDestination.hmdLabel)].arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            property[nameof(VMCProtocolDestination.hmdLabel)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.hmdLabel)].GetArrayElementAtIndex(i).stringValue);
                            property[nameof(VMCProtocolDestination.hmdSerial)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.hmdSerial)].GetArrayElementAtIndex(i).stringValue);
                        }
                    }
                    property[nameof(VMCProtocolDestination.hmdLabel)].arraySize = arraySize;
                    property[nameof(VMCProtocolDestination.hmdSerial)].arraySize = arraySize;
                }

                property[nameof(VMCProtocolDestination.fold1)].boolValue = EditorGUILayout.Foldout(property[nameof(VMCProtocolDestination.fold1)].boolValue, "Tracker");
                if (property[nameof(VMCProtocolDestination.fold1)].boolValue)
                {
                    var arraySize = EditorGUILayout.DelayedIntField("Size", property[nameof(VMCProtocolDestination.trackerLabel)].arraySize);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Label name");
                        EditorGUILayout.LabelField("Serial name");
                    }
                    for (var i = 0; i < property[nameof(VMCProtocolDestination.trackerLabel)].arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            property[nameof(VMCProtocolDestination.trackerLabel)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.trackerLabel)].GetArrayElementAtIndex(i).stringValue);
                            property[nameof(VMCProtocolDestination.trackerSerial)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.trackerSerial)].GetArrayElementAtIndex(i).stringValue);
                        }
                    }
                    property[nameof(VMCProtocolDestination.trackerLabel)].arraySize = arraySize;
                    property[nameof(VMCProtocolDestination.trackerSerial)].arraySize = arraySize;
                }

                property[nameof(VMCProtocolDestination.fold2)].boolValue = EditorGUILayout.Foldout(property[nameof(VMCProtocolDestination.fold2)].boolValue, "BlendShape");
                if (property[nameof(VMCProtocolDestination.fold2)].boolValue)
                {
                    var arraySize = EditorGUILayout.DelayedIntField("Size", property[nameof(VMCProtocolDestination.blendShapeLabel)].arraySize);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Label name");
                        EditorGUILayout.LabelField("Serial name");
                    }
                    for (var i = 0; i < property[nameof(VMCProtocolDestination.blendShapeLabel)].arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            property[nameof(VMCProtocolDestination.blendShapeLabel)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.blendShapeLabel)].GetArrayElementAtIndex(i).stringValue);
                            property[nameof(VMCProtocolDestination.blendShapeSerial)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.blendShapeSerial)].GetArrayElementAtIndex(i).stringValue);
                        }
                    }
                    property[nameof(VMCProtocolDestination.blendShapeLabel)].arraySize = arraySize;
                    property[nameof(VMCProtocolDestination.blendShapeSerial)].arraySize = arraySize;
                }

                property[nameof(VMCProtocolDestination.fold3)].boolValue = EditorGUILayout.Foldout(property[nameof(VMCProtocolDestination.fold3)].boolValue, "Eye");
                if (property[nameof(VMCProtocolDestination.fold3)].boolValue)
                {
                    var arraySize = EditorGUILayout.DelayedIntField("Size", property[nameof(VMCProtocolDestination.eyeLabel)].arraySize);
                    EditorGUILayout.LabelField("Label name");
                    for (var i = 0; i < property[nameof(VMCProtocolDestination.eyeLabel)].arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            property[nameof(VMCProtocolDestination.eyeLabel)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(VMCProtocolDestination.eyeLabel)].GetArrayElementAtIndex(i).stringValue);
                        }
                    }

                    if (arraySize > 2)
                    {
                        arraySize = 2;
                    }

                    property[nameof(VMCProtocolDestination.eyeLabel)].arraySize = arraySize;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
