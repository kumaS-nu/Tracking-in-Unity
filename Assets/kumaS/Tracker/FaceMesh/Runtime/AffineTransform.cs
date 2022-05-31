using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace kumaS.Tracker.FaceMesh
{
    [BurstCompile]
    public struct AffineTransform : IJob
    {
        [ReadOnly]
        public NativeArray<float> Input;

        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr Affine;

        [WriteOnly]
        public NativeArray<float> Output;

        public unsafe void Execute()
        {
            var affine = (float*)Affine.ToPointer();
            for (var i = 0; i < 3; i++)
            {
                Output[3 * i] = affine[0] * Input[3 * i] + affine[1] * Input[3 * i + 1] + affine[2];
                Output[3 * i + 1] = affine[3] * Input[3 * i] + affine[4] * Input[3 * i + 1] + affine[5];
                Output[3 * i + 2] = Input[3 * i + 2];
            }
        }
    }
}
