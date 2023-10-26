using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
}