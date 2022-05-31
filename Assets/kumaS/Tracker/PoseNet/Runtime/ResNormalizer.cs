
using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace kumaS.Tracker.PoseNet
{
    [BurstCompile]
    public class ResNormalizer : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr Input;

        [ReadOnly]
        public int Length;

        [WriteOnly]
        public NativeArray<float> Output;


        public unsafe void Execute()
        {
            var data = (byte*)Input.ToPointer();

            for (var i = 0; i < Length; i++)
            {
                Output[3 * i] = data[3 * i + 2] - 123.15f;
                Output[3 * i + 1] = data[3 * i + 1] - 115.90f;
                Output[3 * i + 2] = data[3 * i] - 103.06f;
            }
        }
    }
}
