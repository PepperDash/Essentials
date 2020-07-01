using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

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
			
			var comm = CommFactory.GetControlPropertiesConfig(dc);
			if (comm == null)
			{
				Debug.Console(0, "ERROR: Control Properties Config for {0} is null", dc.Key);
				return null;
			}

			var sensor = new GlsPartCn(comm.CresnetIdInt, Global.ControlSystem);
			return new GlsPartitionSensorController(dc.Key, dc.Name, sensor);
		}
	}
}