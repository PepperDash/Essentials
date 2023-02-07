using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials
{
    public class Amplifier : EssentialsDevice, IRoutingSinkNoSwitching
    {
        public event SourceInfoChangeHandler CurrentSourceChange;

        public string CurrentSourceInfoKey { get; set; }
        public SourceListItem CurrentSourceInfo
        {
            get
            {
                return _CurrentSourceInfo;
            }
            set
            {
                if (value == _CurrentSourceInfo) return;

                var handler = CurrentSourceChange;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.WillChange);

                _CurrentSourceInfo = value;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.DidChange);
            }
        }
        SourceListItem _CurrentSourceInfo;

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

    public class AmplifierFactory : EssentialsDeviceFactory<Amplifier>
    {
        public AmplifierFactory()
        {
            TypeNames = new List<string>() { "amplifier" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Amplifier Device");
            return new Amplifier(dc.Key, dc.Name);
        }
    }
}