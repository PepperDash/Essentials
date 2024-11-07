using PepperDash.Core;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Base class for RoutingInput and Output ports
    /// </summary>
    public abstract class RoutingPort : IKeyed
	{
		public string Key { get; private set; }
		public eRoutingSignalType Type { get; private set; }
		public eRoutingPortConnectionType ConnectionType { get; private set; }
		public readonly object Selector;
		public bool IsInternal { get; private set; }
        public object FeedbackMatchObject { get; set; }
        public object Port { get; set; }

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
        public string Key { get; private set; }
        public eRoutingSignalType Type { get; private set; }
        public eRoutingPortConnectionType ConnectionType { get; private set; }
        public readonly TSelector Selector;
        public bool IsInternal { get; private set; }
        public object FeedbackMatchObject { get; set; }
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