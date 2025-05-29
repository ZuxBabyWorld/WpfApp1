using System;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace WpfApp1
{
    public class ConfigManager
    {
        // 单例实例
        private static readonly Lazy<ConfigManager> instance = new Lazy<ConfigManager>(() => new ConfigManager());

        private string _configFilePath = "Config.json";

        private Config _config = null;

        // 私有构造函数，确保不能从外部实例化
        private ConfigManager() { }

        // 获取单例实例的公共属性
        public static ConfigManager Instance => instance.Value;

        public void LoadConfig()
        {
            if (!File.Exists(_configFilePath))
            {
                _config = new Config();
                return;
            }
            string json = File.ReadAllText(_configFilePath);
            _config = JsonConvert.DeserializeObject<Config>(json);
        }

        public void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
        }

        public Config GetConfig()
        {
            if (_config == null)
            {
                LoadConfig();
            }
            return _config;
        }
    }
}