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
    public class VRMDestination : ScheduleDestinationBase<PredictedModelData>
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
        private Dictionary<string, Transform> innerTransforms = new Dictionary<string, Transform>();

        private void Awake()
        {
            if (proxy != null)
            {
                foreach (var clip in proxy.GetValues())
                {
                    convertTable[clip.Key.Name] = clip.Key;
                }
            }
            foreach(var t in transforms)
            {
                innerTransforms[t.name] = t;
            }

            isAvailable.Value = true;
        }

        protected override void ProcessInternal(SchedulableData<PredictedModelData> input)
        {
            if (input.IsSuccess)
            {
                foreach(var pos in input.Data.Position)
                {
                    if(innerTransforms.TryGetValue(pos.Key, out var t))
                    {
                        t.position = pos.Value;
                    }
                }

                foreach (var rot in input.Data.Rotation)
                {
                    if (innerTransforms.TryGetValue(rot.Key, out var t))
                    {
                        t.rotation = rot.Value;
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
