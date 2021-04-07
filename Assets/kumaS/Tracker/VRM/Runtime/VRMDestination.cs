using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using System;
using VRM;
using UniRx;
using System.Linq;

namespace kumaS.Tracker.VRM
{
    public class VRMDestination : ScheduleDestinationBase<ModelPredictedData>
    {
        [SerializeField]
        internal List<Transform> transforms = new List<Transform>();

        [SerializeField]
        internal VRMBlendShapeProxy proxy;

        public override string ProcessName { get; set; } = "Apply to VRM";
        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private Dictionary<string, BlendShapeKey> convertTable = new Dictionary<string, BlendShapeKey>();

        private void Awake()
        {
            if (proxy != null)
            {
                foreach (var clip in proxy.GetValues())
                {
                    convertTable[clip.Key.Name] = clip.Key;
                }
            }
            isAvailable.Value = true;
        }

        protected override void ProcessInternal(SchedulableData<ModelPredictedData> input)
        {
            if (input.IsSuccess)
            {
                foreach (var t in transforms)
                {
                    if (input.Data.Position.TryGetValue(t.name, out var pos))
                    {
                        t.position = pos;
                    }
                    if (input.Data.Rotation.TryGetValue(t.name, out var rot))
                    {
                        t.rotation = rot;
                    }
                }

                if (proxy != null)
                {
                    var values = input.Data.Parameter.Where(para => convertTable.ContainsKey(para.Key)).Select(para => new KeyValuePair<BlendShapeKey, float>(convertTable[para.Key], para.Value));
                    proxy.SetValues(values);
                }
            }
        }
    }
}
