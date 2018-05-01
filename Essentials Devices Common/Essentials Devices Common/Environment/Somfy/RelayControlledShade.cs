using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.Shades;

namespace PepperDash.Essentials.Devices.Common.Environment.Somfy
{
    public class RelayControlledShade : ShadeBase
    {

        public RelayControlledShade(string key, string name, RelayControlledShadeConfigProperties props)
            : base(key, name)
        {

        }


        public void Raise()
        {

        }

    }

    public class RelayControlledShadeConfigProperties
    {

    }
}