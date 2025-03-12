using HAComputerGateway.Win7.Configs;
using HAComputerGateway.Win7.Helpers;
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
            IniHelper iniHelper = new IniHelper();
            iniHelper.InitializeIniFile();
            var config = iniHelper.LoadConfig<AppSetting>("Config");
            if (!EntityHelper.IsEntityComplete(config)) 
            {
                Console.WriteLine("配置文件不完整，请检查配置文件");
                return;
            }
            HostFactory.Run(x =>
            {
                x.Service<HAComputerGatewayService>(s =>
                {
                    s.ConstructUsing(name => new HAComputerGatewayService(config));
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription(config.ServiceDescription);
                x.SetDisplayName(config.ServiceName);
                x.SetServiceName(config.ServiceName);
            });
        }
    }
}
