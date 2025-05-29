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

        public ICommand ResetCommand { get; private set; }
        public ICommand ApplyCommand { get; private set; }

        public SettingPageViewModel()
        {
            Config config = ConfigManager.Instance.GetConfig();
            SetValues(config.commonConfig);
            ResetCommand = new SimpleCommand(Reset);
            ApplyCommand = new SimpleCommand(Apply);
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
            ConfigManager.Instance.SaveConfig();
            MessageBox.Show("保存且应用成功");
        }
    }
}