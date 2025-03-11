# HAComputerGateway

## 注意事项
服务依赖.NET8控制台运行时[下载地址](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

## 使用步骤
### 1.下载压缩包解压
### 2.修改目录下的appsetting.json配置文件
```
{
  "ServiceConfig": {
    "ServiceDescription": "HomeAssistant获取电脑相关信息和控制电脑的服务", //服务描述
    "ServiceName": "HAComputerGateway", //服务名称
    "MqttBroker": "192.168.100.1", //MQTT服务器地址
    "MqttPort": 1883, //MQTT服务器端口
    "MqttUserName": "admin", //MQTT用户名
    "MqttPassword": "111111", //MQTT密码
    "MqttClientId": "00001", //客户端ID,多台电脑请保持唯一
    "MqttTopic": "homeassistant/computergateway", //MQTT主题
    "ShutDownTopic": "shutdown", //关机主题名称
    "SystemInfoTopic": "systeminfo", //系统信息主题名称
    "ShutDownInstruction": "shutdown", //关机指令
    "TimeDelay": 10, //关机延时，单位秒
    "SystemInfoPushInterval": 10 //系统信息推送间隔，单位秒
  }
}
```
### 3.执行HAComputerGateway.exe文件打开控制台程序，如果运行正常会显示如下内容
![输入图片说明](https://private-user-images.githubusercontent.com/16460092/421225477-d633218a-58eb-4faf-9faf-912e61a784be.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NDE2NzU5NjcsIm5iZiI6MTc0MTY3NTY2NywicGF0aCI6Ii8xNjQ2MDA5Mi80MjEyMjU0NzctZDYzMzIxOGEtNThlYi00ZmFmLTlmYWYtOTEyZTYxYTc4NGJlLnBuZz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNTAzMTElMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjUwMzExVDA2NDc0N1omWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTlmNTIyZjA5ZmQ1Mjc3NjUzMzFmMGMzZmM5MzdkZTVmYjY0NmIyMmEzMDg0MjliZWI5MGUxZTFjZTY0NDg4YmImWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.1-hBH1nIcqBV_XHSLJlDzEsAx8PiVq8NWi6gHFaJPLE)
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
