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
    public abstract class EssentialsRoomEmergencyBase : IKeyed
    {
        public string Key { get; private set; }

        public EssentialsRoomEmergencyBase(string key)
        {
            Key = key;
        }
    }


    public class EssentialsRoomEmergencyContactClosure : EssentialsRoomEmergencyBase
    {
        EssentialsRoomBase Room;
        string Behavior;
        bool TriggerOnClose;

        public EssentialsRoomEmergencyContactClosure(string key, EssentialsRoomEmergencyConfig config, EssentialsRoomBase room) :
            base(key)
        {
            Room = room;
            var cs = Global.ControlSystem;

            if (config.Trigger.Type.Equals("contact", StringComparison.OrdinalIgnoreCase))
            {
                var portNum = (uint)config.Trigger.Number;
                if (portNum <= cs.NumberOfDigitalInputPorts)
                {
                    cs.DigitalInputPorts[portNum].Register();
                    cs.DigitalInputPorts[portNum].StateChange += EsentialsRoomEmergencyContactClosure_StateChange;
                }
            }
            Behavior = config.Behavior;
            TriggerOnClose = config.Trigger.TriggerOnClose;
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