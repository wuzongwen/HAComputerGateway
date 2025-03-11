using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAComputerGateway.Core.Configs
{
    public class AppSetting
    {
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string MqttBroker { get; set; }
        public int MqttPort { get; set; }
        public string MqttClientId { get; set; }
        public string MqttTopic { get; set; }
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public string ShutDownInstruction { get; set; }
        public string ShutDownTopic { get; set; }
        public string SystemInfoTopic { get; set; }
        public int TimeDelay { get; set; }
        public int SystemInfoPushInterval { get; set; }
    }
}
