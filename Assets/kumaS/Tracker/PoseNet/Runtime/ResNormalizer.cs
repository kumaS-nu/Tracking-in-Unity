
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace kumaS.Tracker.PoseNet
{
    [BurstCompile]
    public class ResNormalizer : IJob
    {
        [ReadOnly]
        public NativeArray<byte> Input;

        [WriteOnly]
        public NativeArray<float> Output;


        public void Execute()
        {
            for (var i = 0; i < Input.Length / 3; i++)
            {
                Output[i * 3] = Input[i * 3 + 2] - 123.15f;
                Output[i * 3 + 1] = Input[i * 3 + 1] - 115.90f;
                Output[i * 3 + 2] = Input[i * 3] - 103.06f;
            }
        }
    }
}
