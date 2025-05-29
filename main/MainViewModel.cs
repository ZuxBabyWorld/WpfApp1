using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {
        //主页
        private MainPage _mainPage = new MainPage();

        private DispatcherTimer _listenTimer;
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
            _listenCallCount = 0;
            _listenTimer = new DispatcherTimer();
            Config config = DataCenter.Instance.GetData<Config>("Config");
            _listenTimer.Interval = TimeSpan.FromMilliseconds(config.DelayFristImageMs);
            _listenTimer.Tick += OnTargetTick;
            _listenTimer.Start();
        }
        private void OnTargetTick(object sender, EventArgs e)
        {
            Config config = DataCenter.Instance.GetData<Config>("Config");
            if (_listenCallCount < config.ImageCount)
            {
                _listenCallCount++;
                if (_mainPage.DataContext is MainPageViewModel mv)
                {
                    mv.DoProcessImage();
                }
                if (_listenCallCount == 1)
                {
                    _listenTimer.Interval = TimeSpan.FromMilliseconds(config.InternalImageMs);
                }
            }
            else
            {
                _listenTimer.Stop();
            }
        }

        public SimpleCommand StopListen { get; private set; }
        private void DoStopListen(Object obj)
        {
            _isListenKey = false;
        }

        public MainViewModel()
        {
            ChangeMainContent(_mainPage);
            ToMainPage = new SimpleCommand(DoToMainPage);
            ToArgsPage = new SimpleCommand(DoToArgsPage);
            StartListen = new SimpleCommand(DoStartListen);
            StopListen = new SimpleCommand(DoStopListen);
        }
    }
}