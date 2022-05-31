using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCvSharp;
using System.Runtime.InteropServices;
using Unity.Jobs;
using Unity.Collections;
using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace kumaS.Tracker.Core
{
    [BurstCompile]
    public struct Color32ToMat : IJob
    {
        [ReadOnly]
        public NativeArray<Color32> Input;

        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr mat;

        [ReadOnly]
        public int height_;

        [ReadOnly]
        public int width_;

        public unsafe void Execute()
        {
            var data = (byte*)mat.ToPointer();
            for (var h = 0; h < height_; h++)
            {
                for (var w = 0; w < width_; w++)
                {
                    var colorIndex = h * width_ + width_ - w - 1;
                    var byteIndex = 3 * (h * width_ + w);
                    data[byteIndex] = Input[colorIndex].b;
                    data[byteIndex + 1] = Input[colorIndex].g;
                    data[byteIndex + 2] = Input[colorIndex].r;
                }
            }
        }
    }

    [BurstCompile]
    public struct MatToColor32 : IJob
    {
        [WriteOnly]
        public NativeArray<Color32> Output;

        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public IntPtr mat;

        [ReadOnly]
        public int height_;

        [ReadOnly]
        public int width_;

        public unsafe void Execute()
        {
            var data = (byte*)mat.ToPointer();
            for (var h = 0; h < height_; h++)
            {
                for (var w = 0; w < width_; w++)
                {
                    var colorIndex = h * width_ + width_ - w - 1;
                    var byteIndex = 3 * (h * width_ + w);
                    Output[colorIndex] = new Color32(data[byteIndex + 2], data[byteIndex + 1], data[byteIndex], 1);
                }
            }
        }
    }
}
