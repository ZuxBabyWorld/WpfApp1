
namespace WpfApp1
{
    public class Config
    {
        //通用
        public int DelayFristImageMs = 500;
        public int InternalImageMs = 200;
        public int ImageCount = 3;
        public int ImageShowMs = 300;

        //凹凸缺陷检测
        public double Sigma1 = 5, Sigma2 = 1;
        public double ThresholdRateMax = 0.5;
        public int SelectAreaMin = 1;
        public int SelectAreaMax = 99999;

        public Config Clone()
        {
            return new Config
            {
                DelayFristImageMs = this.DelayFristImageMs,
                InternalImageMs = this.InternalImageMs,
                ImageCount = this.ImageCount,
                ImageShowMs = this.ImageShowMs,

                Sigma1 = this.Sigma1,
                Sigma2 = this.Sigma2,
                ThresholdRateMax = this.ThresholdRateMax,
                SelectAreaMin = this.SelectAreaMin,
                SelectAreaMax = this.SelectAreaMax
            };
        }
    }
}
