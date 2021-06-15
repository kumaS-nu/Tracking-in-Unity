using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{

    /// <summary>
    /// PoseNetのやつを3d座標に変換。
    /// </summary>
    [BurstCompile]
    public struct TransformTo3d : IJob, IDisposable
    {
        /// <value="Real">現実の体の各点の距離。</value>
        [ReadOnly]
        public NativeArray<float> Real;

        /// <value="Avater">アバターの体の各点の距離。</value>
        [ReadOnly]
        public NativeArray<float> Avatar;

        /// <value="pixcelCenter">画像の中心のピクセル。</value>
        [ReadOnly]
        public Vector2Int pixcelCenter;

        /// <value="Input">PoseNetで取得した値。</value>
        [ReadOnly]
        public NativeArray<Vector2> Input;

        /// <value="Position">推定された3d位置。</value>
        [WriteOnly]
        public NativeArray<Vector3> Position;

        /// <value="Rotation">推定された回転。</value>
        [WriteOnly]
        public NativeArray<Quaternion> Rotation;

        /// <value="zOffset">zのオフセット。オフセットなしだと、カメラのある点が原点。</value>
        [ReadOnly]
        private readonly float zOffset;

        /// <value="focalLength">焦点距離（pixcel）。</value>
        [ReadOnly]
        private readonly float focalLength;

        /// <value="sqrFocalLength">焦点距離の二乗。</value>
        [ReadOnly]
        private readonly float sqrFocalLength;

        /// <value="returnFocalLength">焦点距離の逆数。</value>
        [ReadOnly]
        private readonly float returnFocalLength;

        /// <value="mirror">ミラーにするか。</value>
        [ReadOnly]
        private readonly bool mirror;

        /// <value="Centered">中心を原点にした座標。</value>
        private NativeArray<Vector2> Centered;

        /// <value="Real3d">現実の3dの値。</value>
        private NativeArray<Vector3> Real3d;


        public TransformTo3d(float zOffset, float focalLength, bool isMirror)
        {
            Real = new NativeArray<float>(7, Allocator.Persistent);
            Avatar = new NativeArray<float>(7, Allocator.Persistent);
            this.zOffset = zOffset;
            this.focalLength = focalLength;
            sqrFocalLength = focalLength * focalLength;
            returnFocalLength = 1 / focalLength;
            mirror = isMirror;
            pixcelCenter = default;
            Input = new NativeArray<Vector2>(17, Allocator.Persistent);
            Centered = new NativeArray<Vector2>(17, Allocator.Persistent);
            Real3d = new NativeArray<Vector3>(14, Allocator.Persistent);
            Position = new NativeArray<Vector3>(14, Allocator.Persistent);
            Rotation = new NativeArray<Quaternion>(15, Allocator.Persistent);
        }

        public void Dispose()
        {
            Real.Dispose();
            Avatar.Dispose();
            Input.Dispose();
            Centered.Dispose();
            Real3d.Dispose();
            Position.Dispose();
            Rotation.Dispose();
        }

        public void Execute()
        {
            // 最初に、画像の中心を原点にする。
            for (var i = 0; i < Input.Length; i++)
            {
                Centered[i] -= pixcelCenter;
            }

            // 左肩・腰のz座標を推定。
            var z0 = focalLength * Real[4] / (Centered[5] - Centered[11]).magnitude;
            SetReal3d(2, 5, z0);
            SetReal3d(8, 11, z0);

            // 右肩・腰のz座標を推定。
            var z1 = focalLength * Real[4] / (Centered[6] - Centered[12]).magnitude;
            SetReal3d(3, 6, z1);
            SetReal3d(9, 12, z1);

            var rot0 = Mathf.Atan2(Real3d[3].z - Real3d[2].z, Real3d[3].x - Real3d[2].x) * Mathf.Rad2Deg;
            var forward = Quaternion.Euler(0, rot0, 0);

            // 左肘を推定
            Vector3 leftElbow = forward * new Vector3(-1, 0, 1);
            SetReal3d(2, 5, 4, 7, 2, leftElbow);

            // 右肘を推定
            Vector3 rightElbow = forward * new Vector3(1, 0, 1);
            SetReal3d(3, 6, 5, 8, 2, rightElbow);

            // 左手首を推定
            Vector3 leftWrist = forward * Quaternion.FromToRotation(forward * Vector3.right, Real3d[4] - Real3d[2]) * Vector3.forward;
            SetReal3d(4, 7, 6, 9, 3, leftWrist);

            // 右手首を推定
            Vector3 rightWrist = forward * Quaternion.FromToRotation(forward * Vector3.left, Real3d[5] - Real3d[3]) * Vector3.forward;
            SetReal3d(5, 8, 7, 10, 3, rightWrist);

            // 左膝を推定
            Vector3 knee = forward * Vector3.forward;
            SetReal3d(8, 11, 10, 13, 5, knee);

            // 右膝を推定
            SetReal3d(9, 12, 11, 14, 5, knee);

            // 左足首を推定
            Vector3 leftAnkle = forward * Quaternion.FromToRotation(forward * Vector3.down, Real3d[10] - Real3d[8]) * Vector3.back;
            SetReal3d(10, 13, 12, 15, 6, leftAnkle);

            // 右足首を推定
            Vector3 rightAnkle = forward * Quaternion.FromToRotation(forward * Vector3.down, Real3d[11] - Real3d[9]) * Vector3.back;
            SetReal3d(11, 14, 13, 16, 6, rightAnkle);

            // 頭を推定
            Vector3 head = forward * Vector3.up;
            SetReal3d(2, 3, 5, 6, 1, 0, 0, head);

            // 中心を推定
            Vector3 center = (Real3d[2] + Real3d[3] + Real3d[8] + Real3d[9]) / 4;
            center.y = 0;
            Real3d[0] = center;

            // カメラ座標から変換。
            for (var i = 0; i < Real3d.Length; i++)
            {
                Real3d[i] = new Vector3(-Real3d[i].x, -Real3d[i].y, zOffset - Real3d[i].z);
            }

            var rot1 = Mathf.Atan2(Real3d[2].z - Real3d[3].z, Real3d[3].x - Real3d[2].x) * Mathf.Rad2Deg;

            // ルートと頭の回転計算
            Rotation[0] = Quaternion.Euler(0, rot1, 0);
            Rotation[1] = Quaternion.Euler(0, rot1, Mathf.Atan2(Centered[1].y - Centered[2].y, Centered[1].x - Centered[2].x));
            Rotation[14] = Rotation[0] * Quaternion.FromToRotation(Vector3.up, Real3d[1] - (Real3d[2] + Real3d[3]) / 2) * Quaternion.Euler(-90, 0, 180);

            // 左肩の回転計算
            var leftSholder = new Quaternion(0.5f, -0.5f, -0.5f, 0.5f);
            Rotation[2] = Quaternion.FromToRotation(Vector3.left, Real3d[4] - Real3d[2]) * leftSholder;

            // 右肩の回転計算
            var rightSholder = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
            Rotation[3] = Quaternion.FromToRotation(Vector3.right, Real3d[5] - Real3d[3]) * rightSholder;

            // 左肘の回転計算
            Rotation[4] = GetElbowRotation(Real3d[6] - Real3d[4], Real3d[2] - Real3d[4]);

            // 右肘の回転計算
            Rotation[5] = GetElbowRotation(Real3d[7] - Real3d[5], Real3d[3] - Real3d[5]);

            // 手首の回転を代入
            Rotation[6] = Rotation[4];
            Rotation[7] = Rotation[5];

            // 股関節の回転計算
            var hip = Quaternion.Euler(90, 0, 0);
            Rotation[8] = Quaternion.FromToRotation(Vector3.down, Real3d[10] - Real3d[8]) * hip;
            Rotation[9] = Quaternion.FromToRotation(Vector3.down, Real3d[11] - Real3d[9]) * hip;

            // 左膝の回転計算。
            Rotation[10] = GetKneeRotation(Real3d[12] - Real3d[10], Real3d[8] - Real3d[10]);

            // 右膝の回転計算。
            Rotation[11] = GetKneeRotation(Real3d[13] - Real3d[11], Real3d[9] - Real3d[11]);

            // 左足首の回転計算。
            Vector3 leftAnkleRot = 2 * Real3d[10] - Real3d[12] - Real3d[8];
            Rotation[12] = Quaternion.Euler(0, Mathf.Atan2(leftAnkleRot.x, leftAnkleRot.z) * Mathf.Rad2Deg, 0);

            // 右足首の回転計算。
            Vector3 rightAnkleRot = 2 * Real3d[11] - Real3d[13] - Real3d[9];
            Rotation[13] = Quaternion.Euler(0, Mathf.Atan2(rightAnkleRot.x, rightAnkleRot.z) * Mathf.Rad2Deg, 0);


            // ミラー処理
            if (mirror)
            {
                for (var i = 0; i < Rotation.Length; i++)
                {
                    Rotation[i] = new Quaternion(Rotation[i].x, -Rotation[i].y, -Rotation[i].z, Rotation[i].w);
                }
                Real3d[0] = new Vector3(-Real3d[0].x, 0, Real3d[0].z);
                Swap(2, 3);
                Swap(4, 5);
                Swap(6, 7);
                Swap(8, 9);
                Swap(10, 11);
                Swap(12, 13);
            }

            // 中心位置
            Position[0] = Real3d[0];

            var halfSholderToHip = Avatar[4] / 2;
            var halfSholderToSholder = Avatar[1] / 2;

            Vector3 hLine = Real3d[3] - Real3d[2];
            var hMag = hLine.magnitude;
            var hDy = halfSholderToSholder * hLine.y / hMag;
            var hDxz = halfSholderToSholder * (hLine.x * hLine.x + hLine.z + hLine.z) / hMag;

            Vector3 vLine = Real3d[2] - Real3d[8];
            var vMag = vLine.magnitude;
            var vDy = halfSholderToHip * vLine.y / vMag;
            var vDxz = halfSholderToHip * (vLine.x * vLine.x + vLine.z * vLine.z) / vMag;

            // 肩の位置
            Vector3 sholderOffset = Rotation[0] * Vector3.right;
            Position[2] = new Vector3(Position[0].x + sholderOffset.x * (-hDxz + vDxz), -hDy + vDy, Position[0].z + sholderOffset.z * (-hDxz + vDxz));
            Position[3] = new Vector3(Position[0].x + sholderOffset.x * (hDxz + vDxz), hDy + vDy, Position[0].z + sholderOffset.z * (hDxz + vDxz));

            // 腰の位置
            Position[8] = new Vector3(Position[0].x + sholderOffset.x * (-hDxz - vDxz), -hDy - vDy, Position[0].z + sholderOffset.z * (-hDxz - vDxz));
            Position[9] = new Vector3(Position[0].x + sholderOffset.x * (hDxz - vDxz), hDy - vDy, Position[0].z + sholderOffset.z * (hDxz - vDxz));

            // 左肘の位置
            Position[4] = Position[2] + Rotation[2] * Vector3.forward * Avatar[2];

            // 右肘の位置
            Position[5] = Position[3] + Rotation[3] * Vector3.forward * Avatar[2];

            // 左手首の位置
            Position[6] = Position[4] + Rotation[4] * Vector3.forward * Avatar[3];

            // 右手首の位置
            Position[7] = Position[5] + Rotation[5] * Vector3.forward * Avatar[3];

            // 左膝の位置
            Position[10] = Position[8] + Rotation[8] * Vector3.forward * Avatar[5];

            // 右膝の位置
            Position[11] = Position[9] + Rotation[9] * Vector3.forward * Avatar[5];

            // 左足首の位置
            Position[12] = Position[10] + Rotation[10] * Vector3.forward * Avatar[6];

            // 右足首の位置
            Position[13] = Position[11] + Rotation[11] * Vector3.forward * Avatar[6];

            // 頭の位置
            Position[1] = (Position[2] + Position[3]) / 2 + Rotation[14] * Vector3.forward * Avatar[0];
        }

        /// <summary>
        /// 与えられたz座標を基にカメラ座標から3d座標に変換。
        /// </summary>
        /// <param name="index3d">対象の3dでのインデックス。</param>
        /// <param name="index2d">対象の3dでのインデックス。</param>
        /// <param name="z">カメラ座標でのzの値。</param>
        private void SetReal3d(int index3d, int index2d, float z)
        {
            var x = z * Centered[index2d].x * returnFocalLength;
            var y = z * Centered[index2d].y * returnFocalLength;
            Real3d[index3d] = new Vector3(x, y, z);
        }

        /// <summary>
        /// カメラ座標から3d座標に変換。
        /// </summary>
        /// <param name="rootIndex3d">座標が決まっている方の3dのインデックス。</param>
        /// <param name="rootIndex2d">座標が決まっている方の2dのインデックス。</param>
        /// <param name="estimateIndex3d">推定したい方の3dのインデックス。</param>
        /// <param name="estimateIndex2d">推定したい方の2dのインデックス。</param>
        /// <param name="distanceIndex">距離のインデックス。</param>
        /// <param name="guide">方向のガイドとなるインデックス。</param>
        private void SetReal3d(int rootIndex3d, int rootIndex2d, int estimateIndex3d, int estimateIndex2d, int distanceIndex, Vector3 guide)
        {
            SetReal3dInternal(Real3d[rootIndex3d].z, Centered[rootIndex2d], estimateIndex3d, estimateIndex2d, distanceIndex, guide);
        }

        private void SetReal3d(int rootIndex3d1, int rootIndex3d2, int rootIndex2d1, int rootIndex2d2, int estimateIndex3d, int estimateIndex2d, int distanceIndex, Vector3 guide)
        {
            var z = (Real3d[rootIndex3d1].z + Real3d[rootIndex3d2].z) / 2;
            Vector2 v = (Centered[rootIndex2d1] + Centered[rootIndex2d2]) / 2;
            SetReal3dInternal(z, v, estimateIndex3d, estimateIndex2d, distanceIndex, guide);
        }

        private void SetReal3dInternal(float root3dZ, Vector2 root2d, int estimateIndex3d, int estimateIndex2d, int distanceIndex, Vector3 guide)
        {
            // 二次方程式のパラメーター
            var a = Centered[estimateIndex2d].sqrMagnitude + sqrFocalLength;
            var b = root3dZ * (Vector2.Dot(Centered[estimateIndex2d], root2d) + sqrFocalLength);
            var c = root3dZ * root3dZ * (root2d.sqrMagnitude + sqrFocalLength) - sqrFocalLength * Real[distanceIndex] * Real[distanceIndex];

            var zCenter = b / a;

            // 虚数解のとき
            if (b * b < a * c)
            {
                SetReal3d(estimateIndex3d, estimateIndex2d, zCenter);
                return;
            }

            // 目印を基にどっちか決める
            if (root3dZ + guide.normalized.z * Real[distanceIndex] > zCenter)
            {
                SetReal3d(estimateIndex3d, estimateIndex2d, zCenter + Mathf.Sqrt(b * b - a * c) / a);
            }
            else
            {
                SetReal3d(estimateIndex3d, estimateIndex2d, zCenter - Mathf.Sqrt(b * b - a * c) / a);
            }
        }

        /// <summary>
        /// 肘の回転取得。
        /// </summary>
        /// <param name="toWrist">手首までのベクトル。</param>
        /// <param name="toSholder">肩までのベクトル。</param>
        /// <returns>回転。</returns>
        private Quaternion GetElbowRotation(Vector3 toWrist, Vector3 toSholder)
        {
            Vector3 w = toWrist.normalized;
            Vector3 s = toSholder.normalized;
            var dot = Vector3.Dot(w, s);
            Vector3 y = (dot * w + s) / (dot - 1);
            return Quaternion.LookRotation(w, y);
        }

        /// <summary>
        /// 膝の回転取得。
        /// </summary>
        /// <param name="toAnkle">足首までのベクトル。</param>
        /// <param name="tohip">股関節までのベクトル。</param>
        /// <returns>回転。</returns>
        private Quaternion GetKneeRotation(Vector3 toAnkle, Vector3 tohip)
        {
            Vector3 a = toAnkle.normalized;
            Vector3 h = tohip.normalized;
            var dot = Vector3.Dot(a, h);
            Vector3 y = (dot * a + h) / (1 - dot);
            return Quaternion.LookRotation(a, y);
        }

        /// <summary>
        /// 回転を入れ替え。
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        private void Swap(int index1, int index2)
        {
            Quaternion tmp = Rotation[index1];
            Rotation[index1] = Rotation[index2];
            Rotation[index2] = tmp;
        }
    }
}
