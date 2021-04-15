using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using Live2D.Cubism.Core;
using System;
using UniRx;

namespace kumaS.Tracker.Live2D
{
    /// <summary>
    /// Live2Dに適応させる。
    /// </summary>
    public class Live2DDestnation : ScheduleDestinationBase<PredictedLive2DData>
    {
        public CubismModel model;

        private Dictionary<string, CubismParameter> parameters = new Dictionary<string, CubismParameter>();

        public override string ProcessName { get; set; } = "Apply to Live2D";
        public override Type[] UseType { get; } = new Type[0];
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private void Awake()
        {
            foreach(var p in model.Parameters)
            {
                parameters[p.Id] = p;
            }
            isAvailable.Value = true;
        }

        protected override void ProcessInternal(SchedulableData<PredictedLive2DData> input)
        {
            foreach(var p in input.Data.Parameter)
            {
                if(parameters.TryGetValue(p.Key, out var parameter))
                {
                    parameter.Value = p.Value;
                }
            }
        }
    }
}
