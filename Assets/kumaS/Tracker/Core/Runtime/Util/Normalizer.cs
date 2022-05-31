using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 画像を正規化。
    /// </summary>
    [BurstCompile]
    public struct Normalizer : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr Input;

        [ReadOnly]
        public int Length;

        [WriteOnly]
        public NativeArray<float> Output;

        private static readonly float coeff = 1 / 127.5f;

        public unsafe void Execute()
        {
            var data = (byte*)Input.ToPointer();

            for (var i = 0; i < Length; i++)
            {
                Output[3 * i] = data[3 * i + 2] * coeff - 1f;
                Output[3 * i + 1] = data[3 * i + 1] * coeff - 1f;
                Output[3 * i + 2] = data[3 * i] * coeff - 1f;
            }
        }
    }
}
