using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {
        //主页
        private MainPage _mainPage;
        //配置
        private ArgsPageViewModel _argsPageViewModel;
        //算法
        private BumpDefectProcessor _bumpProcessor;

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
        private void DoToMainPage(Object obj = null)
        {
            _mainPage = new MainPage();
            _mainPage.DataContext = new MainPageViewModel(_bumpProcessor);
            ChangeMainContent(_mainPage);
        }

        public SimpleCommand ToArgsPage { get; private set; }
        private void DoToArgsPage(Object obj)
        {
            ArgsPage argsPage = new ArgsPage();
            argsPage.DataContext = _argsPageViewModel;
            ChangeMainContent(argsPage);
        }

        public MainViewModel()
        {
            _argsPageViewModel = new ArgsPageViewModel();
            _bumpProcessor = new BumpDefectProcessor();
            _argsPageViewModel.SetBumpProcessor(_bumpProcessor);
            //默认切换主页
            DoToMainPage();

            ToMainPage = new SimpleCommand(DoToMainPage);
            ToArgsPage = new SimpleCommand(DoToArgsPage);
        }
    }
}