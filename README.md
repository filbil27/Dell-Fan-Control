# Dell Fan Control
A .Net Core Service that manually controls the speeds of fans on Dell PowerEdge Servers

### Prerequisites

```
.Net Core 3.1
```

### Installing

To install as a windows service run the sc create command
```
sc.exe create DellFanControl binpath= "<exe Location>\Dell Fan Control.exe" start= delayed-auto DisplayName= "Dell Fan Control Depend= Dhcp/Dnscache"
```

To set failure options run the following command
```
sc.exe failure DellFanControl reset= 86400 actions= restart/1000/restart/1000/restart/1000 reset= 86400
```

## Configuration
The following configuration files need to be strored in the folder (C:\ProgramData\Dell Fan Control\Configuration)

* AppSettings.json
* FanLevels.json

For more information about the settings in the configuration files please view the wiki.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
