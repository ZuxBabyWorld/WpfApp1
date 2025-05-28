using System.IO;
using System.Windows.Input;
using System.Xml;
using HalconDotNet;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace WpfApp1
{
    public class AlgorithmConfig
    {
        //凹凸缺陷检测
        public double Sigma1 = 5, Sigma2 = 1;
        public double ThresholdRateMax = 0.5;
        public int SelectAreaMin = 1;
        public int SelectAreaMax = 99999;
    }

    public class ArgsPageViewModel : ViewModelBase
    {
        private string _configFilePath = "AlgorithmConfig.json";
        private AlgorithmConfig _config;
        public AlgorithmConfig Config { get { return _config; } }
        private BumpDefectProcessor _bumpProcessor;

        public double Sigma1 { get { return _config.Sigma1; } set { _config.Sigma1 = value; NotifyPropertyChanged("Sigma1"); } }
        public double Sigma2 { get { return _config.Sigma2; } set { _config.Sigma2 = value; NotifyPropertyChanged("Sigma2"); } }
        public double ThresholdRateMax { get { return _config.ThresholdRateMax; } set { _config.ThresholdRateMax = value; NotifyPropertyChanged("ThresholdRateMax"); } }
        public int SelectAreaMin { get { return _config.SelectAreaMin; } set { _config.SelectAreaMin = value; NotifyPropertyChanged("SelectAreaMin"); } }
        public int SelectAreaMax { get { return _config.SelectAreaMax; } set { _config.SelectAreaMax = value; NotifyPropertyChanged("SelectAreaMax"); } }

        public ICommand ResetCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }

        public ArgsPageViewModel()
        {
            LoadConfig();
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
        }

        private void LoadConfig(bool reset = false)
        {
            if (reset || !File.Exists(_configFilePath))
            {
                _config = new AlgorithmConfig();
            }
            else
            {
                string json = File.ReadAllText(_configFilePath);
                _config = JsonConvert.DeserializeObject<AlgorithmConfig>(json);
            }
        }

        private void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
        }

        private void Reset(object obj)
        {
            LoadConfig(true);
            NotifyPropertyChanged("Sigma1");
            NotifyPropertyChanged("Sigma2");
            NotifyPropertyChanged("ThresholdRateMax");
            NotifyPropertyChanged("SelectAreaMin");
            NotifyPropertyChanged("SelectAreaMax");
        }

        private void Apply(object obj)
        {
            SaveConfig();
            // 在这里调用影响当前算法的方法
            ApplyAlgorithmParameters();
        }

        private void ApplyAlgorithmParameters()
        {
            _bumpProcessor.SetParametersByConfig(_config);
        }

        public void SetBumpProcessor(BumpDefectProcessor processor)
        {
            _bumpProcessor = processor;
            ApplyAlgorithmParameters();
        }
    }
}