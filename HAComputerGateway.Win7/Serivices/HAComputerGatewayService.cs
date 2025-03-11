using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HAComputerGateway.Win7.Helpers;
using uPLibrary.Networking.M2Mqtt;
using NLog;

namespace HAComputerGateway.Win7.Serivices
{
    public class HAComputerGatewayService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private System.Timers.Timer networkTimer; //网络检测定时器
        private System.Timers.Timer systemInfoTimer; // 系统信息定时器
        private MqttHelper _mqtt;

        public void Start()
        {
            try
            {
                Logger.Info("服务启动中...");
                // 启动定时器，立即开始，间隔10秒（10000毫秒）
                networkTimer = new System.Timers.Timer(10000);
                networkTimer.Elapsed += (sender, e) => NetworkCheck();
                networkTimer.AutoReset = true;
                networkTimer.Start();

                _mqtt = new MqttHelper();
                _mqtt.Connect();

                // 首次立即推送硬件信息
                ReportHardwareInfo();

                // 定时推送
                systemInfoTimer = new System.Timers.Timer(30000);
                systemInfoTimer.Elapsed += (sender, e) => ReportHardwareInfo();
                systemInfoTimer.AutoReset = true;
                systemInfoTimer.Start();

                Logger.Info("服务已启动，定时上报间隔 30 秒");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "服务启动失败");
                throw;
            }
        }

        public void Stop()
        {
            Logger.Info("服务停止中...");
            systemInfoTimer?.Stop();
            _mqtt?.Disconnect();
            Logger.Info("服务已停止");
        }

        /// <summary>
        /// 定时器回调：检测网络状态
        /// </summary>
        private void NetworkCheck()
        {
            if (NetworkHelper.IsNetworkAvailable(out IPAddress ip, out string mac))
            {
                Console.WriteLine("网络检测通过，本机IP地址：{0}；本机MAC地址：{1}", ip, mac);
                // 网络就绪，停止并释放定时器
                networkTimer?.Stop();

                //订阅关机主题
                _mqtt = new MqttHelper();
                _mqtt.Connect();

                //推送硬件信息
                ReportHardwareInfo();

                // 定时推送
                systemInfoTimer = new System.Timers.Timer(30000);
                systemInfoTimer.Elapsed += (sender, e) => ReportHardwareInfo();
                systemInfoTimer.AutoReset = true;
                systemInfoTimer.Start();
            }
            else
            {
                Console.WriteLine("网络未就绪，10秒后重试...");
            }
        }

        private void ReportHardwareInfo()
        {
            try
            {
                Logger.Debug("开始采集硬件信息...");
                var info = SystemInfoHelper.GetSystemInfo();
                _mqtt.Publish("hardware/info", info);
                Logger.Info("硬件信息已推送至MQTT");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "硬件信息推送失败");
            }
        }
    }
}
