using HAComputerGateway.Win7.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace HAComputerGateway.Win7.Helpers
{
    /// <summary>
    /// INI文件帮助类，提供初始化和读取配置功能
    /// </summary>
    public class IniHelper
    {
        // INI 文件完整路径（默认为程序根目录下的 config.ini）
        private readonly string iniPath;

        // 使用 P/Invoke 调用 Windows API
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 构造函数，默认使用 config.ini 文件
        /// </summary>
        /// <param name="iniFileName">INI 文件名称</param>
        public IniHelper(string iniFileName = "Config.ini")
        {
            // 程序根目录路径
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            iniPath = Path.Combine(basePath, iniFileName);
        }

        /// <summary>
        /// 初始化 INI 文件，如果文件不存在则创建并写入默认配置
        /// </summary>
        public void InitializeIniFile()
        {
            if (!File.Exists(iniPath))
            {
                // 写入默认配置，这里以 "General" 配置段为例
                WritePrivateProfileString("Config", "ServiceName", "HAComputerGateway", iniPath);
                WritePrivateProfileString("Config", "ServiceDescription", "HomeAssistant获取电脑相关信息和控制电脑的服务", iniPath);
                WritePrivateProfileString("Config", "MqttBroker", "MQTT服务器地址", iniPath);
                WritePrivateProfileString("Config", "MqttPort", "MQTT服务器端口", iniPath);
                WritePrivateProfileString("Config", "MqttUserName", "MQTT用户名", iniPath);
                WritePrivateProfileString("Config", "MqttPassword", "MQTT密码", iniPath);
                WritePrivateProfileString("Config", "MqttClientId", "客户端ID,多台电脑请保持唯一", iniPath);
                WritePrivateProfileString("Config", "MqttTopic", "MQTT主题", iniPath);
                WritePrivateProfileString("Config", "ShutDownTopic", "关机主题名称", iniPath);
                WritePrivateProfileString("Config", "SystemInfoTopic", "系统信息主题名称", iniPath);
                WritePrivateProfileString("Config", "ShutDownInstruction", "关机指令", iniPath);
                WritePrivateProfileString("Config", "TimeDelay", "关机延时，单位秒", iniPath);
                WritePrivateProfileString("Config", "SystemInfoPushInterval", "系统信息推送间隔，单位秒", iniPath);
                // 可根据需要增加其他默认配置项
                Console.WriteLine("配置文件不存在，已创建并写入默认配置。");
            }
        }

        /// <summary>
        /// 读取指定配置段下的所有键值，并将其映射到实体类中
        /// </summary>
        /// <typeparam name="T">配置实体类型，要求属性名与 INI 文件中的键名一致</typeparam>
        /// <param name="section">配置段名称，例如 "General"</param>
        /// <returns>配置实体实例</returns>
        public T LoadConfig<T>(string section) where T : new()
        {
            T config = new T();
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                StringBuilder sb = new StringBuilder(255);
                // 从指定段中读取键值，键名要求与实体属性名称一致
                GetPrivateProfileString(section, prop.Name, "", sb, 255, iniPath);
                string value = sb.ToString();

                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        // 将读取到的字符串转换为属性对应的类型，并赋值
                        object convertedValue = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(config, convertedValue, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"属性 {prop.Name} 值转换失败: {ex.Message}");
                    }
                }
            }
            return config;
        }
    }
}
