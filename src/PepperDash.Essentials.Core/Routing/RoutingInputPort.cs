using Newtonsoft.Json;
using System;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a basic routing input port on a device.
    /// </summary>
    public class RoutingInputPort : RoutingPort
	{
        /// <summary>
        /// The IRoutingInputs object this lives on
        /// </summary>
        [JsonIgnore]
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
				throw new ArgumentNullException(nameof(parent));
			ParentDevice = parent;
		}

        /// <summary>
        /// Returns a string representation of the input port.
        /// </summary>
        /// <returns>A string in the format "ParentDeviceKey|PortKey|SignalType|ConnectionType".</returns>
        public override string ToString()
        {
            return $"{ParentDevice.Key}|{Key}|{Type}|{ConnectionType}";
        }
    }

    /*/// <summary>
    /// Basic RoutingInput with no statuses.
    /// </summary>
    public class RoutingInputPort<TSelector> : RoutingPort<TSelector>
    {
        /// <summary>
        /// The IRoutingInputs object this lives on
        /// </summary>
        public IRoutingInputs<TSelector> ParentDevice { get; private set; }

        /// <summary>
        /// Constructor for a basic RoutingInputPort
        /// </summary>
        /// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
        /// May be string, number, whatever</param>
        /// <param name="parent">The IRoutingInputs object this lives on</param>
        public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            TSelector selector, IRoutingInputs<TSelector> parent)
            : this(key, type, connType, selector, parent, false)
        {
        }

        /// <summary>
        /// Constructor for a virtual routing input port that lives inside a device. For example
        /// the ports that link a DM card to a DM matrix bus
        /// </summary>
        /// <param name="isInternal">true for internal ports</param>
        public RoutingInputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            TSelector selector, IRoutingInputs<TSelector> parent, bool isInternal)
            : base(key, type, connType, selector, isInternal)
        {
            ParentDevice = parent ?? throw new ArgumentNullException(nameof(parent));
        }
    }*/
}