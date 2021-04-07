using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using kumaS.Tracker.Core;

namespace kumaS.Tracker.Core.Editor
{
    [CustomEditor(typeof(StreamScheduler))]
    public class StreamSchedulerEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> property = new Dictionary<string, SerializedProperty>();

        [SerializeField]
        private int selected = -1;
        [SerializeField]
        private MonoBehaviour newSourceObject = null;
        [SerializeField]
        private List<MonoBehaviour> newStreamNodeObjects = new List<MonoBehaviour>();
        [SerializeField]
        private MonoBehaviour newDestinationObject = null;

        private void OnEnable()
        {
            property[nameof(StreamScheduler.sourceFold)] = serializedObject.FindProperty(nameof(StreamScheduler.sourceFold));
            property[nameof(StreamScheduler.streamFold)] = serializedObject.FindProperty(nameof(StreamScheduler.streamFold));
            property[nameof(StreamScheduler.streamFolds)] = serializedObject.FindProperty(nameof(StreamScheduler.streamFolds));
            property[nameof(StreamScheduler.fpsFold)] = serializedObject.FindProperty(nameof(StreamScheduler.fpsFold));
            property[nameof(StreamScheduler.destinationFold)] = serializedObject.FindProperty(nameof(StreamScheduler.destinationFold));
            property[nameof(StreamScheduler.sources)] = serializedObject.FindProperty(nameof(StreamScheduler.sources));
            property[nameof(StreamScheduler.streams)] = serializedObject.FindProperty(nameof(StreamScheduler.streams));
            property[nameof(StreamScheduler.destinations)] = serializedObject.FindProperty(nameof(StreamScheduler.destinations));
            property[nameof(StreamScheduler.streamUnitStarts)] = serializedObject.FindProperty(nameof(StreamScheduler.streamUnitStarts));
            property[nameof(StreamScheduler.streamInputs)] = serializedObject.FindProperty(nameof(StreamScheduler.streamInputs));
            property[nameof(StreamScheduler.streamOutputs)] = serializedObject.FindProperty(nameof(StreamScheduler.streamOutputs));
            property[nameof(StreamScheduler.fps)] = serializedObject.FindProperty(nameof(StreamScheduler.fps));
            property[nameof(StreamScheduler.thread)] = serializedObject.FindProperty(nameof(StreamScheduler.thread));
            property[nameof(StreamScheduler.isDebug)] = serializedObject.FindProperty(nameof(StreamScheduler.isDebug));
            property[nameof(StreamScheduler.isWriteFile)] = serializedObject.FindProperty(nameof(StreamScheduler.isWriteFile));
            property[nameof(StreamScheduler.isDebugFPS)] = serializedObject.FindProperty(nameof(StreamScheduler.isDebugFPS));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var sources = GetSerializedList(property[nameof(StreamScheduler.sources)]);
            var streams = GetSerializedList(property[nameof(StreamScheduler.streams)]);
            var destinations = GetSerializedList(property[nameof(StreamScheduler.destinations)]);
            var units = GetSerializedListInt(property[nameof(StreamScheduler.streamUnitStarts)]);
            var inputs = GetSerializedListInt(property[nameof(StreamScheduler.streamInputs)]);
            var outputs = GetSerializedListInt(property[nameof(StreamScheduler.streamOutputs)]);
            var folds = GetSerializedListBool(property[nameof(StreamScheduler.streamFolds)]);
            var fpss = GetSerializedListInt(property[nameof(StreamScheduler.fps)]);
            var fpssInfo = GetFpss(inputs);

            var defaultColor = GUI.backgroundColor;
            var selectable = new List<Rect>();
            var counter = 0;

            if (newStreamNodeObjects.Count != units.Count)
            {
                foreach (var _ in units)
                {
                    newStreamNodeObjects.Add(null);
                }
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(property[nameof(StreamScheduler.isDebug)], new GUIContent("Debug"));
                if (((StreamScheduler)target).isDebug.Value)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(property[nameof(StreamScheduler.isWriteFile)], new GUIContent("Write to file"));
                        EditorGUILayout.PropertyField(property[nameof(StreamScheduler.isDebugFPS)], new GUIContent("Fps"));
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Performance setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                property[nameof(StreamScheduler.fpsFold)].boolValue = EditorGUILayout.Foldout(property[nameof(StreamScheduler.fpsFold)].boolValue, "Target FPS");
                if (property[nameof(StreamScheduler.fpsFold)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < fpss.Count; i++)
                        {
                            fpss[i] = EditorGUILayout.IntSlider("Stream" + fpssInfo[i].index + " [Source" + fpssInfo[i].source + "]", fpss[i], 1, 144);
                        }
                    }
                }
                if (fpss.Any(value => value <= 0))
                {
                    EditorGUILayout.HelpBox("FPSの制限なしで実行するソースがあります。", MessageType.Info);
                }
                property[nameof(StreamScheduler.thread)].intValue = EditorGUILayout.IntSlider("Number of threads to use", property[nameof(StreamScheduler.thread)].intValue, 1, 64);
                if (property[nameof(StreamScheduler.thread)].intValue <= 0)
                {
                    EditorGUILayout.HelpBox("スレッド数が0以下となってます。", MessageType.Error);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Stream setting", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                #region Source

                property[nameof(StreamScheduler.sourceFold)].boolValue = EditorGUILayout.Foldout(property[nameof(StreamScheduler.sourceFold)].boolValue, "Source");
                if (property[nameof(StreamScheduler.sourceFold)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < property[nameof(StreamScheduler.sources)].arraySize; i++)
                        {
                            ShowNode(property[nameof(StreamScheduler.sources)].GetArrayElementAtIndex(i), i, selected == i, selectable, defaultColor);
                        }

                        void DeleteSource()
                        {
                            sources.RemoveAt(selected);
                            inputs = inputs.Select(value => value == -1 - selected ? 0 : value).ToList();
                            if (selected == property[nameof(StreamScheduler.sources)].arraySize - 1)
                            {
                                selected = -1;
                            }
                        };

                        DeleteButton(selected >= 0 && selected < property[nameof(StreamScheduler.sources)].arraySize, DeleteSource);
                    }

                    newSourceObject = AddNodeForm("source", newSourceObject, typeof(IScheduleSource), (newNode) => sources.Add((MonoBehaviour)newNode));
                }
                else
                {
                    for (var i = 0; i < property[nameof(StreamScheduler.sources)].arraySize; i++)
                    {
                        selectable.Add(Rect.zero);
                    }
                }
                counter += property[nameof(StreamScheduler.sources)].arraySize;

                #endregion
                #region Stream

                property[nameof(StreamScheduler.streamFold)].boolValue = EditorGUILayout.Foldout(property[nameof(StreamScheduler.streamFold)].boolValue, "Stream");
                if (property[nameof(StreamScheduler.streamFold)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < property[nameof(StreamScheduler.streamUnitStarts)].arraySize; i++)
                        {
                            folds[i] = EditorGUILayout.Foldout(folds[i], "Stream " + i);
                            if (selected - property[nameof(StreamScheduler.sources)].arraySize == i)
                            {
                                GUI.backgroundColor = Color.blue;
                            }
                            if (folds[i])
                            {
                                using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    var next = i + 1 == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? property[nameof(StreamScheduler.streams)].arraySize : property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(i + 1).intValue;
                                    var now = property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(i).intValue;
                                    ShowStreamNode(defaultColor, now, next);

                                    void DeleteStreamNode()
                                    {
                                        streams.RemoveAt(next - 1);
                                        for (var j = i + 1; j < units.Count; j++)
                                        {
                                            --units[j];
                                        }
                                        outputs[i] = 0;
                                    };

                                    void AddStreamNode(Component newNode)
                                    {
                                        streams.Insert(next, (MonoBehaviour)newNode);
                                        for (var j = i + 1; j < units.Count; j++)
                                        {
                                            ++units[j];
                                        }
                                        newStreamNodeObjects[i] = null;
                                        if (now == next)
                                        {
                                            var del = fpssInfo.Where(value => value.index == i);
                                            if (del.Any())
                                            {
                                                var delIdx = fpssInfo.IndexOf(del.First());
                                                fpssInfo.RemoveAt(delIdx);
                                                fpss.RemoveAt(delIdx);
                                            }
                                            inputs[i] = 0;
                                        }
                                        outputs[i] = 0;
                                    };
                                    var acceptType = now == next ? null : ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(next - 1).objectReferenceValue).OutputType;
                                    DeleteButton(now != next, DeleteStreamNode);
                                    newStreamNodeObjects[i] = AddNodeForm("stream node", newStreamNodeObjects[i], typeof(IScheduleStream), AddStreamNode, acceptType);
                                    selectable.Add(scope.rect);
                                }
                            }
                            else
                            {
                                selectable.Add(Rect.zero);
                            }
                            GUI.backgroundColor = defaultColor;
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Add new stream"))
                            {
                                units.Add(streams.Count);
                                inputs.Add(0);
                                outputs.Add(0);
                                folds.Add(true);
                                newStreamNodeObjects.Add(null);
                            }
                            var isActive = selected >= counter && selected < counter + property[nameof(StreamScheduler.streamUnitStarts)].arraySize;
                            using (new EditorGUI.DisabledGroupScope(!isActive))
                            {
                                if (GUILayout.Button("Delete Stream"))
                                {
                                    var streamIndex = selected - counter;
                                    if (streams.Count > units[streamIndex])
                                    {
                                        var nextIndex = streamIndex + 1 == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? streams.Count : units[streamIndex + 1];
                                        var deleteCount = nextIndex - units[streamIndex];
                                        streams.RemoveRange(units[streamIndex], deleteCount);
                                        units = units.Select(value => units.IndexOf(value) > streamIndex ? value - deleteCount : value).ToList();
                                    }
                                    if (inputs[streamIndex] < 0)
                                    {
                                        var delIdx = fpssInfo.IndexOf(fpssInfo.Where(value => value.index == streamIndex).First());
                                        fpssInfo.RemoveAt(delIdx);
                                        fpss.RemoveAt(delIdx);
                                    }
                                    units.RemoveAt(streamIndex);
                                    inputs.RemoveAt(streamIndex);
                                    inputs = inputs.Select(value => value == 1 + streamIndex ? 0 : value).ToList();
                                    outputs.RemoveAt(streamIndex);
                                    outputs = outputs.Select(value => value == 1 + streamIndex ? 0 : value).ToList();
                                    folds.RemoveAt(streamIndex);
                                    newStreamNodeObjects.RemoveAt(streamIndex);
                                    if (selected == property[nameof(StreamScheduler.sources)].arraySize + property[nameof(StreamScheduler.streamUnitStarts)].arraySize - 1)
                                    {
                                        selected = -1;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < property[nameof(StreamScheduler.streamUnitStarts)].arraySize; i++)
                    {
                        selectable.Add(Rect.zero);
                    }
                }
                counter += property[nameof(StreamScheduler.streamUnitStarts)].arraySize;

                #endregion
                #region Destination

                property[nameof(StreamScheduler.destinationFold)].boolValue = EditorGUILayout.Foldout(property[nameof(StreamScheduler.destinationFold)].boolValue, "Destination");
                if (property[nameof(StreamScheduler.destinationFold)].boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            for (var i = 0; i < property[nameof(StreamScheduler.destinations)].arraySize; i++)
                            {
                                ShowNode(property[nameof(StreamScheduler.destinations)].GetArrayElementAtIndex(i), i, selected == counter + i, selectable, defaultColor);
                            }
                        }

                        void DeleteDestination()
                        {
                            destinations.RemoveAt(selected - counter);
                            outputs = outputs.Select(value => value == -1 + counter - selected ? 0 : value).ToList();
                            if (selected == counter + property[nameof(StreamScheduler.destinations)].arraySize - 1)
                            {
                                selected = -1;
                            }
                        };

                        DeleteButton(selected >= counter && selected < counter + property[nameof(StreamScheduler.destinations)].arraySize, DeleteDestination);
                    }

                    newDestinationObject = AddNodeForm("destination", newDestinationObject, typeof(IScheduleDestination), (newNode) => destinations.Add((MonoBehaviour)newNode));
                }
                else
                {
                    for (var i = 0; i < property[nameof(StreamScheduler.destinations)].arraySize; i++)
                    {
                        selectable.Add(Rect.zero);
                    }
                }
                counter += property[nameof(StreamScheduler.destinations)].arraySize;

                #endregion
                #region Conect

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Source", GUILayout.Width(75));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Stream", GUILayout.Width(75));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Destination", GUILayout.Width(75));
                    }

                    var beforeInputs = GetSerializedListInt(property[nameof(StreamScheduler.streamInputs)]);
                    var beforeOutputs = GetSerializedListInt(property[nameof(StreamScheduler.streamOutputs)]);
                    for (var i = 0; i < property[nameof(StreamScheduler.streamUnitStarts)].arraySize; i++)
                    {
                        var index = property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(i).intValue;
                        var nextIndex = i + 1 == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? property[nameof(StreamScheduler.streams)].arraySize : property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(i + 1).intValue;
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            List<string> sourceLabel = MakeSourceLabel(i, index, nextIndex);
                            List<string> destinationLabel = MakeDestinationLabel(i, index, nextIndex);
                            var inputCenter = sourceLabel.IndexOf("No source");
                            var outputCenter = destinationLabel.IndexOf("No destination");
                            beforeInputs[i] = EditorGUILayout.Popup(beforeInputs[i] + inputCenter, sourceLabel.ToArray()) - inputCenter;
                            EditorGUILayout.LabelField("Stream " + i, GUILayout.Width(75));
                            beforeOutputs[i] = EditorGUILayout.Popup(beforeOutputs[i] + outputCenter, destinationLabel.ToArray()) - outputCenter;
                        }
                    }

                    if (inputs.Count == beforeInputs.Count)
                    {
                        inputs = beforeInputs;
                        outputs = beforeOutputs;
                    }

                    var fpssInfoAfter = GetFpss(inputs);
                    UpdateFpss(fpss, fpssInfo, fpssInfoAfter);
                }

                #endregion
            }
            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                for (var i = 0; i < selectable.Count; i++)
                {
                    if (selectable[i].Contains(e.mousePosition))
                    {
                        selected = i;
                        break;
                    }
                }
            }

            SetSerializedList(property[nameof(StreamScheduler.sources)], sources);
            SetSerializedList(property[nameof(StreamScheduler.streams)], streams);
            SetSerializedList(property[nameof(StreamScheduler.destinations)], destinations);
            SetSerializedListInt(property[nameof(StreamScheduler.streamUnitStarts)], units);
            SetSerializedListInt(property[nameof(StreamScheduler.streamInputs)], inputs);
            SetSerializedListInt(property[nameof(StreamScheduler.streamOutputs)], outputs);
            SetSerializedListBool(property[nameof(StreamScheduler.streamFolds)], folds);
            SetSerializedListInt(property[nameof(StreamScheduler.fps)], fpss);
            serializedObject.ApplyModifiedProperties();

        }

        private void ShowStreamNode(Color defaultColor, int now, int next)
        {
            GUI.backgroundColor = defaultColor;
            using (new EditorGUI.DisabledGroupScope(true))
            {
                for (var j = now; j < next; j++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(j), new GUIContent((j - now).ToString(), ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(j).objectReferenceValue).ProcessName));
                    }
                }
            }
        }

        private static List<(int index, int source)> GetFpss(List<int> inputs)
        {
            var fpssInfo = new List<(int index, int source)>();
            for (var i = 0; i < inputs.Count; i++)
            {
                if (inputs[i] < 0)
                {
                    fpssInfo.Add((i, -1 - inputs[i]));
                }
            }

            return fpssInfo;
        }
        private void ShowNode(SerializedProperty node, int index, bool isSelected, List<Rect> selectable, Color defaultColor)
        {
            if (isSelected)
            {
                GUI.backgroundColor = Color.blue;
            }
            using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUI.backgroundColor = defaultColor;
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField(node, new GUIContent(index.ToString(), ((ISchedule)node.objectReferenceValue).ProcessName));
                }
                selectable.Add(scope.rect);
            }
        }
        private void DeleteButton(bool isActive, Action deleteAction)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledGroupScope(!isActive))
                {
                    if (GUILayout.Button("Delete"))
                    {
                        deleteAction();
                    }
                }
            }
        }

        private MonoBehaviour AddNodeForm(string label, MonoBehaviour newObject, Type acceptInterface, Action<Component> addAction, Type acceptType = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("New " + label);
                using (new EditorGUILayout.HorizontalScope())
                {
                    newObject = (MonoBehaviour)EditorGUILayout.ObjectField(newObject, typeof(MonoBehaviour), true);
                    var newNodes = newObject == null ? new Component[0] : newObject.GetComponents(acceptInterface);
                    if (acceptType != null)
                    {
                        newNodes = newNodes.Where(node => ((IScheduleStream)node).InputType == acceptType).ToArray();
                    }
                    var str = new List<string>();
                    for (var i = 0; i < newNodes.Length; i++)
                    {
                        str.Add(i + " : [" + newNodes[i].GetType().Name + "] " + ((ISchedule)newNodes[i]).ProcessName);
                    }
                    if (str.Any())
                    {
                        var newIndex = EditorGUILayout.Popup(-1, str.ToArray());
                        if (newIndex >= 0)
                        {
                            addAction(newNodes[newIndex]);
                            newObject = null;
                        }
                    }
                    else
                    {
                        EditorGUILayout.Popup(-1, new string[] { "No " + label });
                    }
                }
            }

            return newObject;
        }

        private List<string> MakeSourceLabel(int unitIndex, int streamIndex, int nextIndex)
        {
            var sourceLabel = new List<string>();
            Type allowSourceType = null;
            var outputIndex = property[nameof(StreamScheduler.streamOutputs)].GetArrayElementAtIndex(unitIndex).intValue;
            if (streamIndex != nextIndex)
            {
                allowSourceType = ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(streamIndex).objectReferenceValue).InputType;
            }
            else if (outputIndex > 0)
            {
                var idx = property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(outputIndex - 1).intValue;
                allowSourceType = ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(idx).objectReferenceValue).InputType;
            }
            else if (outputIndex < 0)
            {
                allowSourceType = ((IScheduleDestination)property[nameof(StreamScheduler.destinations)].GetArrayElementAtIndex(-1 - outputIndex).objectReferenceValue).DestinationType;
            }

            for (var j = 0; j < property[nameof(StreamScheduler.sources)].arraySize; j++)
            {
                if (allowSourceType == null || ((IScheduleSource)property[nameof(StreamScheduler.sources)].GetArrayElementAtIndex(j).objectReferenceValue).SourceType == allowSourceType)
                {
                    sourceLabel.Add("Source " + j);
                }
            }
            sourceLabel.Add("No source");
            for (var j = 0; j < property[nameof(StreamScheduler.streamUnitStarts)].arraySize; j++)
            {
                var nI = j + 1 == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? property[nameof(StreamScheduler.streams)].arraySize : property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(j + 1).intValue;
                if (nI != property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(j).intValue && (allowSourceType == null || ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(nI - 1).objectReferenceValue).OutputType == allowSourceType))
                {
                    sourceLabel.Add("Stream " + j);
                }
            }

            return sourceLabel;
        }

        private List<string> MakeDestinationLabel(int unitIndex, int streamIndex, int nextIndex)
        {
            var destinationLabel = new List<string>();
            Type allowType = null;
            var inputIndex = property[nameof(StreamScheduler.streamInputs)].GetArrayElementAtIndex(unitIndex).intValue;
            if (streamIndex != nextIndex)
            {
                allowType = ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(nextIndex - 1).objectReferenceValue).OutputType;
            }
            else if (inputIndex > 0)
            {
                var idx = inputIndex == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? property[nameof(StreamScheduler.streamUnitStarts)].arraySize : property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(inputIndex).intValue;
                allowType = ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(idx - 1).objectReferenceValue).OutputType;
            }
            else if (inputIndex < 0)
            {
                allowType = ((IScheduleSource)property[nameof(StreamScheduler.sources)].GetArrayElementAtIndex(-1 - inputIndex).objectReferenceValue).SourceType;
            }

            for (var j = 0; j < property[nameof(StreamScheduler.destinations)].arraySize; j++)
            {
                if (allowType == null || ((IScheduleDestination)property[nameof(StreamScheduler.destinations)].GetArrayElementAtIndex(j).objectReferenceValue).DestinationType == allowType)
                {
                    destinationLabel.Add("Destination " + j);
                }
            }
            destinationLabel.Add("No destination");
            for (var j = 0; j < property[nameof(StreamScheduler.streamUnitStarts)].arraySize; j++)
            {
                var nO = j + 1 == property[nameof(StreamScheduler.streamUnitStarts)].arraySize ? property[nameof(StreamScheduler.streams)].arraySize : property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(j + 1).intValue;
                if (nO != property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(j).intValue && (allowType == null || ((IScheduleStream)property[nameof(StreamScheduler.streams)].GetArrayElementAtIndex(property[nameof(StreamScheduler.streamUnitStarts)].GetArrayElementAtIndex(j).intValue).objectReferenceValue).InputType == allowType))
                {
                    destinationLabel.Add("Stream " + j);
                }
            }

            return destinationLabel;
        }

        private static void UpdateFpss(List<int> fpss, List<(int index, int source)> fpssInfo, List<(int index, int source)> fpssInfoAfter)
        {
            var bi = 0;
            var ai = 0;
            while (bi < fpssInfo.Count || ai < fpssInfoAfter.Count)
            {
                var bIdx = fpssInfo.Count == 0 ? int.MaxValue : bi == fpssInfo.Count ? int.MinValue : fpssInfo[bi].index;
                var aIdx = fpssInfoAfter.Count == 0 ? int.MaxValue : ai == fpssInfoAfter.Count ? int.MinValue : fpssInfoAfter[ai].index;
                if (bIdx == aIdx)
                {
                    if (bi >= 0 && bi < fpssInfo.Count && ai >= 0 && ai < fpssInfoAfter.Count && fpssInfo[bi].source != fpssInfoAfter[ai].source)
                    {
                        fpss[ai] = 60;
                    }
                    ++bi;
                    ++ai;
                }
                else if (bIdx > aIdx)
                {
                    fpss.Insert(ai, 60);
                    ++ai;
                }
                else
                {
                    fpss.RemoveAt(ai);
                    ++bi;
                }
            }
        }

        private List<MonoBehaviour> GetSerializedList(SerializedProperty property)
        {
            var ret = new List<MonoBehaviour>();
            for (var i = 0; i < property.arraySize; i++)
            {
                ret.Add((MonoBehaviour)property.GetArrayElementAtIndex(i).objectReferenceValue);
            }
            return ret;
        }

        private List<int> GetSerializedListInt(SerializedProperty property)
        {
            var ret = new List<int>();
            for (var i = 0; i < property.arraySize; i++)
            {
                ret.Add(property.GetArrayElementAtIndex(i).intValue);
            }
            return ret;
        }

        private List<bool> GetSerializedListBool(SerializedProperty property)
        {
            var ret = new List<bool>();
            for (var i = 0; i < property.arraySize; i++)
            {
                ret.Add(property.GetArrayElementAtIndex(i).boolValue);
            }
            return ret;
        }

        private void SetSerializedList(SerializedProperty property, List<MonoBehaviour> list)
        {
            property.arraySize = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }
        }

        private void SetSerializedListInt(SerializedProperty property, List<int> list)
        {
            property.arraySize = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                property.GetArrayElementAtIndex(i).intValue = list[i];
            }
        }

        private void SetSerializedListBool(SerializedProperty property, List<bool> list)
        {
            property.arraySize = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                property.GetArrayElementAtIndex(i).boolValue = list[i];
            }
        }
    }
}
