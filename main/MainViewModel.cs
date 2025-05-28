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
        private MainPage _mainPage = new MainPage();

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

        public MainViewModel()
        {
            ChangeMainContent(_mainPage);
            ToMainPage = new SimpleCommand(DoToMainPage);
            ToArgsPage = new SimpleCommand(DoToArgsPage);
        }
    }
}