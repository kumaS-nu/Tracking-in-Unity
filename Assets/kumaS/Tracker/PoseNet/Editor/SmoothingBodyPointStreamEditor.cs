using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.PoseNet.Editor
{
    [CustomEditor(typeof(SmoothingBodyPointStream))]
    public class SmoothingBodyPointStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        private void OnEnable()
        {
            property[nameof(SmoothingBodyPointStream.bufferSize)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.bufferSize));
            property[nameof(SmoothingBodyPointStream.rotationRange)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.rotationRange));
            property[nameof(SmoothingBodyPointStream.rotationZRange)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.rotationZRange));
            property[nameof(SmoothingBodyPointStream.rotationSpeedLimit)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.rotationSpeedLimit));
            property[nameof(SmoothingBodyPointStream.xMin)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.xMin));
            property[nameof(SmoothingBodyPointStream.xMax)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.xMax));
            property[nameof(SmoothingBodyPointStream.yMin)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.yMin));
            property[nameof(SmoothingBodyPointStream.yMax)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.yMax));
            property[nameof(SmoothingBodyPointStream.zMin)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.zMin));
            property[nameof(SmoothingBodyPointStream.zMax)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.zMax));
            property[nameof(SmoothingBodyPointStream.speedLimit)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.speedLimit));
            property[nameof(SmoothingBodyPointStream.resetKey)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.resetKey));
            property[nameof(SmoothingBodyPointStream.fold0)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.fold0));
            property[nameof(SmoothingBodyPointStream.fold1)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.fold1));
            property[nameof(SmoothingBodyPointStream.fold2)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.fold2));
            property[nameof(SmoothingBodyPointStream.fold3)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.fold3));
            property[nameof(SmoothingBodyPointStream.fold4)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.fold4));
            property[nameof(SmoothingBodyPointStream.isDebug)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.isDebug));
            property[nameof(SmoothingBodyPointStream.isDebugPosition)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.isDebugPosition));
            property[nameof(SmoothingBodyPointStream.isDebugRotation)] = serializedObject.FindProperty(nameof(SmoothingBodyPointStream.isDebugRotation));
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
                EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.isDebug)], new GUIContent("Debug"));
                if (((SmoothingBodyPointStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.isDebugPosition)], new GUIContent("Position"));
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.isDebugRotation)], new GUIContent("Rotation"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Smoothing setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(SmoothingBodyPointStream.bufferSize)].intValue = EditorGUILayout.IntSlider("Buffer size", property[nameof(SmoothingBodyPointStream.bufferSize)].intValue, 1, 32);
                property[nameof(SmoothingBodyPointStream.fold0)].boolValue = EditorGUILayout.Foldout(property[nameof(SmoothingBodyPointStream.fold0)].boolValue, "Root range");
                if (property[nameof(SmoothingBodyPointStream.fold0)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.xMin)], new GUIContent("X min"));
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.xMax)], new GUIContent("X max"));
                        }
                        var xmin = property[nameof(SmoothingBodyPointStream.xMin)].floatValue;
                        var xmax = property[nameof(SmoothingBodyPointStream.xMax)].floatValue;
                        EditorGUILayout.MinMaxSlider("X range", ref xmin, ref xmax, -10, 10);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.yMin)], new GUIContent("Y min"));
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.yMax)], new GUIContent("Y max"));
                        }
                        var ymin = property[nameof(SmoothingBodyPointStream.yMin)].floatValue;
                        var ymax = property[nameof(SmoothingBodyPointStream.yMax)].floatValue;
                        EditorGUILayout.MinMaxSlider("Y range", ref ymin, ref ymax, -10, 10);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.zMin)], new GUIContent("Z min"));
                            EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.zMax)], new GUIContent("Z max"));
                        }
                        var zmin = property[nameof(SmoothingBodyPointStream.zMin)].floatValue;
                        var zmax = property[nameof(SmoothingBodyPointStream.zMax)].floatValue;
                        EditorGUILayout.MinMaxSlider("Z range", ref zmin, ref zmax, -10, 10);
                    }
                }

                property[nameof(SmoothingBodyPointStream.fold1)].boolValue = EditorGUILayout.Foldout(property[nameof(SmoothingBodyPointStream.fold1)].boolValue, "Root speed limit");
                if (property[nameof(SmoothingBodyPointStream.fold1)].boolValue)
                {
                    EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.speedLimit)], new GUIContent("Speed limit"));
                }

                property[nameof(SmoothingBodyPointStream.fold2)].boolValue = EditorGUILayout.Foldout(property[nameof(SmoothingBodyPointStream.fold2)].boolValue, "Rotation range (deg)");
                if (property[nameof(SmoothingBodyPointStream.fold2)].boolValue)
                {
                    foreach (var idx in SmoothingBodyPointStream.useIndex)
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.rotationRange)].GetArrayElementAtIndex(idx), new GUIContent(PredictedModelData.DefaultRotationList[idx]));
                    }
                }

                property[nameof(SmoothingBodyPointStream.fold3)].boolValue = EditorGUILayout.Foldout(property[nameof(SmoothingBodyPointStream.fold3)].boolValue, "Rotation z range (deg)");
                if (property[nameof(SmoothingBodyPointStream.fold3)].boolValue)
                {
                    foreach (var idx in SmoothingBodyPointStream.useIndex)
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.rotationZRange)].GetArrayElementAtIndex(idx), new GUIContent(PredictedModelData.DefaultRotationList[idx]));
                    }
                }

                property[nameof(SmoothingBodyPointStream.fold4)].boolValue = EditorGUILayout.Foldout(property[nameof(SmoothingBodyPointStream.fold4)].boolValue, "Rotation speed limit");
                if (property[nameof(SmoothingBodyPointStream.fold4)].boolValue)
                {
                    foreach (var idx in SmoothingBodyPointStream.useIndex)
                    {
                        EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.rotationSpeedLimit)].GetArrayElementAtIndex(idx), new GUIContent(PredictedModelData.DefaultRotationList[idx]));
                    }
                }

                EditorGUILayout.PropertyField(property[nameof(SmoothingBodyPointStream.resetKey)], new GUIContent("Reset key"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
