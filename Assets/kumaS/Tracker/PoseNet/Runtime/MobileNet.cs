
using kumaS.Tracker.Core;

using OpenCvSharp;

using Unity.Barracuda;
using Unity.Collections;

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
                Input = new NativeArray<byte>(inputSize.Width * inputSize.Height * 3, Allocator.Persistent),
                Output = new NativeArray<float>(inputSize.Width * inputSize.Height * 3, Allocator.Persistent)
            };
            outputStride = stride;
            quantHeight = inputSize.Height / stride;
            quantWidth = inputSize.Width / stride;
            quantAmount = quantHeight * quantWidth;
            flag = interpolation;
            minScore = score * 17;
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
            data = new byte[inputSize.Width * inputSize.Height * 3];
        }

        public override void Dispose()
        {
            normalizer.Input.Dispose();
            normalizer.Output.Dispose();
        }

        protected override void ExecuteInternal()
        {
            normalizer.Input.CopyFrom(data);
            normalizer.Execute();
            var input = new Tensor(1, inputSize.Height, inputSize.Width, 3, normalizer.Output.ToArray());
            worker.Execute(input);
            heatmap = worker.PeekOutput("MobilenetV1/heatmap_2/BiasAdd").ToReadOnlyArray();
            offsets = worker.PeekOutput("MobilenetV1/offset_2/BiasAdd").ToReadOnlyArray();
        }
    }
}
