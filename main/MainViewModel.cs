using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {
        //主页
        private MainPage _mainPage = new MainPage();

        private DispatcherTimer _listenTimer = null;
        private int _listenCallCount = 0;

        private Frame _mainContent;
        public Frame MainContent
        {
            get { return _mainContent; }
            set { _mainContent = value; NotifyPropertyChanged("MainContent"); }
        }

        private void ChangeMainContent(UserControl control)
        {
            MainContent = new Frame()
            {
                Content = control
            };
        }

        public SimpleCommand ToMainPage { get; private set; }
        private void DoToMainPage(Object obj)
        {
            ChangeMainContent(_mainPage);
        }

        public SimpleCommand ToArgsPage { get; private set; }
        private void DoToArgsPage(Object obj)
        {
            ChangeMainContent(new ArgsPage());
        }

        private bool _isListenKey = false;
        public bool IsListenKey { get { return _isListenKey; } }

        public SimpleCommand StartListen { get; private set; }
        private void DoStartListen(Object obj)
        {
            _isListenKey = true;
        }
        public void OnTargetComeTrigger()
        {
            if (_listenTimer != null)
            {
                MessageBox.Show("还在处理上次触发");
                return;
            }
            _listenCallCount = 0;
            _listenTimer = new DispatcherTimer();
            CommonConfig config = ConfigManager.Instance.GetConfig().commonConfig;
            _listenTimer.Interval = TimeSpan.FromMilliseconds(1);
            _listenTimer.Tick += OnTargetTick;
            _listenTimer.Start();
        }
        private void OnTargetTick(object sender, EventArgs e)
        {
            CommonConfig config = ConfigManager.Instance.GetConfig().commonConfig;
            if (_listenCallCount == 0)
            {
                _listenCallCount = 1;
                _listenTimer.Interval = TimeSpan.FromMilliseconds(config.DelayFristImageMs);
                Debug.WriteLine("打开光源1");
                LightManager.Instance.OpenLight();
            }
            else if (_listenCallCount >=1 && _listenCallCount < (1 + config.ImageCount))
            {   
                ++_listenCallCount;
                _listenTimer.Interval = TimeSpan.FromMilliseconds(config.InternalImageMs);
                if (_mainPage.DataContext is MainPageViewModel mv)
                {
                    Debug.WriteLine("采集图像");
                    mv.DoProcessImage();
                }
            }else
            {
                _listenTimer.Stop();
                _listenTimer = null;
                Debug.WriteLine("结束");
                LightManager.Instance.CloseLight();
            }
        }

        public SimpleCommand StopListen { get; private set; }
        private void DoStopListen(Object obj)
        {
            _isListenKey = false;
        }

        public SimpleCommand ToSettingPage { get; private set; }
        private void DoToSettingPage(Object obj)
        {
            ChangeMainContent(new SettingPage());
        }

        public MainViewModel()
        {
            ChangeMainContent(_mainPage);
            ToMainPage = new SimpleCommand(DoToMainPage);
            ToArgsPage = new SimpleCommand(DoToArgsPage);
            StartListen = new SimpleCommand(DoStartListen);
            StopListen = new SimpleCommand(DoStopListen);
            ToSettingPage = new SimpleCommand(DoToSettingPage);
        }
    }
}