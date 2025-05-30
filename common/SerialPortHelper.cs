using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfApp1
{
    public sealed class SerialPortHelper : IDisposable
    {
        #region 单例实现
        private static readonly Lazy<SerialPortHelper> lazy =
            new Lazy<SerialPortHelper>(() => new SerialPortHelper());

        public static SerialPortHelper Instance { get { return lazy.Value; } }

        private SerialPortHelper()
        {
            // 私有构造函数，防止外部实例化
            InitializeSerialPort();
        }
        #endregion

        #region 字段
        private SerialPort _serialPort;
        private Dispatcher _dispatcher;
        private CancellationTokenSource _readCancellationTokenSource;
        private bool _isDisposed;
        private readonly object _lockObject = new object();
        #endregion

        #region 属性
        public bool IsOpen => _serialPort?.IsOpen ?? false;
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }
        public Parity Parity { get; private set; }
        public int DataBits { get; private set; }
        public StopBits StopBits { get; private set; }
        public Handshake Handshake { get; private set; }
        #endregion

        #region 事件
        public event EventHandler<string> DataReceived;
        public event EventHandler<string> ErrorOccurred;
        public event EventHandler ConnectionStatusChanged;
        #endregion

        #region 公共方法
        public void Initialize(Dispatcher dispatcher, string portName, int baudRate = 9600,
            Parity parity = Parity.None, int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            lock (_lockObject)
            {
                if (IsOpen)
                    throw new InvalidOperationException("串口已打开，无法修改配置");

                _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
                PortName = portName;
                BaudRate = baudRate;
                Parity = parity;
                DataBits = dataBits;
                StopBits = stopBits;
                Handshake = handshake;
            }
        }

        public bool Open()
        {
            lock (_lockObject)
            {
                try
                {
                    if (IsOpen)
                        return true;

                    if (_dispatcher == null)
                        throw new InvalidOperationException("请先调用Initialize方法设置Dispatcher");

                    if (string.IsNullOrEmpty(PortName))
                        throw new InvalidOperationException("串口名称未设置");

                    _serialPort.PortName = PortName;
                    _serialPort.BaudRate = BaudRate;
                    _serialPort.Parity = Parity;
                    _serialPort.DataBits = DataBits;
                    _serialPort.StopBits = StopBits;
                    _serialPort.Handshake = Handshake;
                    _serialPort.Encoding = System.Text.Encoding.Default;
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;

                    _serialPort.Open();
                    StartReading();
                    OnConnectionStatusChanged();
                    return true;
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"打开串口失败: {ex.Message}");
                    return false;
                }
            }
        }

        public void Close()
        {
            lock (_lockObject)
            {
                try
                {
                    _readCancellationTokenSource?.Cancel();

                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _serialPort.Close();
                        OnConnectionStatusChanged();
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"关闭串口失败: {ex.Message}");
                }
            }
        }

        public bool Write(string data)
        {
            if (!IsOpen)
            {
                OnErrorOccurred("串口未打开，无法写入数据");
                return false;
            }

            try
            {
                _serialPort.WriteLine(data);
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"写入数据失败: {ex.Message}");
                return false;
            }
        }

        public bool Write(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
            {
                OnErrorOccurred("串口未打开，无法写入数据");
                return false;
            }

            try
            {
                _serialPort.Write(buffer, offset, count);
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"写入数据失败: {ex.Message}");
                return false;
            }
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region 私有方法
        private void InitializeSerialPort()
        {
            _serialPort = new SerialPort();
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        private async void StartReading()
        {
            _readCancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(async () =>
                {
                    var buffer = new byte[4096];

                    while (!_readCancellationTokenSource.Token.IsCancellationRequested && IsOpen)
                    {
                        try
                        {
                            if (_serialPort.BytesToRead > 0)
                            {
                                int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    string data = Encoding.Default.GetString(buffer, 0, bytesRead);
                                    _dispatcher.Invoke(() => OnDataReceived(data));
                                }
                            }
                            else
                            {
                                await Task.Delay(10, _readCancellationTokenSource.Token);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // 操作被取消，退出循环
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (!_readCancellationTokenSource.Token.IsCancellationRequested)
                            {
                                _dispatcher.Invoke(() => OnErrorOccurred($"读取数据失败: {ex.Message}"));
                                // 发生错误时关闭串口
                                _dispatcher.Invoke(Close);
                            }
                            break;
                        }
                    }
                }, _readCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // 操作被取消
            }
            catch (Exception ex)
            {
                _dispatcher.Invoke(() => OnErrorOccurred($"读取任务异常: {ex.Message}"));
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            OnErrorOccurred($"串口错误: {e.EventType}");
        }

        private void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, data);
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, errorMessage);
        }

        private void OnConnectionStatusChanged()
        {
            ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        // 修复CS0549错误：密封类中不能有虚拟成员
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _readCancellationTokenSource?.Cancel();

                    if (_serialPort != null)
                    {
                        _serialPort.ErrorReceived -= SerialPort_ErrorReceived;
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.DiscardInBuffer();
                            _serialPort.DiscardOutBuffer();
                            _serialPort.Close();
                        }
                        _serialPort.Dispose();
                        _serialPort = null;
                    }

                    _readCancellationTokenSource?.Dispose();
                    _readCancellationTokenSource = null;
                }

                _isDisposed = true;
            }
        }

        ~SerialPortHelper()
        {
            Dispose(false);
        }
        #endregion
    }
}