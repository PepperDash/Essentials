using System;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Interface for network switches that support VLAN assignment on individual ports.
    /// </summary>
    public interface INetworkSwitchVlanManager
    {
        /// <summary>
        /// Returns the current access VLAN ID configured on the port.
        /// Return -1 when the value is unavailable (e.g. the switch has not been polled yet
        /// or the implementation does not support VLAN queries).
        /// </summary>
        /// <param name="port">Switch port identifier</param>
        /// <returns>VLAN ID or -1 when unavailable</returns>
        int GetPortCurrentVlan(string port);

        /// <summary>
        /// Changes the access VLAN of a single switch port.
        /// The implementation is responsible for entering/exiting privileged/config mode.
        /// </summary>
        /// <param name="port">Switch port identifier (e.g. "1/0/3" for Netgear, "gi1/0/3" for Cisco)</param>
        /// <param name="vlanId">Target VLAN ID (1-4093)</param>
        void SetPortVlan(string port, uint vlanId);
    }

    /// <summary>
    /// Interface for network switches that support Power over Ethernet (PoE) control on individual ports.
    /// </summary>
    public interface INetworkSwitchPoeManager
    {
        /// <summary>
        /// Enables or disables PoE power delivery on a single switch port.
        /// The implementation is responsible for entering/exiting privileged/config mode.
        /// </summary>
        /// <param name="port">Switch port identifier</param>
        /// <param name="enabled">True to enable PoE; false to disable PoE</param>
        void SetPortPoeState(string port, bool enabled);
    }

    /// <summary>
    /// Standardized interface for network switch devices that support per-port PoE control
    /// and VLAN assignment.
    /// </summary>
    public interface INetworkSwitchPoeVlanManager : INetworkSwitchVlanManager, INetworkSwitchPoeManager
    {
        /// <summary>
        /// Event that is raised when the state of a switch port changes, such as a VLAN change or PoE state change.
        /// </summary>
        event EventHandler<NetworkSwitchPortEventArgs> PortStateChanged;

    }

    /// <summary>
    /// Event arguments for port state changes on a network switch, such as VLAN changes or PoE state changes.
    /// </summary>
    public class NetworkSwitchPortEventArgs : EventArgs
    {
        /// <summary>
        /// The identifier of the port that changed state (e.g. "1/0/3" for Netgear, "gi1/0/3" for Cisco).
        /// </summary>
        public string Port { get; private set; }
        
        /// <summary>
        /// The type of event that occurred on the port (e.g. VLAN change, PoE enabled/disabled).
        /// </summary>
        public NetworkSwitchPortEventType EventType { get; private set; }

        /// <summary>
        /// Constructor for NetworkSwitchPortEventArgs
        /// </summary>
        /// <param name="port">The identifier of the port that changed state</param>
        /// <param name="eventType">The type of event that occurred on the port</param>
        public NetworkSwitchPortEventArgs(string port, NetworkSwitchPortEventType eventType)
        {
            Port = port;
            EventType = eventType;
        }
    }

    /// <summary>
    /// Event arguments for port state changes on a network switch, such as VLAN changes or PoE state changes.
    /// </summary>
    public enum NetworkSwitchPortEventType
    {
        /// <summary>
        /// Indicates that the access VLAN on a port has changed, either through a successful call to SetPortVlan
        /// </summary>
        VlanChanged,

        /// <summary>
        /// Indicates that the PoE state on a port has changed, either through a successful call to SetPortPoeState
        /// </summary>
        PoEDisabled,

        /// <summary>
        /// Indicates that the PoE state on a port has changed, either through a successful call to SetPortPoeState
        /// </summary>
        PoEEnabled
    }
}
