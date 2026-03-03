using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// This wrapper class is meant to allow interfaces to be applied to any Crestron processor
    /// </summary>
    public class CrestronProcessor : Device, ISwitchedOutputCollection
    {
        /// <summary>
        /// Collection of switched outputs (relays) on the processor
        /// </summary>
        public Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; private set; }

        /// <summary>
        /// The underlying CrestronControlSystem processor
        /// </summary>
        public Crestron.SimplSharpPro.CrestronControlSystem Processor { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key for the processor</param>
        public CrestronProcessor(string key)
            : base(key)
        {
            SwitchedOutputs = new Dictionary<uint, ISwitchedOutput>();
            Processor = Global.ControlSystem;

            GetRelays();
        }

        /// <summary>
        /// Creates a GenericRelayDevice for each relay on the processor and adds them to the SwitchedOutputs collection
        /// </summary>
        void GetRelays()
        {
            try
            {
                if (Processor.SupportsRelay)
                {
                    for (uint i = 1; i <= Processor.NumberOfRelayPorts; i++)
                    {
                        var relay = new GenericRelayDevice(string.Format("{0}-relay-{1}", this.Key, i), Processor.RelayPorts[i]);
                        SwitchedOutputs.Add(i, relay);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error Getting Relays from processor:\n '{0}'", e);
            }
        }
    }
}