# HAComputerGateway

## MQTT中推送的电脑信息Json格式
```
{
  "MachineName": "xxx的电脑",
  "OSVersion": "Microsoft Windows 11 专业版",
  "Processor": "12th Gen Intel(R) Core(TM) i7-12700H",
  "CpuUsage": "0.56 %",
  "Memory": {
    "TotalMemory": "31.69 GB",
    "FreeMemory": "19.67 GB",
    "UsedMemory": "12.02 GB"
  },
  "Disks": [
    {
      "Name": "C",
      "TotalSize": "300.58 GB",
      "FreeSpace": "37.47 GB",
      "UsedSpace": "263.11 GB",
      "DriveType": "Fixed"
    },
    {
      "Name": "D",
      "TotalSize": "454.57 GB",
      "FreeSpace": "118.92 GB",
      "UsedSpace": "335.65 GB",
      "DriveType": "Fixed"
    },
    {
      "Name": "E",
      "TotalSize": "152.33 GB",
      "FreeSpace": "12.04 GB",
      "UsedSpace": "140.29 GB",
      "DriveType": "Fixed"
    }
  ],
  "Network": {
    "Ip": "192.168.6.22",
    "Mac": "xx:xx:xx:xx:xx:xx"
  }
}
```
