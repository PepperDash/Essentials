using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Abstractions
{
    /// <summary>
    /// Adapter that wraps actual Crestron Control System for production use
    /// </summary>
    public class CrestronControlSystemAdapter : ICrestronControlSystem
    {
        private readonly CrestronControlSystem _controlSystem;
        private readonly Dictionary<uint, IRelayPort> _relayPorts;

        public CrestronControlSystemAdapter(CrestronControlSystem controlSystem)
        {
            _controlSystem = controlSystem ?? throw new ArgumentNullException(nameof(controlSystem));
            _relayPorts = new Dictionary<uint, IRelayPort>();
            
            if (_controlSystem.SupportsRelay)
            {
                for (uint i = 1; i <= (uint)_controlSystem.NumberOfRelayPorts; i++)
                {
                    _relayPorts[i] = new RelayPortAdapter(_controlSystem.RelayPorts[i]);
                }
            }
        }

        public bool SupportsRelay => _controlSystem.SupportsRelay;
        public uint NumberOfRelayPorts => (uint)_controlSystem.NumberOfRelayPorts;
        public Dictionary<uint, IRelayPort> RelayPorts => _relayPorts;
        public string ProgramIdTag => "TestProgram"; // Simplified for now
        public string ControllerPrompt => _controlSystem.ControllerPrompt;
        public bool SupportsEthernet => _controlSystem.SupportsEthernet;
        public bool SupportsDigitalInput => _controlSystem.SupportsDigitalInput;
        public uint NumberOfDigitalInputPorts => (uint)_controlSystem.NumberOfDigitalInputPorts;
        public bool SupportsVersiPort => _controlSystem.SupportsVersiport;
        public uint NumberOfVersiPorts => (uint)_controlSystem.NumberOfVersiPorts;
    }

    /// <summary>
    /// Adapter for Crestron relay port
    /// </summary>
    public class RelayPortAdapter : IRelayPort
    {
        private readonly Crestron.SimplSharpPro.Relay _relay;

        public RelayPortAdapter(Crestron.SimplSharpPro.Relay relay)
        {
            _relay = relay ?? throw new ArgumentNullException(nameof(relay));
        }

        public void Open() => _relay.Open();
        public void Close() => _relay.Close();
        public void Pulse(int delayMs) 
        {
            // Crestron Relay.Pulse() doesn't take parameters
            // We'll just call the basic Pulse method
            _relay.Close();
            System.Threading.Thread.Sleep(delayMs);
            _relay.Open();
        }
        public bool State => _relay.State;
    }

    /// <summary>
    /// Adapter for Crestron digital input
    /// </summary>
    public class DigitalInputAdapter : IDigitalInput
    {
        private readonly Crestron.SimplSharpPro.DigitalInput _digitalInput;

        public DigitalInputAdapter(Crestron.SimplSharpPro.DigitalInput digitalInput)
        {
            _digitalInput = digitalInput ?? throw new ArgumentNullException(nameof(digitalInput));
            _digitalInput.StateChange += OnStateChange;
        }

        public bool State => _digitalInput.State;
        public event EventHandler<DigitalInputEventArgs> StateChange;

        private void OnStateChange(DigitalInput digitalInput, Crestron.SimplSharpPro.DigitalInputEventArgs args)
        {
            StateChange?.Invoke(this, new Abstractions.DigitalInputEventArgs(args.State));
        }
    }

    /// <summary>
    /// Adapter for Crestron VersiPort
    /// </summary>
    public class VersiPortAdapter : IVersiPort
    {
        private readonly Crestron.SimplSharpPro.Versiport _versiPort;

        public VersiPortAdapter(Crestron.SimplSharpPro.Versiport versiPort)
        {
            _versiPort = versiPort ?? throw new ArgumentNullException(nameof(versiPort));
            _versiPort.VersiportChange += OnVersiportChange;
        }

        public bool DigitalIn => _versiPort.DigitalIn;
        public void SetDigitalOut(bool value) => _versiPort.DigitalOut = value;
        public ushort AnalogIn => _versiPort.AnalogIn;
        public event EventHandler<VersiPortEventArgs> VersiportChange;

        private void OnVersiportChange(Versiport port, VersiportEventArgs args)
        {
            var eventType = args.Event == eVersiportEvent.DigitalInChange 
                ? VersiPortEventType.DigitalInChange 
                : VersiPortEventType.AnalogInChange;
            
            VersiportChange?.Invoke(this, new Abstractions.VersiPortEventArgs 
            { 
                EventType = eventType,
                Value = args.Event == eVersiportEvent.DigitalInChange ? (object)port.DigitalIn : port.AnalogIn
            });
        }
    }
}