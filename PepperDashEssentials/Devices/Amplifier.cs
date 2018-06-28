using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials
{
    public class Amplifier : Device, IRoutingSinkNoSwitching
    {
        public RoutingInputPort AudioIn { get; private set; }

        public Amplifier(string key, string name)
            : base(key, name)
        {
            AudioIn = new RoutingInputPort(RoutingPortNames.AnyAudioIn, eRoutingSignalType.Audio,
                eRoutingPortConnectionType.None, null, this);
            InputPorts = new RoutingPortCollection<RoutingInputPort> { AudioIn };
        }

        #region IRoutingInputs Members

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        #endregion
    }
}