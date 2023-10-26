extern alias Full;

using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;
using PepperDash.Essentials.Devices.Common.VideoCodec.CiscoCodec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    // Helper Classes for Proerties
    public abstract class ValueProperty
    {
        /// <summary>
        /// Triggered when Value is set
        /// </summary>
        public Action ValueChangedAction { get; set; }

        protected void OnValueChanged()
        {
            var a = ValueChangedAction;
            if (a != null)
                a();
        }

    }
}
