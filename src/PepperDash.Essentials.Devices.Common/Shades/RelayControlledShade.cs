using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Shades;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades;

/// <summary>
/// Controls a single shade using three relays
/// </summary>
public class RelayControlledShade : ShadeBase, IShadesOpenCloseStop
{
    RelayControlledShadeConfigProperties Config;

    ISwitchedOutput OpenRelay;
    ISwitchedOutput StopOrPresetRelay;
    ISwitchedOutput CloseRelay;

    int RelayPulseTime;

    /// <summary>
    /// Gets the label for the stop or preset button, depending on how the shade is configured
    /// </summary>
    public string StopOrPresetButtonLabel { get; set; }

    /// <summary>
    /// Constructor for RelayControlledShade
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="config"></param>
    public RelayControlledShade(string key, string name, RelayControlledShadeConfigProperties config)
        : base(key, name)
    {
        Config = config;

        RelayPulseTime = Config.RelayPulseTime;

        StopOrPresetButtonLabel = Config.StopOrPresetLabel;

    }

    /// <inheritdoc />
    public override bool CustomActivate()
    {
        //Create ISwitchedOutput objects based on props
        OpenRelay = GetSwitchedOutputFromDevice(Config.Relays.Open);
        StopOrPresetRelay = GetSwitchedOutputFromDevice(Config.Relays.StopOrPreset);
        CloseRelay = GetSwitchedOutputFromDevice(Config.Relays.Close);


        return base.CustomActivate();
    }

    /// <inheritdoc />
    public override void Open()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Opening Shade: '{0}'", this.Name);

        PulseOutput(OpenRelay, RelayPulseTime);
    }

    /// <inheritdoc />

    public override void Stop()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Shade: '{0}'", this.Name);

        PulseOutput(StopOrPresetRelay, RelayPulseTime);
    }

    /// <inheritdoc />
    public override void Close()
    {
        Debug.LogMessage(LogEventLevel.Debug, this, "Closing Shade: '{0}'", this.Name);

        PulseOutput(CloseRelay, RelayPulseTime);
    }

    void PulseOutput(ISwitchedOutput output, int pulseTime)
    {
        output.On();
        CTimer pulseTimer = new CTimer(new CTimerCallbackFunction((o) => output.Off()), pulseTime);
    }

    /// <summary>
    /// Attempts to get the port on the specified device from config
    /// </summary>
    /// <param name="relayConfig"></param>
    /// <returns></returns>
    ISwitchedOutput GetSwitchedOutputFromDevice(IOPortConfig relayConfig)
    {
        var portDevice = DeviceManager.GetDeviceForKey(relayConfig.PortDeviceKey);

        if (portDevice != null)
        {
            return (portDevice as ISwitchedOutputCollection).SwitchedOutputs[relayConfig.PortNumber];
        }
        else
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Error: Unable to get relay on port '{0}' from device with key '{1}'", relayConfig.PortNumber, relayConfig.PortDeviceKey);
            return null;
        }
    }

}

/// <summary>
/// Configuration properties for RelayControlledShade
/// </summary>
public class RelayControlledShadeConfigProperties
{
    /// <summary>
    /// The amount of time in milliseconds to pulse the relay for when opening or closing the shade
    /// </summary>
    public int RelayPulseTime { get; set; }

    /// <summary>
    /// The relays that control the shade
    /// </summary>
    public ShadeRelaysConfig Relays { get; set; }

    /// <summary>
    /// The label for the stop or preset button, depending on how the shade is configured
    /// </summary>
    public string StopOrPresetLabel { get; set; }

    /// <summary>
    /// Configuration for the relays that control the shade
    /// </summary>
    public class ShadeRelaysConfig
    {
        /// <summary>
        /// The relay that opens the shade
        /// </summary>
        public IOPortConfig Open { get; set; }

        /// <summary>
        /// The relay that stops the shade or presets the shade to a certain position, depending on how the shade is configured
        /// </summary>
        public IOPortConfig StopOrPreset { get; set; }

        /// <summary>
        /// The relay that closes the shade
        /// </summary>
        public IOPortConfig Close { get; set; }
    }
}

/// <summary>
/// Factory for creating RelayControlledShade devices
/// </summary>
public class RelayControlledShadeFactory : EssentialsDeviceFactory<RelayControlledShade>
{
    /// <summary>
    /// Constructor for RelayControlledShadeFactory
    /// </summary>
    public RelayControlledShadeFactory()
    {
        TypeNames = new List<string>() { "relaycontrolledshade" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
        var props = Newtonsoft.Json.JsonConvert.DeserializeObject<RelayControlledShadeConfigProperties>(dc.Properties.ToString());

        return new RelayControlledShade(dc.Key, dc.Name, props);
    }
}