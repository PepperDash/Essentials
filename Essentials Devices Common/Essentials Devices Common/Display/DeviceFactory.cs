using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Displays
{
	public class DisplayDeviceFactory
	{
		public static IKeyed GetDevice(DeviceConfig dc)
		{
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;

			var typeName = dc.Type.ToLower();

			try
			{
				if (typeName == "necmpsx")
				{
					var comm = CommFactory.CreateCommForDevice(dc);
					if (comm != null)
						return new NecPSXMDisplay(dc.Key, dc.Name, comm);
				}
                else if(typeName == "samsungmdc")
                {
                    var comm = CommFactory.CreateCommForDevice(dc);
                    if (comm != null)
                        return new SamsungMDC(dc.Key, dc.Name, comm, dc.Properties["id"].Value<string>());
                }
                if (typeName == "avocorvtf")
                {
                    var comm = CommFactory.CreateCommForDevice(dc);
                    if (comm != null)
                        return new AvocorDisplay(dc.Key, dc.Name, comm, null);
                }
                   
			}
			catch (Exception e)
			{
				Debug.Console(0, "Displays factory: Exception creating device type {0}, key {1}: \nCONFIG JSON: {2} \nERROR: {3}\n\n", 
                    dc.Type, dc.Key, JsonConvert.SerializeObject(dc), e);
				return null;
			}

			return null;
		}
	}
}