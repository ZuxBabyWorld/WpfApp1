using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class LightManager
    {
        // 单例实例
        private static readonly Lazy<LightManager> instance = new Lazy<LightManager>(() => new LightManager());
        // 私有构造函数，确保不能从外部实例化
        private LightManager() { }

        // 获取单例实例的公共属性
        public static LightManager Instance => instance.Value;
        //----------------

        public void OpenLight()
        {
            CommonConfig config = ConfigManager.Instance.GetConfig().commonConfig;
            var pConfig = new SerialPortConfig
            {
                PortName = config.LightComName,
                BaudRate = config.LightComBaudRate,
                IsHexMode = true
            };
            var connection = SerialPortManager.Instance.GetConnection(pConfig);
            connection.DataReceived += (s, data) => {
                Debug.WriteLine("OpenLight 返回:" + data);
            };
            connection.StatusChanged += (s, status) => {
                Debug.WriteLine("OpenLight 状态:" + status);
            };
            connection.Send("48 65 6C 6C 6F");
            SerialPortManager.Instance.ReleaseConnection(connection);
        }

        public void CloseLight()
        {
            CommonConfig config = ConfigManager.Instance.GetConfig().commonConfig;
            var pConfig = new SerialPortConfig
            {
                PortName = config.LightComName,
                BaudRate = config.LightComBaudRate,
                IsHexMode = true
            };
            var connection = SerialPortManager.Instance.GetConnection(pConfig);
            connection.DataReceived += (s, data) => {
                Debug.WriteLine("OpenLight 返回:" + data);
            };
            connection.StatusChanged += (s, status) => {
                Debug.WriteLine("OpenLight 状态:" + status);
            };
            connection.Send("00 00 00");
            SerialPortManager.Instance.ReleaseConnection(connection);
        }
    }
}
