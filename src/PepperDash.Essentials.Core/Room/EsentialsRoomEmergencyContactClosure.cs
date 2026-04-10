using System;
using Crestron.SimplSharpPro;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a EssentialsRoomEmergencyContactClosure
    /// </summary>
    public class EssentialsRoomEmergencyContactClosure : EssentialsRoomEmergencyBase, IEssentialsRoomEmergency
    {
        /// <summary>
        /// Event fired when emergency state changes
        /// </summary>
        public event EventHandler<EventArgs> EmergencyStateChange;

        IEssentialsRoom Room;
        string Behavior;
        bool TriggerOnClose;

        /// <summary>
        /// Gets or sets the InEmergency
        /// </summary>
        public bool InEmergency { get; private set; }

        /// <summary>
        /// Constructor for EssentialsRoomEmergencyContactClosure
        /// </summary>
        /// <param name="key">device key</param>
        /// <param name="config">emergency device config</param>
        /// <param name="room">the room associated with this emergency contact closure</param>
        public EssentialsRoomEmergencyContactClosure(string key, EssentialsRoomEmergencyConfig config, IEssentialsRoom room) :
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
            else if (config.Trigger.Type.Equals("versiport", StringComparison.OrdinalIgnoreCase))
            {
                var portNum = (uint)config.Trigger.Number;
                if (portNum <= cs.NumberOfVersiPorts)
                {
                    cs.VersiPorts[portNum].Register();
                    cs.VersiPorts[portNum].SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                    cs.VersiPorts[portNum].DisablePullUpResistor = true;
                    cs.VersiPorts[portNum].VersiportChange += EssentialsRoomEmergencyContactClosure_VersiportChange;
                }
            }
            Behavior = config.Behavior;
            TriggerOnClose = config.Trigger.TriggerOnClose;
        }

        private void EssentialsRoomEmergencyContactClosure_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            if (args.Event == eVersiportEvent.DigitalInChange)
            {
                ContactClosure_StateChange(port.DigitalIn);
            }
        }

        void EsentialsRoomEmergencyContactClosure_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            ContactClosure_StateChange(args.State);
        }

        void ContactClosure_StateChange(bool portState)
        {
            if (portState && TriggerOnClose || !portState && !TriggerOnClose)
            {
                InEmergency = true;
                if (EmergencyStateChange != null)
                    EmergencyStateChange(this, new EventArgs());
                RunEmergencyBehavior();
            }
            else
            {
                InEmergency = false;
                if (EmergencyStateChange != null)
                    EmergencyStateChange(this, new EventArgs());
            }
        }

        /// <summary>
        /// RunEmergencyBehavior method
        /// </summary>
        public void RunEmergencyBehavior()
        {
            if (Behavior.Equals("shutdown"))
                Room.Shutdown();
        }
    }

    /// <summary>
    /// Defines the contract for IEssentialsRoomEmergency
    /// </summary>
    public interface IEssentialsRoomEmergency
    {
        /// <summary>
        /// Event fired when emergency state changes
        /// </summary>
        event EventHandler<EventArgs> EmergencyStateChange;

        /// <summary>
        /// Gets or sets the InEmergency
        /// </summary>
        bool InEmergency { get; }
    }
}