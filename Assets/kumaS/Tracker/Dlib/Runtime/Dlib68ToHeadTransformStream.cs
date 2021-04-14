﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kumaS.Tracker.Core;
using System;
using UniRx;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace kumaS.Tracker.Dlib
{
    /// <summary>
    /// Dlibの68つの顔の特徴点から頭の位置・回転に変換するストリーム。
    /// </summary>
    public class Dlib68ToHeadTransformStream : ScheduleStreamBase<Dlib68Landmarks, HeadTransform>
    {
        [SerializeField]
        internal float moveScale = 1;

        [SerializeField]
        internal float focalLength = 2371;

        [SerializeField]
        internal bool sourceIsMirror = false;

        [SerializeField]
        internal bool wantMirror = true;

        [SerializeField]
        internal Vector3[] realPoint = new Vector3[] {
            new Vector3(0.0f, -0.03f, 0.11f),
            new Vector3(0.0f, 0.06f, 0.08f),
            new Vector3(-0.048f, -0.07f, 0.066f),
            new Vector3(0.048f, -0.07f, 0.066f),
            new Vector3(-0.03f, 0.007f, 0.088f),
            new Vector3(0.03f, 0.007f, 0.088f),
            new Vector3(-0.015f, -0.07f, 0.08f),
            new Vector3(0.015f, -0.07f, 0.08f)
        };


        [SerializeField]
        internal bool isDebugHead = true;

        [SerializeField]
        internal int type = 0;

        [SerializeField]
        internal float fov = 78;

        [SerializeField]
        internal int width = 1920;

        [SerializeField]
        internal bool fold = false;

        private bool mirror = true;

        private Mat realPoints;
        private Mat distCoeffs = new Mat();
        private Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        private Vector2Int currentSize = Vector2Int.zero;

        // カメラと正面を向いてるから、y軸周りに180度回転させるクォータニオン。
        private Quaternion q = new Quaternion(0, 1, 0, 0);

        public override string ProcessName { get; set; } = "Dlib 68 landmarks to head transform";
        public override Type[] UseType { get; } = new Type[0];
        public override string[] DebugKey { get; } = new string[] {
            SchedulableData<object>.Elapsed_Time,
            nameof(Head_Pos_X), nameof(Head_Pos_Y), nameof(Head_Pos_Z),
            nameof(Head_Rot_X), nameof(Head_Rot_Y), nameof(Head_Rot_Z)
        };
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private readonly string Head = nameof(Head);
        private readonly string Head_Pos_X = nameof(Head_Pos_X);
        private readonly string Head_Pos_Y = nameof(Head_Pos_Y);
        private readonly string Head_Pos_Z = nameof(Head_Pos_Z);
        private readonly string Head_Rot_X = nameof(Head_Rot_X);
        private readonly string Head_Rot_Y = nameof(Head_Rot_Y);
        private readonly string Head_Rot_Z = nameof(Head_Rot_Z);

        public override void InitInternal(int thread)
        {
            var points = new Point3f[8];
            for (var i = 0; i < 8; i++)
            {
                points[i] = new Point3f(realPoint[i].x, realPoint[i].y, realPoint[i].z);
            }
            realPoints = new Mat(realPoint.Length, 1, MatType.CV_32FC3, points);
            mirror = sourceIsMirror != wantMirror;
            isAvailable.Value = true;
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<HeadTransform> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess && isDebugHead)
            {
                data.Data.ToDebugPosition(message, Head_Pos_X, Head_Pos_Y, Head_Pos_Z);
                data.Data.ToDebugRotation(message, Head_Rot_X, Head_Rot_Y, Head_Rot_Z);
            }
            return new DebugMessage(data, message);
        }

        protected override SchedulableData<HeadTransform> ProcessInternal(SchedulableData<Dlib68Landmarks> input)
        {
            if (!input.IsSuccess)
            {
                return new SchedulableData<HeadTransform>(input, default);
            }
            if (currentSize != input.Data.ImageSize)
            {
                var camera = new float[9] { focalLength, 0, (float)input.Data.ImageSize.x / 2, 0, focalLength, (float)input.Data.ImageSize.y / 2, 0, 0, 1 };
                Marshal.Copy(camera, 0, cameraMatrix.Data, 9);
            }

            var predictPoints = ExtractPoint(input.Data.Landmarks);
            var rvec = new Mat();
            var tvec = new Mat();
            Cv2.SolvePnP(realPoints, predictPoints, cameraMatrix, distCoeffs, rvec, tvec);
            var pos = GetPosition(tvec) * moveScale;
            var rot = GetRotation(rvec);
            pos.z *= -1;
            rot = q * rot;
            if (mirror)
            {
                rot.y *= -1;
                rot.z *= -1;
            }
            else
            {
                pos.x *= -1;
            }
            var ret = new HeadTransform(pos, rot);
            return new SchedulableData<HeadTransform>(input, ret);
        }

        /// <summary>
        /// PnP問題を解くために使用する点を抽出。
        /// </summary>
        /// <param name="points">68の特徴点。</param>
        private Mat ExtractPoint(DlibDotNet.Point[] points)
        {
            Point2f[] p = new Point2f[8];
            p[0] = points[30].ToVec2f();
            p[1] = points[8].ToVec2f();
            p[2] = points[45].ToVec2f();
            p[3] = points[36].ToVec2f();
            p[4] = points[54].ToVec2f();
            p[5] = points[48].ToVec2f();
            p[6] = points[42].ToVec2f();
            p[7] = points[39].ToVec2f();
            return new Mat(p.Length, 1, MatType.CV_32FC2, p);
        }

        /// <summary>
        /// 推定位置を取得。
        /// </summary>
        /// <param name="tvec">推定された位置。</param>
        /// <returns>Unity座標系のカメラ座標系での位置。</returns>
        private Vector3 GetPosition(Mat tvec)
        {
            var data = new float[3];
            Marshal.Copy(tvec.Data, data, 0, 3);
            return new Vector3(data[0], -data[1], data[2]);
        }

        /// <summary>
        /// 回転を取得。
        /// </summary>
        /// <param name="rvec">推定された回転。</param>
        /// <returns>Unity座標系のカメラからの回転。</returns>
        private Quaternion GetRotation(Mat rvec)
        {
            var data = new float[3];
            Marshal.Copy(rvec.Data, data, 0, 3);
            var log = new Vector3(data[0], data[1], data[2]);
            var rotation = log.ToQuaternion();
            rotation.x *= -1;
            rotation.z *= -1;
            return rotation;
        }

        private void OnDestroy()
        {
            realPoints.Dispose();
            distCoeffs.Dispose();
            cameraMatrix.Dispose();
        }
    }
}
