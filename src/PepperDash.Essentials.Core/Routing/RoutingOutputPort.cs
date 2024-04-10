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
            ParentDevice = parent ?? throw new ArgumentNullException(nameof(parent));
			InUseTracker = new InUseTracking();
		}

        public override string ToString()
        {
            return ParentDevice.Key + ":" + Key;
        }
	}

    /*public class RoutingOutputPort<TSelector> : RoutingPort<TSelector>
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

        public override string ToString()
        {
            return ParentDevice.Key + ":" + Key;
        }
    }*/
}