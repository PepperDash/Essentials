using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Abstractions;

namespace PepperDash.Essentials.Core.Factory
{
    /// <summary>
    /// Mock provider for unit testing without Crestron hardware
    /// </summary>
    public class CrestronMockProvider : ICrestronEnvironmentProvider
    {
        private MockControlSystem _controlSystem;

        public CrestronMockProvider()
        {
            _controlSystem = new MockControlSystem();
        }

        public ICrestronControlSystem GetControlSystem()
        {
            return _controlSystem;
        }

        public IRelayPort CreateRelayPort(uint portNumber)
        {
            if (!_controlSystem.RelayPorts.ContainsKey(portNumber))
            {
                _controlSystem.RelayPorts[portNumber] = new MockRelayPort();
            }
            return _controlSystem.RelayPorts[portNumber];
        }

        public IDigitalInput CreateDigitalInput(uint portNumber)
        {
            if (!_controlSystem.DigitalInputs.ContainsKey(portNumber))
            {
                _controlSystem.DigitalInputs[portNumber] = new MockDigitalInput();
            }
            return _controlSystem.DigitalInputs[portNumber];
        }

        public IVersiPort CreateVersiPort(uint portNumber)
        {
            if (!_controlSystem.VersiPorts.ContainsKey(portNumber))
            {
                _controlSystem.VersiPorts[portNumber] = new MockVersiPort();
            }
            return _controlSystem.VersiPorts[portNumber];
        }

        public IConsoleManager GetConsoleManager()
        {
            return new MockConsoleManager();
        }

        public ISystemInfo GetSystemInfo()
        {
            return new MockSystemInfo();
        }

        /// <summary>
        /// Configures the mock control system for testing
        /// </summary>
        public void ConfigureMockSystem(Action<MockControlSystem> configure)
        {
            configure(_controlSystem);
        }
    }

    /// <summary>
    /// Mock implementation of control system for testing
    /// </summary>
    public class MockControlSystem : ICrestronControlSystem
    {
        public bool SupportsRelay { get; set; } = true;
        public uint NumberOfRelayPorts { get; set; } = 8;
        public Dictionary<uint, IRelayPort> RelayPorts { get; set; } = new Dictionary<uint, IRelayPort>();
        public Dictionary<uint, IDigitalInput> DigitalInputs { get; set; } = new Dictionary<uint, IDigitalInput>();
        public Dictionary<uint, IVersiPort> VersiPorts { get; set; } = new Dictionary<uint, IVersiPort>();
        public string ProgramIdTag { get; set; } = "TEST_PROGRAM";
        public string ControllerPrompt { get; set; } = "TEST>";
        public bool SupportsEthernet { get; set; } = true;
        public bool SupportsDigitalInput { get; set; } = true;
        public uint NumberOfDigitalInputPorts { get; set; } = 8;
        public bool SupportsVersiPort { get; set; } = true;
        public uint NumberOfVersiPorts { get; set; } = 8;

        public MockControlSystem()
        {
            // Initialize with default relay ports
            for (uint i = 1; i <= NumberOfRelayPorts; i++)
            {
                RelayPorts[i] = new MockRelayPort();
            }

            // Initialize with default digital inputs
            for (uint i = 1; i <= NumberOfDigitalInputPorts; i++)
            {
                DigitalInputs[i] = new MockDigitalInput();
            }

            // Initialize with default versiports
            for (uint i = 1; i <= NumberOfVersiPorts; i++)
            {
                VersiPorts[i] = new MockVersiPort();
            }
        }
    }

    /// <summary>
    /// Mock implementation of relay port for testing
    /// </summary>
    public class MockRelayPort : IRelayPort
    {
        private bool _state;
        
        public bool State => _state;

        public event EventHandler<bool> StateChanged;

        public void Open()
        {
            _state = false;
            StateChanged?.Invoke(this, _state);
        }

        public void Close()
        {
            _state = true;
            StateChanged?.Invoke(this, _state);
        }

        public void Pulse(int delayMs)
        {
            Close();
            System.Threading.Tasks.Task.Delay(delayMs).ContinueWith(_ => Open());
        }

        /// <summary>
        /// Test helper to set state directly
        /// </summary>
        public void SetState(bool state)
        {
            _state = state;
            StateChanged?.Invoke(this, _state);
        }
    }

    /// <summary>
    /// Mock implementation of digital input for testing
    /// </summary>
    public class MockDigitalInput : IDigitalInput
    {
        private bool _state;

        public bool State => _state;

        public event EventHandler<Abstractions.DigitalInputEventArgs> StateChange;

        /// <summary>
        /// Test helper to simulate input change
        /// </summary>
        public void SimulateStateChange(bool newState)
        {
            _state = newState;
            StateChange?.Invoke(this, new Abstractions.DigitalInputEventArgs(newState));
        }
    }

    /// <summary>
    /// Mock implementation of versiport for testing
    /// </summary>
    public class MockVersiPort : IVersiPort
    {
        private bool _digitalIn;
        private bool _digitalOut;
        private ushort _analogIn;

        public bool DigitalIn => _digitalIn;
        public ushort AnalogIn => _analogIn;

        public event EventHandler<Abstractions.VersiPortEventArgs> VersiportChange;

        public void SetDigitalOut(bool value)
        {
            _digitalOut = value;
        }

        /// <summary>
        /// Test helper to simulate digital input change
        /// </summary>
        public void SimulateDigitalInChange(bool value)
        {
            _digitalIn = value;
            VersiportChange?.Invoke(this, new Abstractions.VersiPortEventArgs 
            { 
                EventType = Abstractions.VersiPortEventType.DigitalInChange, 
                Value = value 
            });
        }

        /// <summary>
        /// Test helper to simulate analog input change
        /// </summary>
        public void SimulateAnalogInChange(ushort value)
        {
            _analogIn = value;
            VersiportChange?.Invoke(this, new Abstractions.VersiPortEventArgs 
            { 
                EventType = Abstractions.VersiPortEventType.AnalogInChange, 
                Value = value 
            });
        }
    }

    /// <summary>
    /// Mock implementation of console manager for testing
    /// </summary>
    public class MockConsoleManager : IConsoleManager
    {
        public List<string> OutputLines { get; } = new List<string>();
        public Dictionary<string, Action<string>> Commands { get; } = new Dictionary<string, Action<string>>();

        public void Print(string message)
        {
            OutputLines.Add(message);
            Console.Write(message);
        }

        public void PrintLine(string message)
        {
            OutputLines.Add(message);
            Console.WriteLine(message);
        }

        public void RegisterCommand(string command, Action<string> handler, string help)
        {
            Commands[command] = handler;
        }

        /// <summary>
        /// Test helper to execute a registered command
        /// </summary>
        public void ExecuteCommand(string command, string args)
        {
            if (Commands.TryGetValue(command, out var handler))
            {
                handler(args);
            }
        }
    }

    /// <summary>
    /// Mock implementation of system info for testing
    /// </summary>
    public class MockSystemInfo : ISystemInfo
    {
        public string ProgramName { get; set; } = "TestProgram";
        public string SerialNumber { get; set; } = "TEST123456";
        public string MacAddress { get; set; } = "00:11:22:33:44:55";
        public string IpAddress { get; set; } = "192.168.1.100";
        public string FirmwareVersion { get; set; } = "1.0.0.0";
        public DateTime SystemUpTime { get; set; } = DateTime.Now.AddHours(-1);
    }
}