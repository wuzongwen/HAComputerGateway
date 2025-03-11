using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HAComputerGateway.Core.Helpers
{
    public class SystemInfoHelper
    {
        /// <summary>
        /// 获取系统关键信息，并返回 JSON 格式字符串
        /// </summary>
        public static string GetSystemInfo()
        {
            try
            {
                // 1. 计算机名和系统版本
                string machineName = Regex.Unescape(Environment.MachineName);
                string osVersion = GetWmiOSCaption();

                // 2. 处理器信息，通过 WMI 查询 Win32_Processor
                float cpuUsage = GetCpuUsage();
                string processorName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"); // 备用
                try
                {
                    using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            processorName = item["Name"]?.ToString() ?? processorName;
                            break; // 只取第一个处理器的信息
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 如果 WMI 查询失败，则使用环境变量
                    processorName = processorName + " (WMI query failed: " + ex.Message + ")";
                }

                // 3. 内存信息，通过 WMI 查询 Win32_OperatingSystem
                string totalMemory = "Unknown";
                string freeMemory = "Unknown";
                string usedMemory = "Unknown";
                try
                {
                    using (var searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize, FreePhysicalMemory from Win32_OperatingSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            ulong totalKb = Convert.ToUInt64(item["TotalVisibleMemorySize"]);
                            ulong freeKb = Convert.ToUInt64(item["FreePhysicalMemory"]);
                            totalMemory = (totalKb / 1024.0 / 1024.0).ToString("F2") + " GB";
                            freeMemory = (freeKb / 1024.0 / 1024.0).ToString("F2") + " GB";
                            double usedKb = totalKb - freeKb;
                            usedMemory = (usedKb / 1024.0 / 1024.0).ToString("F2") + " GB";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    totalMemory = "Error: " + ex.Message;
                    freeMemory = "Error: " + ex.Message;
                    usedMemory = "Error: " + ex.Message;
                }

                // 4. 磁盘信息，获取所有已准备好的盘
                var disks = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Select(d => new {
                        d.Name,
                        TotalSize = (d.TotalSize / (1024 * 1024 * 1024)) + " GB",
                        FreeSpace = (d.TotalFreeSpace / (1024 * 1024 * 1024)) + " GB",
                        DriveType = d.DriveType.ToString()
                    });

                // 5. 网络信息，获取所有网络接口信息
                var networks = new { Ip = "-", Mac = "-" };
                if (NetworkHelper.IsNetworkAvailable(out IPAddress ip, out string mac))
                {
                    networks = new { Ip = ip.ToString(), Mac = mac };
                }

                // 组合所有信息到一个对象中
                var sysInfo = new
                {
                    MachineName = machineName,
                    OSVersion = osVersion,
                    Processor = processorName,
                    CpuUsage = cpuUsage.ToString("F2") + " %",
                    Memory = new
                    {
                        TotalMemory = totalMemory,
                        FreeMemory = freeMemory,
                        UsedMemory = usedMemory
                    },
                    Disks = disks,
                    Network = networks
                };

                // 使用 System.Text.Json 序列化（启用格式化输出）
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                return JsonSerializer.Serialize(sysInfo, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取系统信息失败：" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取系统版本
        /// </summary>
        /// <returns></returns>
        public static string GetWmiOSCaption()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        return os["Caption"]?.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                return "Error: " + ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取 CPU 使用率
        /// </summary>
        public static float GetCpuUsage()
        {
            try
            {
                using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    // 必须先调用一次 NextValue() 初始化计数器值
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(500); // 等待以获取准确的使用率
                    return cpuCounter.NextValue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取 CPU 使用率失败：" + ex.Message);
                return -1; // 返回 -1 表示错误
            }
        }
    }
}
