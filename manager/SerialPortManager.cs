using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    /// <summary>
    /// 串口连接配置
    /// </summary>
    public class SerialPortConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public bool IsHexMode { get; set; } = false;
    }

    /// <summary>
    /// 串口连接对象
    /// </summary>
    public interface ISerialPortConnection : IDisposable
    {
        bool IsOpen { get; }
        string PortName { get; }
        bool IsHexMode { get; set; }

        event EventHandler<string> DataReceived;
        event EventHandler<string> StatusChanged;

        bool Send(string data);
        Task<bool> SendAsync(string data);
    }

    /// <summary>
    /// 串口管理器单例，用于WPF项目中管理串口通信
    /// </summary>
    public sealed class SerialPortManager
    {
        #region 单例实现
        private static readonly Lazy<SerialPortManager> lazy =
            new Lazy<SerialPortManager>(() => new SerialPortManager());

        public static SerialPortManager Instance { get { return lazy.Value; } }

        private SerialPortManager() { }
        #endregion

        #region 私有字段
        private readonly ConcurrentDictionary<string, SerialPortConnection> _connections =
            new ConcurrentDictionary<string, SerialPortConnection>();
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取系统中可用的串口列表
        /// </summary>
        /// <returns>串口名称数组</returns>
        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 获取或创建串口连接
        /// </summary>
        /// <param name="config">串口配置</param>
        /// <returns>串口连接对象</returns>
        public ISerialPortConnection GetConnection(SerialPortConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(config.PortName))
                throw new ArgumentException("PortName cannot be null or empty", nameof(config.PortName));

            string key = $"{config.PortName}_{config.BaudRate}_{config.DataBits}_{config.StopBits}_{config.Parity}";

            return _connections.GetOrAdd(key, k =>
            {
                var connection = new SerialPortConnection(config);
                connection.ConnectionReleased += OnConnectionReleased;
                return connection;
            });
        }

        /// <summary>
        /// 释放指定的串口连接
        /// </summary>
        /// <param name="connection">要释放的连接</param>
        public void ReleaseConnection(ISerialPortConnection connection)
        {
            if (connection is SerialPortConnection conn)
            {
                conn.Release();
            }
        }
        #endregion

        #region 私有方法
        private void OnConnectionReleased(object sender, EventArgs e)
        {
            if (sender is SerialPortConnection connection)
            {
                connection.ConnectionReleased -= OnConnectionReleased;

                if (_connections.TryRemove(connection.Key, out _))
                {
                    connection.Dispose();
                }
            }
        }
        #endregion

        #region 内部实现类
        private class SerialPortConnection : ISerialPortConnection
        {
            private SerialPort _serialPort;
            private readonly SerialPortConfig _config;
            private int _referenceCount = 0;
            private bool _isDisposed = false;

            public string Key { get; }
            public string PortName => _config.PortName;
            public bool IsHexMode { get => _config.IsHexMode; set => _config.IsHexMode = value; }
            public bool IsOpen => _serialPort?.IsOpen ?? false;

            public event EventHandler<string> DataReceived;
            public event EventHandler<string> StatusChanged;
            public event EventHandler ConnectionReleased;

            public SerialPortConnection(SerialPortConfig config)
            {
                _config = config;
                Key = $"{config.PortName}_{config.BaudRate}_{config.DataBits}_{config.StopBits}_{config.Parity}";
                Open();
            }

            private void Open()
            {
                try
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(nameof(SerialPortConnection));

                    if (IsOpen)
                        return;

                    _serialPort = new SerialPort
                    {
                        PortName = _config.PortName,
                        BaudRate = _config.BaudRate,
                        DataBits = _config.DataBits,
                        StopBits = _config.StopBits,
                        Parity = _config.Parity,
                        ReadTimeout = 500,
                        WriteTimeout = 500,
                        Encoding = Encoding.UTF8
                    };

                    _serialPort.DataReceived += SerialPortDataReceived;
                    _serialPort.ErrorReceived += SerialPortErrorReceived;

                    _serialPort.Open();
                    _referenceCount++;
                    OnStatusChanged($"串口 {_config.PortName} 已打开");
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"打开串口失败: {ex.Message}");
                    throw;
                }
            }

            public bool Send(string data)
            {
                if (!IsOpen)
                {
                    OnStatusChanged("无法发送数据: 串口未打开");
                    return false;
                }

                try
                {
                    if (IsHexMode)
                    {
                        byte[] bytes = HexStringToByteArray(data);
                        _serialPort.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        _serialPort.Write(data);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"发送数据失败: {ex.Message}");
                    return false;
                }
            }

            public async Task<bool> SendAsync(string data)
            {
                return await Task.Run(() => Send(data));
            }

            public void Release()
            {
                if (_isDisposed)
                    return;

                _referenceCount--;

                if (_referenceCount <= 0)
                {
                    Close();
                    ConnectionReleased?.Invoke(this, EventArgs.Empty);
                }
            }

            private void Close()
            {
                try
                {
                    if (_serialPort != null)
                    {
                        if (_serialPort.IsOpen)
                        {
                            _serialPort.DataReceived -= SerialPortDataReceived;
                            _serialPort.ErrorReceived -= SerialPortErrorReceived;
                            _serialPort.Close();
                        }

                        _serialPort.Dispose();
                        _serialPort = null;
                        OnStatusChanged("串口已关闭");
                    }
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"关闭串口失败: {ex.Message}");
                }
            }

            private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                try
                {
                    if (_serialPort == null || !_serialPort.IsOpen)
                        return;

                    string data;

                    if (IsHexMode)
                    {
                        int bytesToRead = _serialPort.BytesToRead;
                        byte[] buffer = new byte[bytesToRead];
                        _serialPort.Read(buffer, 0, bytesToRead);
                        data = ByteArrayToHexString(buffer);
                    }
                    else
                    {
                        data = _serialPort.ReadExisting();
                    }

                    OnDataReceived(data);
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"接收数据出错: {ex.Message}");
                }
            }

            private void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
            {
                OnStatusChanged($"串口错误: {e.EventType}");
            }

            private void OnDataReceived(string data)
            {
                DataReceived?.Invoke(this, data);
            }

            private void OnStatusChanged(string status)
            {
                StatusChanged?.Invoke(this, status);
            }

            private string ByteArrayToHexString(byte[] bytes)
            {
                if (bytes == null || bytes.Length == 0)
                    return string.Empty;

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.AppendFormat("{0:X2} ", b);
                }

                return builder.ToString().TrimEnd();
            }

            private byte[] HexStringToByteArray(string hex)
            {
                if (string.IsNullOrWhiteSpace(hex))
                    return new byte[0];

                hex = hex.Replace(" ", "");
                int length = hex.Length;

                if (length % 2 != 0)
                {
                    OnStatusChanged("HEX字符串长度必须为偶数");
                    return new byte[0];
                }

                byte[] bytes = new byte[length / 2];
                for (int i = 0; i < length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return bytes;
            }

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    Close();
                    _isDisposed = true;
                }
            }
        }
        #endregion
    }
}