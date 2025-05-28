using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace WpfApp1
{
    public class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // 私有方法，用于触发 PropertyChanged 事件
        // 当属性值改变时，调用该方法通知视图更新
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
