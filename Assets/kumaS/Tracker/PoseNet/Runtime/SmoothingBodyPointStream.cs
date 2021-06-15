using kumaS.Tracker.Core;

using System.Collections.Generic;

using UniRx;

using UnityEngine;

namespace kumaS.Tracker.PoseNet
{
    /// <summary>
    /// 体の点の動きを滑らかにする。
    /// </summary>
    public sealed class SmoothingBodyPointStream : SmoothingStreamBase<BodyPoints>
    {
        [SerializeField]
        internal bool fold2 = false;

        [SerializeField]
        internal float[] rotationRange = new float[15];

        [SerializeField]
        internal bool fold3 = false;

        [SerializeField]
        internal float[] rotationZRange = new float[15];

        [SerializeField]
        internal bool fold4 = false;

        [SerializeField]
        internal float[] rotationSpeedLimit = new float[15];

        [SerializeField]
        internal bool fold0 = false;

        [SerializeField]
        internal float xMax = 3;

        [SerializeField]
        internal float xMin = -3;

        [SerializeField]
        internal float yMax = 3;

        [SerializeField]
        internal float yMin = -3;

        [SerializeField]
        internal float zMax = 3;

        [SerializeField]
        internal float zMin = -3;

        [SerializeField]
        internal bool fold1 = false;

        [SerializeField]
        internal float speedLimit;

        [SerializeField]
        internal bool isDebugPosition = true;

        [SerializeField]
        internal bool isDebugRotation = true;

        public override string ProcessName { get; set; } = "Smoothing body points";
        public override string[] DebugKey { get => debugKey; }

        private string[] debugKey;
        public override IReadOnlyReactiveProperty<bool> IsAvailable { get => isAvailable; }

        private readonly ReactiveProperty<bool> isAvailable = new ReactiveProperty<bool>(false);

        private string[] positionX;
        private string[] positionY;
        private string[] positionZ;
        private string[] rotationX;
        private string[] rotationY;
        private string[] rotationZ;

        public static readonly int[] useIndex = new int[] { 0, 1, 2, 3, 4, 5, 8, 9, 10, 11, 14 };

        private float sqrSpeedLimit;
        private readonly float[] rotationRangeInternal = new float[15];
        private readonly float[] rotationZRangeNegative = new float[15];

        private void Awake()
        {
            var dkey = new List<string>();
            var px = new List<string>();
            var py = new List<string>();
            var pz = new List<string>();
            dkey.Add(SchedulableData<object>.Elapsed_Time);
            for (var i = 0; i < 14; i++)
            {
                var part = PredictedModelData.DefaultPositionList[i];
                dkey.Add(part + "_Pos_X");
                px.Add(part + "_Pos_X");
                dkey.Add(part + "_Pos_Y");
                py.Add(part + "_Pos_Y");
                dkey.Add(part + "_Pos_Z");
                pz.Add(part + "Pos_Z");
            }
            positionX = px.ToArray();
            positionY = py.ToArray();
            positionZ = pz.ToArray();

            var rx = new List<string>();
            var ry = new List<string>();
            var rz = new List<string>();
            for (var i = 0; i < 15; i++)
            {
                var part = PredictedModelData.DefaultRotationList[i];
                dkey.Add(part + "_Rot_X");
                rx.Add(part + "_Rot_X");
                dkey.Add(part + "_Rot_Y");
                ry.Add(part + "_Rot_Y");
                dkey.Add(part + "_Rot_Z");
                rz.Add(part + "_Rot_Z");
            }
            rotationX = rx.ToArray();
            rotationY = ry.ToArray();
            rotationZ = rz.ToArray();

            debugKey = dkey.ToArray();

            sqrSpeedLimit = speedLimit * speedLimit;
            for (var i = 0; i < rotationRange.Length; i++)
            {
                rotationRangeInternal[i] = Mathf.Cos(rotationRange[i]);
                rotationZRangeNegative[i] = 360.0f - rotationZRange[i];
            }
            isAvailable.Value = true;
        }

        protected override BodyPoints Average(BodyPoints[] datas)
        {
            var position = new Vector3[14];
            var logLotation = new Vector3[15];
            var average = new Quaternion[15];
            var inverse = new Quaternion[15];

            for (var i = 0; i < position.Length; i++)
            {
                position[i] = Vector3.zero;
            }
            for (var i = 0; i < logLotation.Length; i++)
            {
                logLotation[i] = Vector3.zero;
            }

            if (lastOutput == default)
            {
                average = datas[0].Rotation;
            }
            else
            {
                average = lastOutput.Rotation;
            }

            for (var i = 0; i < average.Length; i++)
            {
                inverse[i] = Quaternion.Inverse(average[i]);
            }

            foreach (BodyPoints data in datas)
            {
                for (var i = 0; i < position.Length; i++)
                {
                    position[i] += data.Position[i];
                }

                for (var i = 0; i < logLotation.Length; i++)
                {
                    logLotation[i] += (inverse[i] * data.Rotation[i]).ToLogQuaternion();
                }

            }

            for (var i = 0; i < position.Length; i++)
            {
                position[i] /= datas.Length;
            }

            var rot = new Quaternion[15];
            for (var i = 0; i < logLotation.Length; i++)
            {
                logLotation[i] /= datas.Length;
                rot[i] = average[i] * logLotation[i].ToQuaternion();
            }

            return new BodyPoints(position, rot);
        }

        protected override IDebugMessage DebugLogInternal(SchedulableData<BodyPoints> data)
        {
            var message = new Dictionary<string, string>();
            data.ToDebugElapsedTime(message);
            if (data.IsSuccess)
            {
                if (isDebugPosition)
                {
                    for (var i = 0; i < 14; i++)
                    {
                        data.Data.ToDebugPosition(message, i, positionX[i], positionY[i], positionZ[i]);
                    }
                }

                if (isDebugRotation)
                {
                    for (var i = 0; i < 15; i++)
                    {
                        data.Data.ToDebugRotation(message, i, rotationX[i], rotationY[i], rotationZ[i]);
                    }
                }
            }
            return new DebugMessage(data, message);
        }

        private readonly Quaternion leftSholder = Quaternion.Inverse(Quaternion.Euler(0, -45, -90));
        private readonly Quaternion rightSholder = Quaternion.Inverse(Quaternion.Euler(0, 45, 90));
        private readonly Quaternion leftElbow = Quaternion.Euler(90, 0, 0);
        private readonly Quaternion rightElbow = Quaternion.Euler(-90, 0, 0);
        private readonly Quaternion hip = Quaternion.Inverse(Quaternion.Euler(90, 0, 0));
        private readonly Quaternion knee = Quaternion.Euler(90, 180, 0);
        private readonly Quaternion head = Quaternion.Inverse(Quaternion.Euler(-90, 0, 180));

        protected override bool ValidateData(BodyPoints input)
        {
            if (input.Position[0].x < xMin || input.Position[0].x > xMax)
            {
                return false;
            }

            if (input.Position[0].y < yMin || input.Position[0].y > yMax)
            {
                return false;
            }

            if (input.Position[0].z < zMin || input.Position[0].z > zMin)
            {
                return false;
            }

            if (lastOutput != default && (input.Position[0] - lastOutput.Position[0]).sqrMagnitude > sqrSpeedLimit)
            {
                return false;
            }

            if (RotationCheck(input.Rotation[0], lastOutput.Rotation[0], 0))
            {
                return false;
            }

            var inverse0 = Quaternion.Inverse(input.Rotation[0]);
            var inverse1 = Quaternion.Inverse(lastOutput.Rotation[0]);

            if (RotationCheck(inverse0 * input.Rotation[1], inverse1 * lastOutput.Rotation[1], 1))
            {
                return false;
            }

            if (RotationCheck(inverse0 * input.Rotation[2] * leftSholder, inverse1 * lastOutput.Rotation[2] * leftSholder, 2))
            {
                return false;
            }

            if (RotationCheck(inverse0 * input.Rotation[3] * rightSholder, inverse1 * lastOutput.Rotation[3] * rightSholder, 3))
            {
                return false;
            }

            if (RotationCheck(input.Rotation[4] * Quaternion.Inverse(input.Rotation[2] * leftElbow), lastOutput.Rotation[4] * Quaternion.Inverse(lastOutput.Rotation[2] * leftElbow), 4))
            {
                return false;
            }

            if (RotationCheck(input.Rotation[5] * Quaternion.Inverse(input.Rotation[3] * rightElbow), lastOutput.Rotation[5] * Quaternion.Inverse(lastOutput.Rotation[3] * rightElbow), 5))
            {
                return false;
            }

            if (RotationCheck(inverse0 * input.Rotation[8] * hip, inverse1 * lastOutput.Rotation[8] * hip, 8))
            {
                return false;
            }

            if (RotationCheck(inverse0 * input.Rotation[9] * hip, inverse1 * lastOutput.Rotation[9] * hip, 9))
            {
                return false;
            }

            if (RotationCheck(input.Rotation[10] * Quaternion.Inverse(input.Rotation[8] * knee), lastOutput.Rotation[10] * Quaternion.Inverse(lastOutput.Rotation[8] * knee), 10))
            {
                return false;
            }

            if (RotationCheck(input.Rotation[11] * Quaternion.Inverse(input.Rotation[9] * knee), lastOutput.Rotation[11] * Quaternion.Inverse(lastOutput.Rotation[9] * knee), 11))
            {
                return false;
            }

            if (RotationCheck(inverse0 * input.Rotation[14] * head, inverse1 * lastOutput.Rotation[14] * head, 14))
            {
                return false;
            }

            return true;

        }

        private bool RotationCheck(Quaternion target, Quaternion reference, int index)
        {
            Vector3 localForward = target * Vector3.forward;
            if (localForward.y < rotationRangeInternal[index])
            {
                return false;
            }

            var z = target.eulerAngles.z;
            if (z > rotationZRange[index] && z < rotationZRangeNegative[index])
            {
                return false;
            }

            if (Vector3.Angle(localForward, reference * Vector3.forward) > rotationSpeedLimit[index])
            {
                return false;
            }

            return true;
        }
    }
}
