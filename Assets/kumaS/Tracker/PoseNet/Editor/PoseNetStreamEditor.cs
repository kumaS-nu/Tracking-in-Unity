using kumaS.Tracker.Core;

using OpenCvSharp;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.PoseNet.Editor
{
    [CustomEditor(typeof(PoseNetStream))]
    public class PoseNetStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private readonly string[] settingType = new string[] { "Focal length", "FoV & width" };
        private string[] interpolationLabel;
        private string[] pathTypeLabel;
        private int[] pathTypeIndex;

        private void OnEnable()
        {
            property[nameof(PoseNetStream.modelFile)] = serializedObject.FindProperty(nameof(PoseNetStream.modelFile));
            property[nameof(PoseNetStream.debugImage)] = serializedObject.FindProperty(nameof(PoseNetStream.debugImage));
            property[nameof(PoseNetStream.interval)] = serializedObject.FindProperty(nameof(PoseNetStream.interval));
            property[nameof(PoseNetStream.markColor)] = serializedObject.FindProperty(nameof(PoseNetStream.markColor));
            property[nameof(PoseNetStream.markSize)] = serializedObject.FindProperty(nameof(PoseNetStream.markSize));
            property[nameof(PoseNetStream.fontScale)] = serializedObject.FindProperty(nameof(PoseNetStream.fontScale));
            property[nameof(PoseNetStream.modelType)] = serializedObject.FindProperty(nameof(PoseNetStream.modelType));
            property[nameof(PoseNetStream.stride)] = serializedObject.FindProperty(nameof(PoseNetStream.stride));
            property[nameof(PoseNetStream.interpolation)] = serializedObject.FindProperty(nameof(PoseNetStream.interpolation));
            property[nameof(PoseNetStream.isDefaultInputSize)] = serializedObject.FindProperty(nameof(PoseNetStream.isDefaultInputSize));
            property[nameof(PoseNetStream.inputResolution)] = serializedObject.FindProperty(nameof(PoseNetStream.inputResolution));
            property[nameof(PoseNetStream.minScore)] = serializedObject.FindProperty(nameof(PoseNetStream.minScore));
            property[nameof(PoseNetStream.isDebug)] = serializedObject.FindProperty(nameof(PoseNetStream.isDebug));
            property[nameof(PoseNetStream.isDebugLandmark)] = serializedObject.FindProperty(nameof(PoseNetStream.isDebugLandmark));
            property[nameof(PoseNetStream.isDebugImage)] = serializedObject.FindProperty(nameof(PoseNetStream.isDebugImage));
            property[nameof(PoseNetStream.isDebugIndex)] = serializedObject.FindProperty(nameof(PoseNetStream.isDebugIndex));
            if (interpolationLabel == null)
            {
                interpolationLabel = new string[6];
                for (var i = 0; i < 6; i++)
                {
                    interpolationLabel[i] = ((InterpolationFlags)i).ToString();
                }
            }
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
                    EditorGUILayout.LabelField(new GUIContent("Apache License 2.0", "by PoseNet, tf2onnx"), EditorStyles.linkLabel);
                    UnityEngine.Rect rect = GUILayoutUtility.GetLastRect();
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    Event nowEvent = Event.current;
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/tensorflow/tfjs-models/blob/master/LICENSE");
                        Help.BrowseURL("https://github.com/onnx/tensorflow-onnx/blob/master/LICENSE");
                    }

                    EditorGUILayout.LabelField(new GUIContent("MIT License", "by tfjs-to-tf"), EditorStyles.linkLabel);
                    rect = GUILayoutUtility.GetLastRect();
                    if (nowEvent.type == EventType.MouseDown && rect.Contains(nowEvent.mousePosition))
                    {
                        Help.BrowseURL("https://github.com/patlevin/tfjs-to-tf/blob/master/LICENSE");
                    }
                }
            }

            ((ISchedule)target).ProcessName = EditorGUILayout.TextField("Process name", ((ISchedule)target).ProcessName);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.isDebug)], new GUIContent("Debug"));
                if (((PoseNetStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(PoseNetStream.isDebugLandmark)], new GUIContent("Landmarks"));
                        EditorGUILayout.PropertyField(property[nameof(PoseNetStream.isDebugImage)], new GUIContent("by Image"));
                        if (property[nameof(PoseNetStream.isDebugImage)].boolValue)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.debugImage)], new GUIContent("Image debugger"));
                                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.markColor)], new GUIContent("Mark color"));
                                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.markSize)], new GUIContent("Mark size"));
                                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.interval)], new GUIContent("Interval"));
                                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.isDebugIndex)], new GUIContent("with Index"));
                                if (property[nameof(PoseNetStream.isDebugIndex)].boolValue)
                                {
                                    EditorGUILayout.PropertyField(property[nameof(PoseNetStream.fontScale)], new GUIContent("Font scale"));
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
                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.modelFile)], new GUIContent("Model file"));

                property[nameof(PoseNetStream.modelType)].intValue = EditorGUILayout.IntPopup("Model type", property[nameof(PoseNetStream.modelType)].intValue, PoseNetStream.MODEL_TYPE, PoseNetStream.MODEL_TYPE_INDEX);
                int[] stride = default;
                if (property[nameof(PoseNetStream.modelType)].intValue == 0)
                {
                    stride = PoseNetStream.MOBILE_NET_STRIDE;
                }
                else
                {
                    stride = PoseNetStream.RES_NET_STRIDE;
                }
                property[nameof(PoseNetStream.stride)].intValue = EditorGUILayout.IntPopup("Stride", property[nameof(PoseNetStream.stride)].intValue, stride.Select(val => val.ToString()).ToArray(), stride);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Input setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(PoseNetStream.isDefaultInputSize)], new GUIContent("Default size"));
                if (!property[nameof(PoseNetStream.isDefaultInputSize)].boolValue)
                {
                    EditorGUILayout.PropertyField(property[nameof(PoseNetStream.inputResolution)], new GUIContent("Resolution"));
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Detect setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(PoseNetStream.minScore)].floatValue = EditorGUILayout.Slider("Threshold score", property[nameof(PoseNetStream.minScore)].floatValue, -1000, 0);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
