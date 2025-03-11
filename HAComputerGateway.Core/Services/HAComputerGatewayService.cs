using MQTTnet.Client;
using MQTTnet;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HAComputerGateway.Core.Configs;
using HAComputerGateway.Core.Helpers;

namespace HAComputerGateway.Core.Services
{
    public class HAComputerGatewayService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IMqttClient mqttClient;
        private AppSetting config;
        private Timer networkTimer; //网络检测定时器
        private Timer systemInfoTimer; // 系统信息定时器

        public HAComputerGatewayService(AppSetting config)
        {
            var originalConsoleOut = Console.Out;
            Console.SetOut(new NLogTextWriter(originalConsoleOut));
            this.config = config;
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
        }

        public void Start()
        {
            Console.WriteLine("服务启动");
            // 启动定时器，立即开始，间隔10秒（10000毫秒）
            networkTimer = new Timer(NetworkCheck, null, 0, 10000);
        }

        public void Stop()
        {
            // 释放网络检测定时器（如果尚未停止）
            networkTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            networkTimer?.Dispose();
            networkTimer = null;

            // 释放系统信息定时器
            systemInfoTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            systemInfoTimer?.Dispose();
            systemInfoTimer = null;

            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.DisconnectAsync().GetAwaiter().GetResult();
                Logger.Info("MQTT 客户端断开连接。");
            }

            Console.WriteLine("服务停止");
            mqttClient?.DisconnectAsync().Wait();
        }

        private async Task ConnectMqttClient()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(config.MqttBroker, config.MqttPort)
                .WithCredentials(config.MqttUserName, config.MqttPassword)
                .WithClientId(config.MqttPassword)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine("接收到MQTT消息：{0}", message);
                if (message == config.ShutDownInstruction)
                {
                    Console.WriteLine("接收到关机指令");
                    ShutdownComputer();
                }
                return Task.CompletedTask;
            };

            mqttClient.ConnectedAsync += async e =>
            {
                //发布上线主题
                await mqttClient.PublishStringAsync($"{config.MqttTopic}/{config.MqttClientId}/availability", "online");

                //订阅关机主题
                Console.WriteLine("MQTT服务已连接");
                await mqttClient.SubscribeAsync($"{config.MqttTopic}/{config.MqttClientId}/{config.ShutDownTopic}");

                //发布系统信息主题
                await PushSystemInfo();
                // 启动系统信息定时器，每30秒获取一次系统信息并发布
                systemInfoTimer = new Timer(async _ =>
                {
                    await PushSystemInfo();
                }, null, TimeSpan.FromSeconds(config.SystemInfoPushInterval), TimeSpan.FromSeconds(config.SystemInfoPushInterval));
            };

            try
            {
                await mqttClient.ConnectAsync(mqttClientOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("连接MQTT服务失败：" + ex.Message);

                // 如果连接失败，则重新启动网络检测定时器以尝试重连（如果定时器已被释放，则重新创建）
                if (networkTimer == null)
                {
                    networkTimer = new Timer(NetworkCheck, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
                }
            }
        }

        /// <summary>
        /// 发布系统信息
        /// </summary>
        /// <returns></returns>
        private async Task PushSystemInfo()
        {
            string sysInfo = SystemInfoHelper.GetSystemInfo();
            if (sysInfo != null)
            {
                try
                {
                    await mqttClient.PublishStringAsync($"{config.MqttTopic}/{config.MqttClientId}/{config.SystemInfoTopic}", sysInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("系统发送失败：{0}", ex);
                }
                Console.WriteLine("系统信息已发送");
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
                Process.Start("shutdown", $"/s /t {config.TimeDelay}");
                Console.WriteLine("关机指令执行成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"关机指令执行失败,{JsonSerializer.Serialize(ex)}");
            }
        }

        /// <summary>
        /// 定时器回调：检测网络状态
        /// </summary>
        private void NetworkCheck(object state)
        {
            if (NetworkHelper.IsNetworkAvailable(out IPAddress ip, out string mac))
            {
                Console.WriteLine("网络检测通过，本机IP地址：{0}；本机MAC地址：{1}", ip, mac);
                // 网络就绪，停止并释放定时器
                networkTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                networkTimer?.Dispose();
                networkTimer = null;
                // 开始连接MQTT服务
                Task.Run(() => ConnectMqttClient());
            }
            else
            {
                Console.WriteLine("网络未就绪，10秒后重试...");
            }
        }
    }
}
