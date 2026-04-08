using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Shades;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades;

/// <summary>
/// Class that contains the shades to be controlled in a room
/// </summary>
public class ShadeController : EssentialsDevice, IShades
{
    ShadeControllerConfigProperties Config;

    /// <summary>
    /// List of shades to be controlled by this controller
    /// </summary>
    public List<IShadesOpenCloseStop> Shades { get; private set; }

    /// <summary>
    /// Constructor for ShadeController
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="config"></param>
    public ShadeController(string key, string name, ShadeControllerConfigProperties config)
        : base(key, name)
    {
        Config = config;

        Shades = new List<IShadesOpenCloseStop>();
    }

    /// <inheritdoc />
    protected override bool CustomActivate()
    {
        foreach (var shadeConfig in Config.Shades)
        {
            var shade = DeviceManager.GetDeviceForKey(shadeConfig.Key) as ShadeBase;

            if (shade != null)
            {
                AddShade(shade);
            }
        }
        return base.CustomActivate();
    }

    void AddShade(IShadesOpenCloseStop shade)
    {
        Shades.Add(shade);
    }
}

/// <summary>
/// Class representing the properties for the ShadeController device, including a list of shades to control
/// </summary>
public class ShadeControllerConfigProperties
{
    /// <summary>
    /// List of shades to control, represented by their unique keys
    /// </summary>
    public List<ShadeConfig> Shades { get; set; }

    /// <summary>
    /// Class representing the configuration for an individual shade, including its unique key
    /// </summary>
    public class ShadeConfig : IKeyed
    {
        /// <summary>
        /// The unique key of the shade device to be controlled
        /// </summary>
        public string Key { get; set; }
    }
}

/// <summary>
/// Factory for creating ShadeController devices
/// </summary>
public class ShadeControllerFactory : EssentialsDeviceFactory<ShadeController>
{
    /// <summary>
    /// Constructor for ShadeControllerFactory
    /// </summary>
    public ShadeControllerFactory()
    {
        TypeNames = new List<string>() { "shadecontroller" };
    }

    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new ShadeController Device");
        var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ShadeControllerConfigProperties>(dc.Properties.ToString());

        return new ShadeController(dc.Key, dc.Name, props);
    }
}