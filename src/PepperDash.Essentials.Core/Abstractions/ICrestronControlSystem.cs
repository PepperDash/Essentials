using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Abstractions
{
    /// <summary>
    /// Abstraction for Crestron Control System to enable unit testing
    /// </summary>
    public interface ICrestronControlSystem
    {
        bool SupportsRelay { get; }
        uint NumberOfRelayPorts { get; }
        Dictionary<uint, IRelayPort> RelayPorts { get; }
        string ProgramIdTag { get; }
        string ControllerPrompt { get; }
        bool SupportsEthernet { get; }
        bool SupportsDigitalInput { get; }
        uint NumberOfDigitalInputPorts { get; }
        bool SupportsVersiPort { get; }
        uint NumberOfVersiPorts { get; }
    }

    /// <summary>
    /// Abstraction for relay port
    /// </summary>
    public interface IRelayPort
    {
        void Open();
        void Close();
        void Pulse(int delayMs);
        bool State { get; }
    }

    /// <summary>
    /// Abstraction for digital input
    /// </summary>
    public interface IDigitalInput
    {
        bool State { get; }
        event EventHandler<DigitalInputEventArgs> StateChange;
    }

    public class DigitalInputEventArgs : EventArgs
    {
        public bool State { get; set; }
        public DigitalInputEventArgs(bool state)
        {
            State = state;
        }
    }

    /// <summary>
    /// Abstraction for VersiPort
    /// </summary>
    public interface IVersiPort
    {
        bool DigitalIn { get; }
        void SetDigitalOut(bool value);
        ushort AnalogIn { get; }
        event EventHandler<VersiPortEventArgs> VersiportChange;
    }

    public class VersiPortEventArgs : EventArgs
    {
        public VersiPortEventType EventType { get; set; }
        public object Value { get; set; }
    }

    public enum VersiPortEventType
    {
        DigitalInChange,
        AnalogInChange
    }
}