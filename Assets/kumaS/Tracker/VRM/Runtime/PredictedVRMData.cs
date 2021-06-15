using kumaS.Tracker.Core;

using System.Collections.Generic;

using UnityEngine;

namespace kumaS.Tracker.VRM
{
    /// <summary>
    /// 予測されたVRMモデル用のデータ。
    /// </summary>
    public class PredictedVRMData : PredictedModelData
    {
        /// <param name="position">位置。</param>
        /// <param name="rotation">回転。</param>
        /// <param name="parameter">ブレンドシェイプの値。</param>
        /// <param name="option">その他。</param>
        public PredictedVRMData(Dictionary<string, Vector3> position = null, Dictionary<string, Quaternion> rotation = null, Dictionary<string, float> parameter = null, Dictionary<string, object> option = null) :
            base(position, rotation, parameter, option)
        { }
    }
}
