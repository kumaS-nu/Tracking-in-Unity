using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 画像を正規化。
    /// </summary>
    [BurstCompile]
    public struct Normalizer : IJob
    {
        [ReadOnly]
        public NativeArray<byte> Input;

        [WriteOnly]
        public NativeArray<float> Output;

        private static readonly float coeff = 1 / 127.5f;

        public void Execute()
        {
            for (var i = 0; i < Input.Length / 3; i++)
            {
                Output[i * 3] = Input[i * 3 + 2] * coeff - 0.5f;
                Output[i * 3 + 1] = Input[i * 3 + 1] * coeff - 0.5f;
                Output[i * 3 + 2] = Input[i * 3] * coeff - 0.5f;
            }
        }
    }
}
