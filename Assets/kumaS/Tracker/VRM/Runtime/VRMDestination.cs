using kumaS.Tracker.Core;

using System;
using System.Collections.Generic;
using System.Linq;

using UniRx;

using UnityEngine;

using VRM;

namespace kumaS.Tracker.VRM
{
    /// <summary>
    /// VRMに適応する。
    /// </summary>
    public sealed class VRMDestination : ScheduleDestinationBase<PredictedVRMData>
    {
        [SerializeField]
        internal List<Transform> transforms = new List<Transform>();

        [SerializeField]
        internal VRMBlendShapeProxy proxy;

        public override string ProcessName { get; set; } = "Apply to VRM";
        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly Dictionary<string, BlendShapeKey> convertTable = new Dictionary<string, BlendShapeKey>();
        private readonly Dictionary<string, Transform> innerTransforms = new Dictionary<string, Transform>();

        private void Awake()
        {
            if (proxy != null)
            {
                foreach (KeyValuePair<BlendShapeKey, float> clip in proxy.GetValues())
                {
                    convertTable[clip.Key.Name] = clip.Key;
                }
            }
            foreach (Transform t in transforms)
            {
                innerTransforms[t.name] = t;
            }

            isAvailable.Value = true;
        }

        protected override void ProcessInternal(SchedulableData<PredictedVRMData> input)
        {
            if (input.IsSuccess)
            {
                foreach (KeyValuePair<string, Vector3> pos in input.Data.Position)
                {
                    if (innerTransforms.TryGetValue(pos.Key, out Transform t))
                    {
                        t.position = pos.Value;
                    }
                }

                foreach (KeyValuePair<string, Quaternion> rot in input.Data.Rotation)
                {
                    if (innerTransforms.TryGetValue(rot.Key, out Transform t))
                    {
                        t.rotation = rot.Value;
                    }
                }

                if (proxy != null)
                {
                    IEnumerable<KeyValuePair<BlendShapeKey, float>> values = input.Data.Parameter.Where(para => convertTable.ContainsKey(para.Key)).Select(para => new KeyValuePair<BlendShapeKey, float>(convertTable[para.Key], para.Value));
                    proxy.SetValues(values);
                }
            }
        }
    }
}
