using System;

namespace PepperDash.Essentials.Core.DeviceInfo;

public class DeviceInfoEventArgs:EventArgs
{
    public DeviceInfo DeviceInfo { get; set; }

    public DeviceInfoEventArgs()
    {
        
    }

    public DeviceInfoEventArgs(DeviceInfo devInfo)
    {
        DeviceInfo = devInfo;
    }
}