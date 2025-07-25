using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Routing
{
 /// <summary>
 /// Represents a DummyRoutingInputsDevice
 /// </summary>
	public class DummyRoutingInputsDevice : Device, IRoutingSource, IRoutingOutputs
	{
  /// <summary>
  /// Gets or sets the AudioVideoOutputPort
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