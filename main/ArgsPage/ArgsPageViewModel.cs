using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using HalconDotNet;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

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

    public class ArgsPageViewModel : ViewModelBase
    {
        private string _configFilePath = "Config.json";
        private Config _tempConfig;
        private Config _sureConfig;


        public int DelayFristImageMs { get { return _tempConfig.DelayFristImageMs; } set { _tempConfig.DelayFristImageMs = value; NotifyPropertyChanged("Sigma1"); } }
        public int InternalImageMs { get { return _tempConfig.InternalImageMs; } set { _tempConfig.InternalImageMs = value; NotifyPropertyChanged("Sigma1"); } }
        public int ImageCount { get { return _tempConfig.ImageCount; } set { _tempConfig.ImageCount = value; NotifyPropertyChanged("Sigma1"); } }
        public int ImageShowMs { get { return _tempConfig.ImageShowMs; } set { _tempConfig.ImageShowMs = value; NotifyPropertyChanged("Sigma1"); } }

        public double Sigma1 { get { return _tempConfig.Sigma1; } set { _tempConfig.Sigma1 = value; NotifyPropertyChanged("Sigma1"); } }
        public double Sigma2 { get { return _tempConfig.Sigma2; } set { _tempConfig.Sigma2 = value; NotifyPropertyChanged("Sigma2"); } }
        public double ThresholdRateMax { get { return _tempConfig.ThresholdRateMax; } set { _tempConfig.ThresholdRateMax = value; NotifyPropertyChanged("ThresholdRateMax"); } }
        public int SelectAreaMin { get { return _tempConfig.SelectAreaMin; } set { _tempConfig.SelectAreaMin = value; NotifyPropertyChanged("SelectAreaMin"); } }
        public int SelectAreaMax { get { return _tempConfig.SelectAreaMax; } set { _tempConfig.SelectAreaMax = value; NotifyPropertyChanged("SelectAreaMax"); } }

        public ICommand ResetCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }

        public ArgsPageViewModel()
        {
            _sureConfig = LoadConfig();
            _tempConfig = _sureConfig.Clone();
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
        }

        private Config LoadConfig(bool reset = false)
        {
            if (reset || !File.Exists(_configFilePath))
            {
                return new Config();
            }
            else
            {
                string json = File.ReadAllText(_configFilePath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }

        private void SaveConfig()
        {
            _sureConfig = _tempConfig.Clone();
            string json = JsonConvert.SerializeObject(_sureConfig, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
        }

        private void Reset(object obj)
        {
            _tempConfig = LoadConfig(true);
            NotifyPropertyChanged("DelayFristImageMs");
            NotifyPropertyChanged("InternalImageMs");
            NotifyPropertyChanged("ImageCount");
            NotifyPropertyChanged("ImageShowMs");

            NotifyPropertyChanged("Sigma1");
            NotifyPropertyChanged("Sigma2");
            NotifyPropertyChanged("ThresholdRateMax");
            NotifyPropertyChanged("SelectAreaMin");
            NotifyPropertyChanged("SelectAreaMax");
            MessageBox.Show("重置成功");
        }

        private void Apply(object obj)
        {
            SaveConfig();
            DataCenter.Instance.SetData("Config", _sureConfig);
            MessageBox.Show("保存且应用成功");
        }
    }
}