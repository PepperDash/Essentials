using System;

namespace PepperDash.Essentials.Core
{
    public class RoutingOutputPort : RoutingPort
    {
        /// <summary>
        /// The IRoutingOutputs object this port lives on
        /// </summary>
        public IRoutingOutputs ParentDevice { get; private set; }

        public InUseTracking InUseTracker { get; private set; }


        /// <summary>
        /// </summary>
        /// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
        /// May be string, number, whatever</param>
        /// <param name="parent">The IRoutingOutputs object this port lives on</param>
        public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            object selector, IRoutingOutputs parent)
            : this(key, type, connType, selector, parent, false)
        {
        }

        public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            object selector, IRoutingOutputs parent, bool isInternal)
            : base(key, type, connType, selector, isInternal)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            ParentDevice = parent;
            InUseTracker = new InUseTracking();
        }

        public override string ToString()
        {
            return ParentDevice.Key + ":" + Key;
        }

        ///// <summary>
        ///// Static method to get a named port from a named device
        ///// </summary>
        ///// <returns>Returns null if device or port doesn't exist</returns>
        //public static RoutingOutputPort GetDevicePort(string deviceKey, string portKey)
        //{
        //    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as IRoutingOutputs;
        //    if (sourceDev == null)
        //        return null;
        //    var port = sourceDev.OutputPorts[portKey];
        //    if (port == null)
        //        Debug.Console(0, "WARNING: Device '{0}' does does not contain output port '{1}'", deviceKey, portKey);
        //    return port;
        //}

        ///// <summary>
        ///// Static method to get a named port from a card in a named ICardPortsDevice device
        ///// Uses ICardPortsDevice.GetChildOutputPort on that device
        ///// </summary>
        ///// <param name="cardKey">'input-N' or 'output-N'</param>
        ///// <returns>null if device, card or port doesn't exist</returns>
        //public static RoutingOutputPort GetDeviceCardPort(string deviceKey, string cardKey, string portKey)
        //{
        //    var sourceDev = DeviceManager.GetDeviceForKey(deviceKey) as ICardPortsDevice;
        //    if (sourceDev == null)
        //        return null;
        //    var port = sourceDev.GetChildOutputPort(cardKey, portKey);
        //}
    }
}