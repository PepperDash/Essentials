using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Shades;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Class that contains the shades to be controlled in a room
    /// </summary>
    public class ShadeController : EssentialsDevice, IShades
    {
        ShadeControllerConfigProperties Config;

        public List<IShadesOpenCloseStop> Shades { get; private set; }

        public ShadeController(string key, string name, ShadeControllerConfigProperties config)
            : base(key, name)
        {
            Config = config;

            Shades = new List<IShadesOpenCloseStop>();
        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
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
    /// Represents a ShadeControllerConfigProperties
    /// </summary>
    public class ShadeControllerConfigProperties
    {
        /// <summary>
        /// Gets or sets the Shades
        /// </summary>
        public List<ShadeConfig> Shades { get; set; }


        /// <summary>
        /// Represents a ShadeConfig
        /// </summary>
        public class ShadeConfig
        {
            /// <summary>
            /// Gets or sets the Key
            /// </summary>
            public string Key { get; set; }
        }
    }

    /// <summary>
    /// Represents a ShadeControllerFactory
    /// </summary>
    public class ShadeControllerFactory : EssentialsDeviceFactory<ShadeController>
    {
        public ShadeControllerFactory()
        {
            TypeNames = new List<string>() { "shadecontroller" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new ShadeController Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ShadeControllerConfigProperties>(dc.Properties.ToString());

            return new ShadeController(dc.Key, dc.Name, props);
        }
    }

}