using kumaS.Tracker.Core;

using OpenCvSharp;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace kumaS.Tracker.FaceMesh.Editor
{
    [CustomEditor(typeof(FaceMeshStream))]
    public class FaceMeshStreamEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();
        private string[] interpolationLabel;
        private string[] pathTypeLabel;
        private int[] pathTypeIndex;

        private void OnEnable()
        {
            property[nameof(FaceMeshStream.filePath)] = serializedObject.FindProperty(nameof(FaceMeshStream.filePath));
            property[nameof(FaceMeshStream.pathType)] = serializedObject.FindProperty(nameof(FaceMeshStream.pathType));
            property[nameof(FaceMeshStream.interpolation)] = serializedObject.FindProperty(nameof(FaceMeshStream.interpolation));
            property[nameof(FaceMeshStream.minScore)] = serializedObject.FindProperty(nameof(FaceMeshStream.minScore));
            property[nameof(FaceMeshStream.isDebug)] = serializedObject.FindProperty(nameof(FaceMeshStream.isDebug));
            property[nameof(FaceMeshStream.isDebugLandmark)] = serializedObject.FindProperty(nameof(FaceMeshStream.isDebugLandmark));
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
                    EditorGUILayout.LabelField(new GUIContent("Apache License 2.0", "by face-landmark-detection, tf2onnx"), EditorStyles.linkLabel);
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
                EditorGUILayout.PropertyField(property[nameof(FaceMeshStream.isDebug)], new GUIContent("Debug"));
                if (((FaceMeshStream)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(FaceMeshStream.isDebugLandmark)], new GUIContent("Landmarks"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Predict setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                var filePath = property[nameof(FaceMeshStream.filePath)].stringValue;
                var pathType = property[nameof(FaceMeshStream.pathType)].intValue;
                filePath = PathUtil.Deserialize(filePath, pathType);
                pathType = EditorGUILayout.IntPopup("Path type", pathType, pathTypeLabel, pathTypeIndex);
                filePath = EditorGUILayout.TextField("File path", filePath);
                filePath = PathUtil.Serialize(filePath, pathType, false);

                if (filePath == "" || !File.Exists(filePath))
                {
                    EditorGUILayout.HelpBox("モデルファイルが見つかりませんでした。", MessageType.Error);
                }
                property[nameof(FaceMeshStream.filePath)].stringValue = filePath;
                property[nameof(FaceMeshStream.pathType)].intValue = pathType;
                property[nameof(FaceMeshStream.interpolation)].enumValueIndex = EditorGUILayout.Popup("Interpolation", property[nameof(FaceMeshStream.interpolation)].enumValueIndex, interpolationLabel);
                property[nameof(FaceMeshStream.minScore)].floatValue = EditorGUILayout.Slider("Threshold score", property[nameof(FaceMeshStream.minScore)].floatValue, 0, 1);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
