using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Shades
{
    public class ShadeController : Device, IShades
    {
        public List<ShadeBase> Shades { get; private set; }

        public ShadeController(string key, string name)
            : base(key, name)
        {
            Shades = new List<ShadeBase>();
        }

    }

}