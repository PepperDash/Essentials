using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM.Chassis
{
	public class HdMdNxM4kEController : Device, IRoutingInputsOutputs, IRouting
	{
		public HdMdNxM Chassis { get; private set; }

		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }




		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="chassis"></param>
		public HdMdNxM4kEController(string key, string name, HdMdNxM chassis, 
			HdMdNxM4kEPropertiesConfig props)
			: base(key, name)
		{
			Chassis = chassis;

			// logical ports
			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			for (uint i = 1; i <= 4; i++)
			{
				InputPorts.Add(new RoutingInputPort("hdmiIn" + i, eRoutingSignalType.AudioVideo,
					eRoutingPortConnectionType.Hdmi, i, this));
			}
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
			OutputPorts.Add(new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, null, this));

			// physical settings
			if (props != null && props.Inputs != null)
			{
				foreach (var kvp in props.Inputs)
				{
					var inputNum = Convert.ToUInt32(kvp.Key);

					var port = chassis.HdmiInputs[inputNum].HdmiInputPort;
					// set hdcp disables
					if (kvp.Value.DisableHdcp)
						port.HdcpSupportOff();
					else
						port.HdcpSupportOn();
				}
			}
		}

		public override bool CustomActivate()
		{
			var result = Chassis.Register();
			if (result != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Debug.Console(0, this, "Device registration failed: {0}", result);
				return false;
			}

			return base.CustomActivate();
		}



		#region IRouting Members

		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
		{
			Chassis.HdmiOutputs[1].VideoOut = Chassis.HdmiInputs[(uint)inputSelector];
		}

		#endregion

		/////////////////////////////////////////////////////

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="properties"></param>
		/// <returns></returns>
		public static HdMdNxM4kEController GetController(string key, string name,
			string type, HdMdNxM4kEPropertiesConfig properties)
		{
			try
			{
				var ipid = properties.Control.IpIdInt;
				var address = properties.Control.TcpSshProperties.Address;

				type = type.ToLower();
				if (type == "hdmd4x14ke")
				{
					var chassis = new HdMd4x14kE(ipid, address, Global.ControlSystem);
					return new HdMdNxM4kEController(key, name, chassis, properties);
				}
				return null;
			}
			catch (Exception e)
			{
				Debug.Console(0, "ERROR Creating device key {0}: \r{1}", key, e);
				return null;
			}
		}
	}
}