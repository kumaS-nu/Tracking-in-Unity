using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace kumaS.Tracker.FaceMesh
{
    [BurstCompile]
    public struct AffineTransform : IJob
    {
        [ReadOnly]
        public NativeArray<float> Input;

        [ReadOnly]
        public NativeArray<float> Affine;

        [WriteOnly]
        public NativeArray<float> Output;

        public void Execute()
        {
            for (var i = 0; i < 3; i++)
            {
                Output[3 * i] = Affine[0] * Input[3 * i] + Affine[1] * Input[3 * i + 1] + Affine[2];
                Output[3 * i + 1] = Affine[3] * Input[3 * i] + Affine[4] * Input[3 * i + 1] + Affine[5];
                Output[3 * i + 2] = Input[3 * i + 2];
            }
        }
    }
}
