using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.Room
{	
    public class EssentialsRoomEmergencyContactClosure : EssentialsRoomEmergencyBase
    {
        IEssentialsRoom Room;
        string Behavior;
        bool TriggerOnClose;

		// TODO [ ] Issue #1071 - Emergency
        public EssentialsRoomEmergencyContactClosure(string key, EssentialsRoomEmergencyConfig config, IEssentialsRoom room) :
            base(key)
        {
			if (config == null || room == null) return;

            Room = room;
			Behavior = config.Behavior;
			TriggerOnClose = config.Trigger.TriggerOnClose; 
			
			var cs = Global.ControlSystem;

			Debug.Console(0, this, "Control system supports digital inputs {0}", cs.SupportsDigitalInput);
			Debug.Console(0, this, "Control system supports versiports {0}", cs.SupportsVersiport);

			Debug.Console(0, this, "------> type check");

	        if (string.IsNullOrEmpty(config.Trigger.Type))
	        {
				Debug.Console(0, this, "Emergency Contact Closure type is empty or null");
		        return;
	        }

	        if (!config.Trigger.Type.Equals("contact", StringComparison.OrdinalIgnoreCase))
	        {
				Debug.Console(0, this, "Emergency Contact Closure type is not 'contact'");
				return;
	        }

			Debug.Console(0, this, "------> type passed");
			Debug.Console(0, this, "------> portNum check");

			var portNum = (uint)config.Trigger.Number;
	        if (portNum > cs.NumberOfDigitalInputPorts) return;

			Debug.Console(0, this, "------> portNum passed");
			Debug.Console(0, this, "------> port check");

			var port = cs.DigitalInputPorts[portNum];
	        if (port == null)
	        {
		        Debug.Console(0, this, "Control system does not support digital inputs");
				return;
	        }

			Debug.Console(0, this, "------> port passed");
			Debug.Console(0, this, "------> port register check");

			port.Register();

			Debug.Console(0, this, "------> port register passed");
			Debug.Console(0, this, "------> port event check");

	        port.StateChange += EsentialsRoomEmergencyContactClosure_StateChange;

			Debug.Console(0, this, "------> port event passed");
        }

        void EsentialsRoomEmergencyContactClosure_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            if (args.State && TriggerOnClose || !args.State && !TriggerOnClose)
                RunEmergencyBehavior();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RunEmergencyBehavior()
        {
            if (Behavior.Equals("shutdown"))
                Room.Shutdown();
        }
    }
}