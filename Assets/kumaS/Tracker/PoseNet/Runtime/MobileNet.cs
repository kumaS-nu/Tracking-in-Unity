﻿
using kumaS.Tracker.Core;

using OpenCvSharp;

using Unity.Barracuda;
using Unity.Collections;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    internal sealed class MobileNet : PoseNetBase
    {
        private Normalizer normalizer;

        internal MobileNet(Model model, int stride, InterpolationFlags interpolation, float score, Size input = default)
        {
            if (input != default)
            {
                inputSize = input;
            }
            normalizer = new Normalizer
            {
                Length = inputSize.Width * inputSize.Height,
                Output = new NativeArray<float>(inputSize.Width * inputSize.Height * 3, Allocator.Persistent)
            };
            outputStride = stride;
            flag = interpolation;
            minScore = score * 17;
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
        }

        public override void DisposeInternal()
        {
            normalizer.Output.Dispose();
        }

        protected override (Tensor heatmap, Tensor offsets) ExecuteInternal()
        {
            normalizer.Input = data.Data;
            normalizer.Execute();
            var input = new Tensor(1, inputSize.Height, inputSize.Width, 3, normalizer.Output.ToArray());
            worker.Execute(input);
            input.Dispose();
            return (worker.PeekOutput("MobilenetV1/heatmap_2/BiasAdd"), worker.PeekOutput("MobilenetV1/offset_2/BiasAdd"));
        }
    }
}
