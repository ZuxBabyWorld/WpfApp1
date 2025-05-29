using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public class ArgsPageViewModel : ViewModelBase
    {
        private Config _tempConfig;

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
            _tempConfig = ConfigManager.Instance.GetConfigClone();
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
        }

        private void SaveConfig()
        {
            ConfigManager.Instance.SaveConfig(_tempConfig);
        }

        private void Reset(object obj)
        {
            ConfigManager.Instance.ResetConfig(true);
            _tempConfig = ConfigManager.Instance.GetConfigClone();
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
            MessageBox.Show("保存且应用成功");
        }
    }
}