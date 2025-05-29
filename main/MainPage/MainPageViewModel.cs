using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1
{
    public class MainPageViewModel : ViewModelBase
    {
        // 可调参数
        private int _maxNgCount = 20;

        // 图像处理核心
        private HalconCore _core = null;
        private BumpDefectProcessor _processor = null;

        // 私有字段，用于存储要显示的图像对象
        private HObject _displayImage;
        // 公共属性，用于绑定到视图上显示的图像
        // 当属性值改变时，触发 PropertyChanged 事件通知视图更新
        // 同时将显示区域置为 null
        public HObject DisplayImage
        {
            get { return _displayImage; }
            set { _displayImage = value; NotifyPropertyChanged("DisplayImage"); DisplayRegion = null; }
        }

        private HObject _displayRegion;
        public HObject DisplayRegion
        {
            get { return _displayRegion; }
            set { _displayRegion = value; NotifyPropertyChanged("DisplayRegion"); }
        }

        private int _ngCount = 0;
        public int NgCount
        {
            get { return _ngCount; }
            set
            {
                _ngCount = value;
                NotifyPropertyChanged("NgCount");
                NotifyPropertyChanged("ResultColor");
                NotifyPropertyChanged("ResultText");
                NotifyPropertyChanged("DisplayMsg");
            }
        }

        public string ResultColor
        {
            get
            {
                if (NgCount > _maxNgCount) return "red";
                return "#2ecc71";
            }
        }

        public string ResultText
        {
            get
            {
                if (NgCount > _maxNgCount) return "NG";
                return "GO";
            }
        }

        public string DisplayMsg
        {
            get
            {
                return "缺陷数：" + NgCount;
            }
        }

        public SimpleCommand ProcessImage { get; private set; }

        // 采集队列
        private Queue<(int, HObject)> _acquisitionQueue = new Queue<(int, HObject)>();
        // 计算队列
        private Queue<(int, HObject)> _processingQueue = new Queue<(int, HObject)>();
        // 显示队列
        private Queue<(int, HObject, HObject, int)> _displayQueue = new Queue<(int, HObject, HObject, int)>();

        // 处理序号计数器
        private int _processingIndex = 0;

        // 显示定时器
        private DispatcherTimer _displayTimer;

        // 停止标志位
        private volatile bool _isStopping = false;

        // 用于线程同步的事件
        private ManualResetEvent _workAvailable = new ManualResetEvent(false);

        public MainPageViewModel()
        {
            _core = new HalconCore();
            _processor = new BumpDefectProcessor();
            ProcessImage = new SimpleCommand(DoProcessImage);

            // 启动显示定时器
            _displayTimer = new DispatcherTimer();
            Config config = ConfigManager.Instance.GetConfig();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(config.ImageShowMs);
            _displayTimer.Tick += DisplayTimer_Tick;
            _displayTimer.Start();

            // 启动处理线程
            ThreadPool.QueueUserWorkItem(ProcessQueue);
        }

        public void DoProcessImage(Object obj = null)
        {
            // 打开相机
            if (_core.OpenCamera() == null)
            {
                return;
            }

            // 采集灰度图
            HObject capturedImage = _core.CaptureGrayImage();
            if (capturedImage != null)
            {
                // 分配处理序号
                int index = Interlocked.Increment(ref _processingIndex);

                // 将图像放入采集队列
                lock (_acquisitionQueue)
                {
                    _acquisitionQueue.Enqueue((index, capturedImage));
                }

                // 通知处理线程有新工作
                _workAvailable.Set();
            }
        }

        private void ProcessQueue(object state)
        {
            while (!_isStopping)
            {
                // 等待有工作可做
                _workAvailable.WaitOne();

                // 从采集队列中提取图像
                (int index, HObject image) = (0, null);
                lock (_acquisitionQueue)
                {
                    if (_acquisitionQueue.Count > 0)
                    {
                        (index, image) = _acquisitionQueue.Dequeue();
                    }
                }

                if (image != null)
                {
                    // 将图像放入计算队列
                    lock (_processingQueue)
                    {
                        _processingQueue.Enqueue((index, image));
                    }

                    // 处理计算队列中的图像
                    lock (_processingQueue)
                    {
                        if (_processingQueue.Count > 0)
                        {
                            (int processingIndex, HObject processingImage) = _processingQueue.Dequeue();

                            // 进行算法处理
                            HObject regionImage = _core.ProcessImage(processingImage, _processor);
                            int ngCount = _core.NgCount;

                            // 将处理结果放入显示队列
                            lock (_displayQueue)
                            {
                                _displayQueue.Enqueue((processingIndex, processingImage, regionImage, ngCount));
                            }
                        }
                    }
                }

                // 如果队列为空，重置事件
                lock (_acquisitionQueue)
                {
                    if (_acquisitionQueue.Count == 0)
                    {
                        _workAvailable.Reset();
                    }
                }
            }
        }

        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            // 从显示队列中提取处理后的图像和缺陷数
            lock (_displayQueue)
            {
                if (_displayQueue.Count > 0)
                {
                    (int index, HObject processingImage, HObject regionImage, int ngCount) = _displayQueue.Dequeue();

                    // 显示处理后的图像和缺陷数
                    DisplayImage = processingImage;
                    DisplayRegion = regionImage;
                    NgCount = ngCount;
                }
            }
        }

        // 停止处理线程的方法
        public void StopProcessing()
        {
            _isStopping = true;
            _workAvailable.Set(); // 唤醒可能正在等待的线程
        }
    }
}
