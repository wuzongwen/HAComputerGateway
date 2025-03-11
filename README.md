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
<img src="https://private-user-images.githubusercontent.com/16460092/421225477-d633218a-58eb-4faf-9faf-912e61a784be.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NDE2NzU5NjcsIm5iZiI6MTc0MTY3NTY2NywicGF0aCI6Ii8xNjQ2MDA5Mi80MjEyMjU0NzctZDYzMzIxOGEtNThlYi00ZmFmLTlmYWYtOTEyZTYxYTc4NGJlLnBuZz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNTAzMTElMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjUwMzExVDA2NDc0N1omWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTlmNTIyZjA5ZmQ1Mjc3NjUzMzFmMGMzZmM5MzdkZTVmYjY0NmIyMmEzMDg0MjliZWI5MGUxZTFjZTY0NDg4YmImWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.1-hBH1nIcqBV_XHSLJlDzEsAx8PiVq8NWi6gHFaJPLE" alt="图片描述" width="600" height="450" />

### 4.HACS中安装button-card

### 5.编辑configuration.yaml添加以下配置，这里的配置是单台电脑的，如果需要控制多台电脑，每一台电脑都要添加一组下面的配置
##### button用于控制电脑关机
##### sensor获取电脑的设备信息
##### switch控制电脑开机
这三个实体在下一步的button-card配置中会用到
```
mqtt:
  - button:
      unique_id: pc001_btn
      name: "pc001btn"   # 定义HA中实体的名称,可任意命名
      command_topic: "homeassistant/hacmputergateway/00001/shutdown"   # 发送关机指令的主题名称
      payload_press: "shutdown"   # 发送的关机指令
      qos: 0
      retain: false
      entity_category: "config"
      device_class: "restart"
  - sensor:
      name: "pc001sensor"    # 定义HA中实体的名称,可任意命名
      state_topic: "homeassistant/hacmputergateway/00001/systeminfo"   # 订阅系统信息的主题名称
      value_template: "{{ value_json.MachineName }}"
      json_attributes_topic: "homeassistant/hacmputergateway/00001/systeminfo"   # 订阅系统信息的主题名称
      json_attributes_template: "{{ value_json | tojson }}"
switch:
  - platform: wake_on_lan
    name: "pc001switch"                 # 定义HA中实体的名称,可任意命名
    mac: "04:42:1a:ec:a5:8f"        # 主机(电脑)的MAC地址
    host: "192.168.100.10"            # 主机(电脑)地址,可省略
    broadcast_address: "192.168.100.255"      # 广播地址.不可省略.此处假设路由器地址为192.168.1.1,如为其他网段需要修改
    broadcast_port: 9               # 止定wol端口,可省略
```

### 6.HomeAsisstant的仪表盘中添加自定义卡片
```
type: vertical-stack
cards:
  - type: custom:button-card
    entity: switch.pc001switch
    name: 家里电脑
    icon: mdi:desktop-tower-monitor
    show_name: true
    show_icon: true
    tap_action:
      action: call-service
      service: >
        [[[ return entity.state === 'off' ? 'switch.turn_on' : 'button.press';
        ]]]
      service_data:
        entity_id: >
          [[[ return entity.state === 'off' ? 'switch.jia_li_dian_nao' :
          'button.pc001btn'; ]]]
      confirmation:
        text: |
          [[[ return entity.state === 'off' ? '确定要开机吗？' : '确定要关机吗？'; ]]]
  - type: conditional
    conditions:
      - entity: switch.pc001switch
        state: "on"
    card:
      type: vertical-stack
      cards:
        - type: custom:button-card
          entity: sensor.pc001sensor
          show_name: false
          show_icon: false
          show_label: true
          styles:
            card:
              - padding: 16px
            label:
              - text-align: left
          label: |
            [[[ 
              const attr = entity.attributes;
              if (attr.Status === 0) return "设备关机中";
              let htmlOutput = `
                <div style="display: flex; flex-direction: column; white-space: normal; word-wrap: break-word;">
                  <div>
                    <ha-icon icon="mdi:desktop-classic" style="margin-right:6px; max-width:20px;"></ha-icon>
                    电脑名称: ${attr.MachineName || "N/A"}
                  </div>
                  <div>
                    <ha-icon icon="mdi:microsoft-windows" style="margin-right:6px; max-width:20px;"></ha-icon>
                    系统版本: ${attr.OSVersion || "N/A"}
                  </div>
                  <div>
                    <ha-icon icon="mdi:cpu-64-bit" style="margin-right:6px; max-width:20px;"></ha-icon>
                    处理器: ${attr.Processor || "N/A"}
                  </div>
                  <div>
                    <ha-icon icon="mdi:memory" style="margin-right:6px; max-width:20px;"></ha-icon>
                    内存: ${attr.Memory?.UsedMemory || "N/A"}/${attr.Memory?.TotalMemory || "N/A"}
                  </div>
              `;
              if (attr.Disks && Array.isArray(attr.Disks)) {
                htmlOutput += `<div>`;
                htmlOutput += attr.Disks.map(disk => {
                   return `<div style="margin-top:4px;">
                             <ha-icon icon="mdi:harddisk" style="margin-right:6px; max-width:20px;"></ha-icon>
                             ${disk.Name || "N/A"}盘：${disk.UsedSpace || "Unknown"} / ${disk.TotalSize || "Unknown"}
                           </div>`;
                }).join('');
                htmlOutput += `</div>`;
              }
              htmlOutput += `</div>`;
              return htmlOutput;
            ]]]

```

### 7.如果一切正常，到这里卡片就可以正常显示出来了，点击卡片可以控制电脑开机关机，关机状态下不显示设备信息
<img src="https://github.com/user-attachments/assets/e3a123a0-81d3-4774-9523-774643af5b09" alt="图片描述" width="250" height="450" />
<img src="https://github.com/user-attachments/assets/14217cc7-0998-4940-9bea-70f9ce70407c" alt="图片描述" width="250" height="450" />
<img src="https://github.com/user-attachments/assets/b20c8727-ac46-4075-8261-9fd3f49623d6" alt="图片描述" width="250" height="450" />

### 8.执行目录下的install.bat文件安装服务，后面就会以系统服务的形式运行了
修改配置后可以重启下服务，如果不知道怎么重启也可以执行uninstall.bat卸载服务后再安装即可

# 扩展
## MQTT中推送的电脑信息Json格式,如果HA大佬可以开发卡片的话，看看是否有用
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
