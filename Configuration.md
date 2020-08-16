# Configuration

## AppSettings.json

### Application Options
#### IPMI Tool Location
The location of the Dell IPMI Tool, this should include the exe.

### IMPI Options
#### Username
The username to connect to the Dell iDrac Interaction
#### Password
The user password to connect to the Dell iDrac Interaction
#### Host
The iDrac Interaction URL

### Example
``` JSON
{
  "ApplicationOptions": {
    "IPMIToolLocation": "C:\\Program Files (x86)\\Dell\\SysMgt\\bmc\\ipmitool.exe"
  },
  "IMPIOptions": {
    "Username": "FanControlUser",
    "Password": "Password",
    "Host":  "192.168.254.27"
  }
}
```

## Fan Levels
This config can be reload as the service is running.
If the service detects the file has been changed then the service will automaticly reload the file.

### Other
#### Minimum Time Before Dropping
How long the service will waiting before allowing a lower fan level to be set (In Seconds)

#### Temperture Measure
How the service should measure the tempture of the server. 
Available Options
* Highest - Uses the highest temperture reading
* Average - Averages all the temperture readings

### Levels
Set what the fan speed should be depending on temperture of the server. 
#### Low
The lower temperture bound
#### High
The high temperture bound
#### Speed
The power percentage that the fan should be set to. **Not the Fan RPM**

### Example
``` JSON
{
  "MinimumTimeBeforeDropping": 300,
  "TempertureMeasure": "Highest",
  "Levels": [
    {
      "Low": 0,
      "High": 30,
      "Speed": 10
    },
    {
      "Low": 30,
      "High": 50,
      "Speed": 20
    },
    {
      "Low": 50,
      "High": 60,
      "Speed": 30
    }
  ]
}
```

