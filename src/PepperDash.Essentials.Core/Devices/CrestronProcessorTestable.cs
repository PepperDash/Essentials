using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Abstractions;
using PepperDash.Essentials.Core.CrestronIO;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Testable version of CrestronProcessor that uses abstractions
    /// </summary>
    public class CrestronProcessorTestable : Device, ISwitchedOutputCollection
    {
        public Dictionary<uint, ISwitchedOutput> SwitchedOutputs { get; private set; }

        public ICrestronControlSystem Processor { get; private set; }

        public CrestronProcessorTestable(string key, ICrestronControlSystem processor)
            : base(key)
        {
            SwitchedOutputs = new Dictionary<uint, ISwitchedOutput>();
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
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
                        if (Processor.RelayPorts.ContainsKey(i))
                        {
                            var relay = new GenericRelayDeviceTestable(
                                string.Format("{0}-relay-{1}", this.Key, i), 
                                Processor.RelayPorts[i]);
                            SwitchedOutputs.Add(i, relay);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error Getting Relays from processor:\n '{0}'", e);
            }
        }
    }

    /// <summary>
    /// Testable version of GenericRelayDevice
    /// </summary>
    public class GenericRelayDeviceTestable : Device, ISwitchedOutput
    {
        private readonly IRelayPort _relayPort;

        public GenericRelayDeviceTestable(string key, IRelayPort relayPort)
            : base(key)
        {
            _relayPort = relayPort ?? throw new ArgumentNullException(nameof(relayPort));
            OutputIsOnFeedback = new BoolFeedback(() => IsOn);
        }

        public void OpenRelay()
        {
            _relayPort.Open();
            OutputIsOnFeedback.FireUpdate();
        }

        public void CloseRelay()
        {
            _relayPort.Close();
            OutputIsOnFeedback.FireUpdate();
        }

        public void PulseRelay(int delayMs)
        {
            _relayPort.Pulse(delayMs);
        }

        public void On()
        {
            CloseRelay();
        }

        public void Off()
        {
            OpenRelay();
        }

        public void PowerToggle()
        {
            if (IsOn)
                Off();
            else
                On();
        }

        public bool IsOn => _relayPort.State;

        public BoolFeedback OutputIsOnFeedback { get; private set; }

        public override bool CustomActivate()
        {
            OutputIsOnFeedback = new BoolFeedback(() => IsOn);
            return base.CustomActivate();
        }
    }
}