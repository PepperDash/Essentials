using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Basic RoutingInput with no statuses.
    /// </summary>
    public class RoutingInputPort : RoutingPort
    {
        /// <summary>
        /// The IRoutingInputs object this lives on
        /// </summary>
        public IRoutingInputs ParentDevice { get; private set; }

        /// <summary>
        /// Constructor for a basic RoutingInputPort
        /// </summary>
        /// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
        /// May be string, number, whatever</param>
        /// <param name="parent">The IRoutingInputs object this lives on</param>
        public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            object selector, IRoutingInputs parent)
            : this (key, type, connType, selector, parent, false)
        {
        }

        /// <summary>
        /// Constructor for a virtual routing input port that lives inside a device. For example
        /// the ports that link a DM card to a DM matrix bus
        /// </summary>
        /// <param name="isInternal">true for internal ports</param>
        public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            object selector, IRoutingInputs parent, bool isInternal)
            : base(key, type, connType, selector, isInternal)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            ParentDevice = parent;
        }




        ///// <summary>
        ///// Static method to get a named port from a named device
        ///// </summary>
        ///// <returns>Returns null if device or port doesn't exist</returns>
        //public static RoutingInputPort GetDevicePort(string deviceKey, string portKey)
        //{
        //    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as IRoutingInputs;
        //    if (sourceDev == null)
        //        return null;
        //    return sourceDev.InputPorts[portKey];
        //}

        ///// <summary>
        ///// Static method to get a named port from a card in a named ICardPortsDevice device
        ///// Uses ICardPortsDevice.GetChildInputPort 
        ///// </summary>
        ///// <param name="cardKey">'input-N'</param>
        ///// <returns>null if device, card or port doesn't exist</returns>
        //public static RoutingInputPort GetDeviceCardPort(string deviceKey, string cardKey, string portKey)
        //{
        //    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as ICardPortsDevice;
        //    if (sourceDev == null)
        //        return null;
        //    return sourceDev.GetChildInputPort(cardKey, portKey);
        //}
    }
}