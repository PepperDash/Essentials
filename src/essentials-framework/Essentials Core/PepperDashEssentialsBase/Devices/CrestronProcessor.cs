﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// This wrapper class is meant to allow interfaces to be applied to any Crestron processor
    /// </summary>
    public class CrestronProcessor : Device, ISwitchedOutputCollection
    {
        public Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; private set; }

        public Crestron.SimplSharpPro.CrestronControlSystem Processor { get; private set; }

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
                Debug.Console(1, this, "Error Getting Relays from processor:\n '{0}'", e);
            }
        }
    }
}