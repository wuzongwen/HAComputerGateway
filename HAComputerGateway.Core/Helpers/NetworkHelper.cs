using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HAComputerGateway.Core.Helpers
{
    public static class NetworkHelper
    {
        /// <summary>
        /// 检查本机是否有网络连接，并获取到有效的IPv4地址及MAC地址
        /// </summary>
        /// <returns>如果有网络且能获取到IP、MAC，则返回true，否则返回false</returns>
        public static bool IsNetworkAvailable(out IPAddress localIp, out string macAddress)
        {
            localIp = null;
            macAddress = string.Empty;

            // 首先判断网络连接状态
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            try
            {
                // 获取本机所有IPv4地址
                var host = Dns.GetHostEntry(Dns.GetHostName());
                localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
                if (localIp == null)
                    return false;
                // 通过遍历所有网络接口，查找包含该 IP 的接口，从而获取 MAC 地址
                // 将 localIp 复制到局部变量，以便在 lambda 表达式中使用
                var ipForLambda = localIp;
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n =>
                        n.OperationalStatus == OperationalStatus.Up &&
                        n.GetIPProperties().UnicastAddresses.Any(ua => ua.Address.Equals(ipForLambda))
                    );

                if (nic != null)
                {
                    var macForLambda = nic.GetPhysicalAddress().ToString();
                    macAddress = string.Join(":", Enumerable.Range(0, macForLambda.Length / 2)
                                .Select(i => macForLambda.Substring(i * 2, 2)));
                }
                else
                {
                    macAddress = "Unknown";
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
