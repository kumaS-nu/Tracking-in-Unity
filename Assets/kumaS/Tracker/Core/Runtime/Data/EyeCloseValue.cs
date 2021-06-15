namespace kumaS.Tracker.Core
{
    /// <summary>
    /// 目の閉じ具合のデータ．
    /// </summary>
    public class EyeCloseValue
    {
        public float Left { get; }
        public float Right { get; }

        public EyeCloseValue(float left, float right)
        {
            Left = left;
            Right = right;
        }
    }
}
