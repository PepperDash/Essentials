using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
    public class DeviceFactory
    {
		/// <summary>
		/// A dictionary of factory methods, keyed by config types, added by plugins.
		/// These methods are looked up and called by GetDevice in this class.
		/// </summary>
		static Dictionary<string, Func<DeviceConfig, IKeyed>> FactoryMethods =
			new Dictionary<string, Func<DeviceConfig, IKeyed>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Adds a plugin factory method
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
		public static void AddFactoryForType(string type, Func<DeviceConfig, IKeyed> method) 
		{
			Debug.Console(0, Debug.ErrorLogLevel.Notice, "Adding factory method for type '{0}'", type);
			DeviceFactory.FactoryMethods.Add(type, method);
		}

		/// <summary>
		/// The factory method for Core "things". Also iterates the Factory methods that have
		/// been loaded from plugins
		/// </summary>
		/// <param name="dc"></param>
		/// <returns></returns>
        public static IKeyed GetDevice(DeviceConfig dc)
        {
            var key = dc.Key;
            var name = dc.Name;
            var type = dc.Type;
            var properties = dc.Properties;	

            var typeName = dc.Type.ToLower();

			// Check "core" types first
            if (typeName == "genericcomm")
            {
                Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
                return new GenericComm(dc);
            }
            else if (typeName == "ceniodigin104")
            {
                var control = CommFactory.GetControlPropertiesConfig(dc);
                var ipid = control.CresnetIdInt;

                return new CenIoDigIn104Controller(key, name, new Crestron.SimplSharpPro.GeneralIO.CenIoDi104(ipid, Global.ControlSystem));
            }

			// then check for types that have been added by plugin dlls. 
			if (FactoryMethods.ContainsKey(typeName))
			{
				Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading '{0}' from plugin", dc.Type);
				return FactoryMethods[typeName](dc);
			}

            return null;
        }
    }
}