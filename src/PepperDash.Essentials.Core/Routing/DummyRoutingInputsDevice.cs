using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Routing
{
	public class DummyRoutingInputsDevice : Device, IRoutingSource, IRoutingOutputs
	{
		/// <summary>
		/// A single output port, backplane, audioVideo
		/// </summary>
		public RoutingOutputPort AudioVideoOutputPort { get; private set; }

		/// <summary>
		/// contains the output port
		/// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts
		{
			get { return new RoutingPortCollection<RoutingOutputPort>() { AudioVideoOutputPort }; }
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="key">key for special device</param>
		public DummyRoutingInputsDevice(string key) : base(key)
		{
			AudioVideoOutputPort = new RoutingOutputPort("internal", eRoutingSignalType.Audio | eRoutingSignalType.Video, eRoutingPortConnectionType.BackplaneOnly,
				null, this, true);
		}
	}
}