using HAComputerGateway.Win7.Serivices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace HAComputerGateway.Win7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<HAComputerGatewayService>(s =>
                {
                    s.ConstructUsing(name => new HAComputerGatewayService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("通过MQTT控制关机并定时上报系统信息");
                x.SetDisplayName("ShutdownService");
                x.SetServiceName("ShutdownService");
            });
        }
    }
}
