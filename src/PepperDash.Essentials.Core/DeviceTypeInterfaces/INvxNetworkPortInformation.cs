using Crestron.SimplSharpPro.DM.Streaming;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Represents a collection of network port information and provides notifications when the information changes.
    /// </summary>
    /// <remarks>This interface is designed to provide access to a list of network port details and to notify
    /// subscribers when the port information is updated. Implementations of this interface should ensure that the  <see
    /// cref="PortInformationChanged"/> event is raised whenever the <see cref="NetworkPorts"/> collection
    /// changes.</remarks>
    public interface INvxNetworkPortInformation
    {
        /// <summary>
        /// Occurs when the port information changes.
        /// </summary>
        /// <remarks>This event is triggered whenever there is a change in the port information, such as
        /// updates to port settings or status. Subscribers can handle this event to respond to such changes.</remarks>
        event EventHandler PortInformationChanged;

        /// <summary>
        /// Gets the collection of network port information associated with the current instance.
        /// </summary>
        /// <remarks>The collection provides information about the network ports, such as their status,
        /// configuration, or other relevant details. The returned list is read-only and cannot be modified
        /// directly.</remarks>
        List<NvxNetworkPortInformation> NetworkPorts { get; }
    }

    /// <summary>
    /// Represents information about a network port, including its configuration and associated system details.
    /// </summary>
    /// <remarks>This class provides properties to describe various attributes of a network port, such as its
    /// name, description, VLAN configuration, and management IP address. It is typically used to store and retrieve 
    /// metadata about network ports in a managed environment.</remarks>
    public class NvxNetworkPortInformation
    {
        private readonly DmNvxBaseClass.DmNvx35xNetwork.DmNvxNetworkLldpPort port;

        /// <summary>
        /// Gets or sets the index of the device port.
        /// </summary>
        public uint DevicePortIndex { get; }

        /// <summary>
        /// Gets or sets the name of the port used for communication.
        /// </summary>        
        public string PortName => port.PortNameFeedback.StringValue;

        /// <summary>
        /// Gets or sets the description of the port.
        /// </summary>
        public string PortDescription => port.PortNameDescriptionFeedback.StringValue;

        /// <summary>
        /// Gets or sets the name of the VLAN (Virtual Local Area Network).
        /// </summary>
        public string VlanName => port.VlanNameFeedback.StringValue;

        /// <summary>
        /// Gets the IP management address associated with the port.
        /// </summary>
        public string IpManagementAddress => port.IpManagementAddressFeedback.StringValue;

        /// <summary>
        /// Gets the name of the system as reported by the associated port.
        /// </summary>
        public string SystemName => port.SystemNameFeedback.StringValue;

        /// <summary>
        /// Gets the description of the system name.
        /// </summary>
        public string SystemNameDescription => port.SystemNameDescriptionFeedback.StringValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NvxNetworkPortInformation"/> class with the specified network port
        /// and device port index.
        /// </summary>
        /// <param name="port">The network port associated with the device. Cannot be <see langword="null"/>.</param>
        /// <param name="devicePortIndex">The index of the device port.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="port"/> is <see langword="null"/>.</exception>
        public NvxNetworkPortInformation(DmNvxBaseClass.DmNvx35xNetwork.DmNvxNetworkLldpPort port, uint devicePortIndex)
        {
            this.port = port ?? throw new ArgumentNullException(nameof(port), "Port cannot be null");
            DevicePortIndex = devicePortIndex;
        }
    }
}
