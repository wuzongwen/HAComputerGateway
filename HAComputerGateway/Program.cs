using HAComputerGateway.Core.Configs;
using HAComputerGateway.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Text.Json;
using Topshelf;

namespace HAComputerGateway
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main()
        {
            try
            {
                // 使用 ConfigurationBuilder 加载 INI 配置
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // 将配置绑定到模型对象
                var serviceConfig = new AppSetting();
                configuration.GetSection("ServiceConfig").Bind(serviceConfig);

                // 设置依赖注入
                var services = new ServiceCollection();
                services.AddSingleton(serviceConfig);
                services.AddSingleton<HAComputerGatewayService>();

                var serviceProvider = services.BuildServiceProvider();

                HostFactory.Run(x =>
                {
                    x.Service<HAComputerGatewayService>(s =>
                    {
                        s.ConstructUsing(name => serviceProvider.GetService<HAComputerGatewayService>());
                        s.WhenStarted(service => service.Start());
                        s.WhenStopped(service => service.Stop());
                    });

                    x.RunAsLocalSystem();
                    x.SetServiceName(serviceConfig.ServiceName);
                    x.SetDisplayName(serviceConfig.ServiceName);
                    x.SetDescription(serviceConfig.ServiceDescription);
                });
            }
            catch (Exception ex)
            {
                Logger.Info(JsonSerializer.Serialize(ex));
            }
        }
    }
}
