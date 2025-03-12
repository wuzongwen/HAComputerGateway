using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using Newtonsoft.Json;
using System.Diagnostics;
using HAComputerGateway.Win7.Configs;

namespace HAComputerGateway.Win7.Helpers
{
    public class MqttHelper
    {
        private MqttClient _client;
        private AppSetting config;

        public MqttHelper(AppSetting config)
        {
            this.config = config;
        }

        public bool Connect()
        {
            try
            {
                // 服务器地址、端口、是否使用 SSL、CA 证书、客户端证书、SSL 协议（此处为不使用 SSL）
                int port = 18133; // 根据实际情况修改端口号
                _client = new MqttClient(config.MqttBroker, config.MqttPort, false, null, null, null);
                // 使用用户名和密码进行连接
                string username = config.MqttUserName;
                string password = config.MqttPassword;

                //连接到 MQTT 服务器
                _client.Connect(config.MqttClientId, username, password);

                // 订阅关机主题
                _client.Subscribe(new[] { $"{config.MqttTopic}/{config.MqttClientId}/{config.ShutDownTopic}" }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                _client.MqttMsgPublishReceived += OnMessageReceived;
                Console.WriteLine("MQTT服务已连接");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("MQTT服务连接失败：" + ex.Message);
                return false;
            }
        }

        public void Disconnect() => _client?.Disconnect();

        private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            Console.WriteLine("接收到MQTT消息：{0}", message);
            if (message == config.ShutDownInstruction)
            {
                Console.WriteLine("接收到关机指令");
                SystemHelper.ShutdownComputer(config.TimeDelay);
            }
        }

        public void Publish(string payload)
        {
            if (_client != null && _client.IsConnected)
            {
                _client.Publish($"{config.MqttTopic}/{config.MqttClientId}/{config.SystemInfoTopic}", System.Text.Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }
    }
}
