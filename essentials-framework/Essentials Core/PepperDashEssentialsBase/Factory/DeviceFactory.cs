using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.Touchpanels;

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

            // Check for types that have been added by plugin dlls. 
            if (FactoryMethods.ContainsKey(typeName))
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading '{0}' from plugin", dc.Type);
                return FactoryMethods[typeName](dc);
            }

			// Check "core" types 
            //if (typeName == "genericcomm")
            //{
            //    Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            //    return new GenericComm(dc);
            //}
            if (typeName == "ceniodigin104")
            {
                var control = CommFactory.GetControlPropertiesConfig(dc);
                var ipid = control.IpIdInt;

                return new CenIoDigIn104Controller(key, name, new Crestron.SimplSharpPro.GeneralIO.CenIoDi104(ipid, Global.ControlSystem));
            }
		    if (typeName == "statussign")
		    {
		        var control = CommFactory.GetControlPropertiesConfig(dc);
		        var cresnetId = control.CresnetIdInt;

                return  new StatusSignController(key, name, new StatusSign(cresnetId, Global.ControlSystem));
		    }
		    if (typeName == "c2nrths")
		    {
		        var control = CommFactory.GetControlPropertiesConfig(dc);
		        var cresnetId = control.CresnetIdInt;

		        return new C2nRthsController(key, name, new C2nRths(cresnetId, Global.ControlSystem));
		    }

            return null;
        }

        /// <summary>
        /// Prints the type names fromt the FactoryMethods collection.
        /// </summary>
        /// <param name="command"></param>
        public static void GetDeviceFactoryTypes(string filter)
        {
            List<string> typeNames = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                typeNames = FactoryMethods.Keys.Where(k => k.Contains(filter)).ToList();
            }
            else
            {
                typeNames = FactoryMethods.Keys.ToList();
            }

            Debug.Console(0, "Device Types:");

            foreach (var type in typeNames)
            {
                Debug.Console(0, "type: '{0}'", type);
            }
        }
    }


    /// <summary>
    /// Responsible for loading all of the device types
    /// </summary>
    public class CoreDeviceFactory
    {
        public CoreDeviceFactory()
        {
            var genComm = new GenericCommFactory() as IDeviceFactory;
            genComm.LoadTypeFactories();
        }
    }
}