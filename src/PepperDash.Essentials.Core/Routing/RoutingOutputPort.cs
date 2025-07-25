using Newtonsoft.Json;
using System;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a RoutingOutputPort
    /// </summary>
    public class RoutingOutputPort : RoutingPort
    {
        /// <summary>
        /// The IRoutingOutputs object this port lives on.
        /// </summary>
        [JsonIgnore]
        public IRoutingOutputs ParentDevice { get; private set; }

		/// <summary>
		/// Tracks which destinations are currently using this output port.
		/// </summary>
		public InUseTracking InUseTracker { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingOutputPort"/> class.
		/// </summary>
		/// <param name="key">The unique key for this port.</param>
		/// <param name="type">The signal type supported by this port.</param>
		/// <param name="connType">The physical connection type of this port.</param>
		/// <param name="selector">An object used to refer to this port in the parent device's ExecuteSwitch method.</param>
		/// <param name="parent">The <see cref="IRoutingOutputs"/> device this port belongs to.</param>
		public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingOutputs parent)
			: this(key, type, connType, selector, parent, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingOutputPort"/> class, potentially marking it as internal.
		/// </summary>
		/// <param name="key">The unique key for this port.</param>
		/// <param name="type">The signal type supported by this port.</param>
		/// <param name="connType">The physical connection type of this port.</param>
		/// <param name="selector">An object used to refer to this port in the parent device's ExecuteSwitch method.</param>
		/// <param name="parent">The <see cref="IRoutingOutputs"/> device this port belongs to.</param>
		/// <param name="isInternal">True if this port represents an internal connection within a device (e.g., card to backplane).</param>
		public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
			object selector, IRoutingOutputs parent, bool isInternal)
			: base(key, type, connType, selector, isInternal)
		{
            ParentDevice = parent ?? throw new ArgumentNullException(nameof(parent));
			InUseTracker = new InUseTracking();
		}

        /// <summary>
        /// Returns a string representation of the output port.
        /// </summary>
        /// <returns>A string in the format "ParentDeviceKey|PortKey|SignalType|ConnectionType".</returns>
        public override string ToString()
        {
            return $"{ParentDevice.Key}|{Key}|{Type}|{ConnectionType}";
        }
	}

    /*public class RoutingOutputPort<TSelector> : RoutingPort<TSelector>
    {
        /// <summary>
        /// Gets or sets the ParentDevice
        /// </summary>
        public IRoutingOutputs ParentDevice { get; private set; }

        /// <summary>
        /// Gets or sets the InUseTracker
        /// </summary>
        public InUseTracking InUseTracker { get; private set; }


        /// <summary>
        /// </summary>
        /// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
        /// May be string, number, whatever</param>
        /// <param name="parent">The IRoutingOutputs object this port lives on</param>
        public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            TSelector selector, IRoutingOutputs parent)
            : this(key, type, connType, selector, parent, false)
        {
        }

        public RoutingOutputPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType,
            TSelector selector, IRoutingOutputs parent, bool isInternal)
            : base(key, type, connType, selector, isInternal)
        {
            ParentDevice = parent ?? throw new ArgumentNullException(nameof(parent));
            InUseTracker = new InUseTracking();
        }

        /// <summary>
        /// ToString method
        /// </summary>
        /// <inheritdoc />
        public override string ToString()
        {
            return ParentDevice.Key + ":" + Key;
        }
    }*/
}