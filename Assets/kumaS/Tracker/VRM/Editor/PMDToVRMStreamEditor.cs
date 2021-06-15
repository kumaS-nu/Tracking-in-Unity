using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.VRM.Editor
{
    [CustomEditor(typeof(PMDToVRMStream))]
    public class PMDToVRMStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(PMDToVRMStream.fold0)] = serializedObject.FindProperty(nameof(PMDToVRMStream.fold0));
            property[nameof(PMDToVRMStream.PMDPosition)] = serializedObject.FindProperty(nameof(PMDToVRMStream.PMDPosition));
            property[nameof(PMDToVRMStream.VRMPosition)] = serializedObject.FindProperty(nameof(PMDToVRMStream.VRMPosition));
            property[nameof(PMDToVRMStream.fold1)] = serializedObject.FindProperty(nameof(PMDToVRMStream.fold1));
            property[nameof(PMDToVRMStream.PMDRotation)] = serializedObject.FindProperty(nameof(PMDToVRMStream.PMDRotation));
            property[nameof(PMDToVRMStream.VRMRotation)] = serializedObject.FindProperty(nameof(PMDToVRMStream.VRMRotation));
            property[nameof(PMDToVRMStream.RotationOffset)] = serializedObject.FindProperty(nameof(PMDToVRMStream.RotationOffset));
            property[nameof(PMDToVRMStream.fold2)] = serializedObject.FindProperty(nameof(PMDToVRMStream.fold2));
            property[nameof(PMDToVRMStream.PMDParameter)] = serializedObject.FindProperty(nameof(PMDToVRMStream.PMDParameter));
            property[nameof(PMDToVRMStream.VRMParameter)] = serializedObject.FindProperty(nameof(PMDToVRMStream.VRMParameter));
            property[nameof(PMDToVRMStream.fold3)] = serializedObject.FindProperty(nameof(PMDToVRMStream.fold3));
            property[nameof(PMDToVRMStream.PMDOption)] = serializedObject.FindProperty(nameof(PMDToVRMStream.PMDOption));
            property[nameof(PMDToVRMStream.VRMOption)] = serializedObject.FindProperty(nameof(PMDToVRMStream.VRMOption));
            property[nameof(PMDToVRMStream.isDebug)] = serializedObject.FindProperty(nameof(PMDToVRMStream.isDebug));
            property[nameof(PMDToVRMStream.isDebugPosition)] = serializedObject.FindProperty(nameof(PMDToVRMStream.isDebugPosition));
            property[nameof(PMDToVRMStream.isDebugRotation)] = serializedObject.FindProperty(nameof(PMDToVRMStream.isDebugRotation));
            property[nameof(PMDToVRMStream.isDebugParameter)] = serializedObject.FindProperty(nameof(PMDToVRMStream.isDebugParameter));
            property[nameof(PMDToVRMStream.isDebugOption)] = serializedObject.FindProperty(nameof(PMDToVRMStream.isDebugOption));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PMDToVRMStream.isDebug)], new GUIContent("Debug"));
                if (((PMDToVRMStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(PMDToVRMStream.isDebugPosition)], new GUIContent("Position"));
                        EditorGUILayout.PropertyField(property[nameof(PMDToVRMStream.isDebugRotation)], new GUIContent("Rotation"));
                        EditorGUILayout.PropertyField(property[nameof(PMDToVRMStream.isDebugParameter)], new GUIContent("Parameter"));
                        EditorGUILayout.PropertyField(property[nameof(PMDToVRMStream.isDebugOption)], new GUIContent("Option"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Convert setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.Space();
                property[nameof(PMDToVRMStream.fold0)].boolValue = EditorGUILayout.Foldout(property[nameof(PMDToVRMStream.fold0)].boolValue, "Position");
                if (property[nameof(PMDToVRMStream.fold0)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var size = EditorGUILayout.DelayedIntField("Size", property[nameof(PMDToVRMStream.PMDPosition)].arraySize);
                        EditorGUILayout.Space();
                        property[nameof(PMDToVRMStream.PMDPosition)].arraySize = size;
                        property[nameof(PMDToVRMStream.VRMPosition)].arraySize = size;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("PMD label");
                            EditorGUILayout.LabelField("VRM label");
                        }
                        EditorGUILayout.Space();
                        for (var i = 0; i < property[nameof(PMDToVRMStream.PMDPosition)].arraySize; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                property[nameof(PMDToVRMStream.PMDPosition)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.PMDPosition)].GetArrayElementAtIndex(i).stringValue);
                                property[nameof(PMDToVRMStream.VRMPosition)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.VRMPosition)].GetArrayElementAtIndex(i).stringValue);
                            }
                        }
                    }
                }

                EditorGUILayout.Space();
                property[nameof(PMDToVRMStream.fold1)].boolValue = EditorGUILayout.Foldout(property[nameof(PMDToVRMStream.fold1)].boolValue, "Rotation");
                if (property[nameof(PMDToVRMStream.fold1)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var size = EditorGUILayout.DelayedIntField("Size", property[nameof(PMDToVRMStream.PMDRotation)].arraySize);
                        EditorGUILayout.Space();
                        property[nameof(PMDToVRMStream.PMDRotation)].arraySize = size;
                        property[nameof(PMDToVRMStream.VRMRotation)].arraySize = size;
                        property[nameof(PMDToVRMStream.RotationOffset)].arraySize = size;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("PMD label");
                            EditorGUILayout.LabelField("VRM label");
                        }
                        EditorGUILayout.Space();
                        for (var i = 0; i < property[nameof(PMDToVRMStream.PMDRotation)].arraySize; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                property[nameof(PMDToVRMStream.PMDRotation)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.PMDRotation)].GetArrayElementAtIndex(i).stringValue);
                                property[nameof(PMDToVRMStream.VRMRotation)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.VRMRotation)].GetArrayElementAtIndex(i).stringValue);
                            }
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Offset", EditorStyles.boldLabel);
                        for (var i = 0; i < property[nameof(PMDToVRMStream.PMDRotation)].arraySize; i++)
                        {
                            property[nameof(PMDToVRMStream.RotationOffset)].GetArrayElementAtIndex(i).vector3Value =
                                EditorGUILayout.Vector3Field(property[nameof(PMDToVRMStream.VRMRotation)].GetArrayElementAtIndex(i).stringValue, property[nameof(PMDToVRMStream.RotationOffset)].GetArrayElementAtIndex(i).vector3Value);
                        }
                    }
                }

                EditorGUILayout.Space();
                property[nameof(PMDToVRMStream.fold2)].boolValue = EditorGUILayout.Foldout(property[nameof(PMDToVRMStream.fold2)].boolValue, "Parameter");
                if (property[nameof(PMDToVRMStream.fold2)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var size = EditorGUILayout.DelayedIntField("Size", property[nameof(PMDToVRMStream.PMDParameter)].arraySize);
                        EditorGUILayout.Space();
                        property[nameof(PMDToVRMStream.PMDParameter)].arraySize = size;
                        property[nameof(PMDToVRMStream.VRMParameter)].arraySize = size;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("PMD label");
                            EditorGUILayout.LabelField("VRM label");
                        }
                        EditorGUILayout.Space();
                        for (var i = 0; i < property[nameof(PMDToVRMStream.PMDParameter)].arraySize; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                property[nameof(PMDToVRMStream.PMDParameter)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.PMDParameter)].GetArrayElementAtIndex(i).stringValue);
                                property[nameof(PMDToVRMStream.VRMParameter)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.VRMParameter)].GetArrayElementAtIndex(i).stringValue);
                            }
                        }
                    }
                }

                EditorGUILayout.Space();
                property[nameof(PMDToVRMStream.fold3)].boolValue = EditorGUILayout.Foldout(property[nameof(PMDToVRMStream.fold3)].boolValue, "Option");
                if (property[nameof(PMDToVRMStream.fold3)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var size = EditorGUILayout.DelayedIntField("Size", property[nameof(PMDToVRMStream.PMDOption)].arraySize);
                        EditorGUILayout.Space();
                        property[nameof(PMDToVRMStream.PMDOption)].arraySize = size;
                        property[nameof(PMDToVRMStream.VRMOption)].arraySize = size;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("PMD label");
                            EditorGUILayout.LabelField("VRM label");
                        }
                        EditorGUILayout.Space();
                        for (var i = 0; i < property[nameof(PMDToVRMStream.PMDOption)].arraySize; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                property[nameof(PMDToVRMStream.PMDOption)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.PMDOption)].GetArrayElementAtIndex(i).stringValue);
                                property[nameof(PMDToVRMStream.VRMOption)].GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TextField(property[nameof(PMDToVRMStream.VRMOption)].GetArrayElementAtIndex(i).stringValue);
                            }
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
