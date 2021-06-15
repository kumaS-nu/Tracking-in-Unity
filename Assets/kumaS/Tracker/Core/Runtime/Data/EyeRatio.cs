namespace kumaS.Tracker.Core
{

    /// <summary>
    /// 目の縦横比のデータ。
    /// </summary>
    public class EyeRatio
    {

        public float Left { get; }
        public float Right { get; }

        public EyeRatio(float left, float right)
        {
            Left = left;
            Right = right;
        }
    }
}
