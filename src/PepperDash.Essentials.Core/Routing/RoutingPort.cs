namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Base class for <see cref="RoutingInputPort"/> and <see cref="RoutingOutputPort"/>.
    /// </summary>
    public abstract class RoutingPort : IKeyed
	{
		/// <summary>
		/// The unique key identifying this port within its parent device.
		/// </summary>
		public string Key { get; private set; }
		/// <summary>
		/// The type of signal this port handles (e.g., Audio, Video, AudioVideo).
		/// </summary>
		public eRoutingSignalType Type { get; private set; }
		/// <summary>
		/// The physical connection type of this port (e.g., Hdmi, Rca, Dm).
		/// </summary>
		public eRoutingPortConnectionType ConnectionType { get; private set; }
		/// <summary>
		/// An object (often a number or string) used by the parent routing device to select this port during switching.
		/// </summary>
		public readonly object Selector;
		/// <summary>
		/// Indicates if this port represents an internal connection within a device (e.g., card to backplane).
		/// </summary>
		public bool IsInternal { get; private set; }
        /// <summary>
        /// An object used to match feedback values to this port, if applicable.
        /// </summary>
        public object FeedbackMatchObject { get; set; }
        /// <summary>
        /// A reference to the underlying hardware port object (e.g., SimplSharpPro Port), if applicable.
        /// </summary>
        public object Port { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingPort"/> class.
		/// </summary>
		/// <param name="key">The unique key for this port.</param>
		/// <param name="type">The signal type supported by this port.</param>
		/// <param name="connType">The physical connection type of this port.</param>
		/// <param name="selector">The selector object for switching.</param>
		/// <param name="isInternal">True if this port is internal.</param>
		public RoutingPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType, object selector, bool isInternal)
		{
			Key = key;
			Type = type;
			ConnectionType = connType;
			Selector = selector;
			IsInternal = isInternal;
		}        
    }

    /*public abstract class RoutingPort<TSelector>:IKeyed
    {
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public eRoutingSignalType Type { get; private set; }
        /// <summary>
        /// Gets or sets the ConnectionType
        /// </summary>
        public eRoutingPortConnectionType ConnectionType { get; private set; }
        public readonly TSelector Selector;
        /// <summary>
        /// Gets or sets the IsInternal
        /// </summary>
        public bool IsInternal { get; private set; }
        /// <summary>
        /// Gets or sets the FeedbackMatchObject
        /// </summary>
        public object FeedbackMatchObject { get; set; }
        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public object Port { get; set; }

        public RoutingPort(string key, eRoutingSignalType type, eRoutingPortConnectionType connType, TSelector selector, bool isInternal)
        {
            Key = key;
            Type = type;
            ConnectionType = connType;
            Selector = selector;
            IsInternal = isInternal;
        }
    }*/
}