using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public class SettingPageViewModel : ViewModelBase
    {
        private int _delayFristImageMs;
        private int _internalImageMs;
        private int _imageCount;
        private int _imageShowMs;

        public int DelayFristImageMs { get { return _delayFristImageMs; } set { _delayFristImageMs = value; NotifyPropertyChanged("DelayFristImageMs"); } }
        public int InternalImageMs { get { return _internalImageMs; } set { _internalImageMs = value; NotifyPropertyChanged("InternalImageMs"); } }
        public int ImageCount { get { return _imageCount; } set { _imageCount = value; NotifyPropertyChanged("ImageCount"); } }
        public int ImageShowMs { get { return _imageShowMs; } set { _imageShowMs = value; NotifyPropertyChanged("ImageShowMs"); } }

        private string _lightComName;
        private int _lightComBaudRate;
        private int _lightComDataBit;
        private int _lightComStopBit;
        private int _lightComSdd;
        public string LightComName { get { return _lightComName; } set { _lightComName = value; NotifyPropertyChanged("LightComName"); } }
        public int LightComBaudRate { get { return _lightComBaudRate; } set { _lightComBaudRate = value; NotifyPropertyChanged("LightComBaudRate"); } }
        public int LightComDataBit { get { return _lightComDataBit; } set { _lightComDataBit = value; NotifyPropertyChanged("LightComDataBit"); } }
        public int LightComStopBit { get { return _lightComStopBit; } set { _lightComStopBit = value; NotifyPropertyChanged("LightComStopBit"); } }
        public int LightComSdd { get { return _lightComSdd; } set { _lightComSdd = value; NotifyPropertyChanged("LightComSdd"); } }


        public ICommand ResetCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }
        public ICommand OpenUartWinCommand { get; private set; }

        public SettingPageViewModel()
        {
            Config config = ConfigManager.Instance.GetConfig();
            SetValues(config.commonConfig);
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
            OpenUartWinCommand = new SimpleCommand(OpenUartWin);
        }

        private void SetValues(CommonConfig config = null)
        {
            if (config == null)
            {
                config = new CommonConfig();
            }
            DelayFristImageMs = config.DelayFristImageMs;
            InternalImageMs = config.InternalImageMs;
            ImageCount = config.ImageCount;
            ImageShowMs = config.ImageShowMs;

            LightComName = config.LightComName;
            LightComBaudRate = config.LightComBaudRate;
            LightComDataBit = config.LightComDataBit;
            LightComStopBit = config.LightComStopBit;
            LightComSdd = config.LightComSdd;
        }

        private void Reset(object obj)
        {
            SetValues();
        }

        private void Apply(object obj)
        {
            CommonConfig config = ConfigManager.Instance.GetConfig().commonConfig;

            config.DelayFristImageMs = this.DelayFristImageMs;
            config.InternalImageMs = this.InternalImageMs;
            config.ImageCount = this.ImageCount;
            config.ImageShowMs = this.ImageShowMs;

            config.LightComName = this.LightComName;
            config.LightComBaudRate = this.LightComBaudRate;
            config.LightComDataBit = this.LightComDataBit;
            config.LightComStopBit = this.LightComStopBit;
            config.LightComSdd = this.LightComSdd;

            ConfigManager.Instance.SaveConfig();
            MessageBox.Show("保存且应用成功");
        }

        private void OpenUartWin(object obj)
        {
            UartWindow win = new UartWindow();
            win.Show();
        }
    }
}