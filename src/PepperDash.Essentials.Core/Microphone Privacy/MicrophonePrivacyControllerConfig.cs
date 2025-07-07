using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Privacy;

public class MicrophonePrivacyControllerConfig
{
    public List<KeyedDevice> Inputs { get; set; }
    public KeyedDevice GreenLedRelay { get; set; }
    public KeyedDevice RedLedRelay { get; set; }
}

public class KeyedDevice
{
    public string DeviceKey { get; set; }
}