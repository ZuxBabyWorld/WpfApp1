using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public class ArgsPageViewModel : ViewModelBase
    {
        private double _sigma1, _sigma2 ;
        private double _thresholdRateMax;
        private int _selectAreaMin;
        private int _selectAreaMax;

        public double Sigma1 { get { return _sigma1; } set { _sigma1 = value; NotifyPropertyChanged("Sigma1"); } }
        public double Sigma2 { get { return _sigma2; } set { _sigma2 = value; NotifyPropertyChanged("Sigma2"); } }
        public double ThresholdRateMax { get { return _thresholdRateMax; } set { _thresholdRateMax = value; NotifyPropertyChanged("ThresholdRateMax"); } }
        public int SelectAreaMin { get { return _selectAreaMin; } set { _selectAreaMin = value; NotifyPropertyChanged("SelectAreaMin"); } }
        public int SelectAreaMax { get { return _selectAreaMax; } set { _selectAreaMax = value; NotifyPropertyChanged("SelectAreaMax"); } }

        public ICommand ResetCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }

        public ArgsPageViewModel()
        {
            Config config = ConfigManager.Instance.GetConfig();
            SetValues(config.argsConfig);
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
        }

        private void SetValues(ArgsConfig config = null)
        {
            if (config == null)
            {
                config = new ArgsConfig();
            }
            Sigma1 = config.Sigma1;
            Sigma2 = config.Sigma2;
            ThresholdRateMax = config.ThresholdRateMax;
            SelectAreaMin = config.SelectAreaMin;
            SelectAreaMax = config.SelectAreaMax;
        }

        private void Reset(object obj)
        {
            SetValues();
        }

        private void Apply(object obj)
        {
            ArgsConfig config = ConfigManager.Instance.GetConfig().argsConfig;
            config.Sigma1 = this.Sigma1;
            config.Sigma2 = this.Sigma2;
            config.ThresholdRateMax = this.ThresholdRateMax;
            config.SelectAreaMin = this.SelectAreaMin;
            config.SelectAreaMax = this.SelectAreaMax;
            ConfigManager.Instance.SaveConfig();
            MessageBox.Show("保存且应用成功");
        }
    }
}