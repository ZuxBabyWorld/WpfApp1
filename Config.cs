
namespace WpfApp1
{
    public class CommonConfig
    {
        public int DelayFristImageMs = 500;
        public int InternalImageMs = 200;
        public int ImageCount = 3;
        public int ImageShowMs = 300;
    }
    public class ArgsConfig
    {
        public double Sigma1 = 5, Sigma2 = 1;
        public double ThresholdRateMax = 0.5;
        public int SelectAreaMin = 1;
        public int SelectAreaMax = 99999;
    }
    public class Config
    {
        public CommonConfig commonConfig = new CommonConfig();
        public ArgsConfig argsConfig = new ArgsConfig();
    }
}
