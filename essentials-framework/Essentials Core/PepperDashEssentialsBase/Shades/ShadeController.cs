using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Class that contains the shades to be controlled in a room
    /// </summary>
    public class ShadeController : EssentialsDevice, IShades
    {
        ShadeControllerConfigProperties Config;

        public List<ShadeBase> Shades { get; private set; }

        public ShadeController(string key, string name, ShadeControllerConfigProperties config)
            : base(key, name)
        {
            Config = config;

            Shades = new List<ShadeBase>();
        }

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

        void AddShade(ShadeBase shade)
        {
            Shades.Add(shade);
        }
    }

    public class ShadeControllerConfigProperties
    {
        public List<ShadeConfig> Shades { get; set; }


        public class ShadeConfig
        {
            public string Key { get; set; }
        }
    }

    public class ShadeControllerFactory : EssentialsDeviceFactory<ShadeController>
    {
        public ShadeControllerFactory()
        {
            TypeNames = new List<string>() { "shadecontroller" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ShadeController Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Shades.ShadeControllerConfigProperties>(dc.Properties.ToString());

            return new Core.Shades.ShadeController(dc.Key, dc.Name, props);
        }
    }

}