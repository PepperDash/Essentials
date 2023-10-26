using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
	/// <summary>
	/// Represents and audio endpoint
	/// </summary>
	public class GenericAudioOut : EssentialsDevice, IRoutingSinkNoSwitching
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

		public RoutingInputPort AnyAudioIn { get; private set; }

		public GenericAudioOut(string key, string name)
			: base(key, name)
		{
			AnyAudioIn = new RoutingInputPort(RoutingPortNames.AnyAudioIn, eRoutingSignalType.Audio, 
				eRoutingPortConnectionType.LineAudio, null, this);
		}

		#region IRoutingInputs Members

		public RoutingPortCollection<RoutingInputPort> InputPorts
		{
			get { return new RoutingPortCollection<RoutingInputPort> { AnyAudioIn }; }
		}

		#endregion
	}
}