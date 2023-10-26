using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Shades
{
    public class ShadeControllerConfigProperties
    {
        public List<ShadeConfig> Shades { get; set; }


        public class ShadeConfig
        {
            public string Key { get; set; }
        }
    }
}