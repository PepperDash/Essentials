using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.Occupancy;

namespace PepperDash.Essentials.Devices.Common.PartitionSensor
{
	public class GlsPartitionSensorControllerFactory : EssentialsDeviceFactory<GlsPartitionSensorController>
	{
		public GlsPartitionSensorControllerFactory()
		{
			TypeNames = new List<string>() { "glspartcn" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(2, "Factory Attempting to create new GLS-PART-CN Device");

			var typeName = dc.Type.ToLower();
			var key = dc.Key;
			var name = dc.Name;
			var comm = CommFactory.GetControlPropertiesConfig(dc);
			if (comm == null)
			{
				Debug.Console(0, "ERROR: Control Properties Config are null");
				return null;
			}

			var sensor = new GlsPartCn(comm.CresnetIdInt, Global.ControlSystem);
			return new GlsPartitionSensorController(dc.Key, dc.Name, sensor);
		}
	}
}