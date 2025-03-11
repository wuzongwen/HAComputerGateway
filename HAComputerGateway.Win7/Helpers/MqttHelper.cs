using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using Newtonsoft.Json;
using System.Diagnostics;

namespace HAComputerGateway.Win7.Helpers
{
    public class MqttHelper
    {
        private MqttClient _client;
        private const string Broker = "mqtt.example.com"; // MQTT服务器地址
        private const string ClientId = "PC_Client";

        public void Connect()
        {
            _client = new MqttClient(Broker);
            _client.Connect(ClientId);
            _client.Subscribe(new[] { "pc/shutdown" }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            _client.MqttMsgPublishReceived += OnMessageReceived;
        }

        public void Disconnect() => _client?.Disconnect();

        private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            Console.WriteLine("接收到MQTT消息：{0}", message);
            if (message == "shutdown")
            {
                Console.WriteLine("接收到关机指令");
                ShutdownComputer();
            }
        }

        public void Publish(string topic, string payload)
        {
            if (_client != null && _client.IsConnected)
            {
                _client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            }
        }

        /// <summary>
        /// 关机
        /// </summary>
        private void ShutdownComputer()
        {
            try
            {
                Console.WriteLine("执行关机指令");
                Process.Start("shutdown", $"/s /t 1000");
                Console.WriteLine("关机指令执行成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关机指令执行失败,{JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
