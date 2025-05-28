using System;
using System.Collections.Generic;

namespace WpfApp1
{
    public class DataCenter
    {
        // 单例实例
        private static readonly Lazy<DataCenter> instance = new Lazy<DataCenter>(() => new DataCenter());

        // 存储数据的字典
        private readonly Dictionary<string, object> dataStore = new Dictionary<string, object>();

        // 私有构造函数，确保不能从外部实例化
        private DataCenter() { }

        // 获取单例实例的公共属性
        public static DataCenter Instance => instance.Value;

        // 存储数据的方法
        public void SetData(string key, object value)
        {
            if (dataStore.ContainsKey(key))
            {
                dataStore[key] = value;
            }
            else
            {
                dataStore.Add(key, value);
            }
        }

        // 获取数据的方法
        public T GetData<T>(string key)
        {
            if (dataStore.ContainsKey(key) && dataStore[key] is T)
            {
                return (T)dataStore[key];
            }
            return default(T);
        }

        // 移除数据的方法
        public void RemoveData(string key)
        {
            if (dataStore.ContainsKey(key))
            {
                dataStore.Remove(key);
            }
        }
    }
}