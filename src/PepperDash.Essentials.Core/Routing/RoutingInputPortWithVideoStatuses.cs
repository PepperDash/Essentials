namespace PepperDash.Essentials.Core;

/// <summary>
/// Represents a routing input port that provides video status feedback (e.g., sync, resolution).
/// Suitable for devices like DM transmitters or DM input cards.
/// </summary>
public class RoutingInputPortWithVideoStatuses : RoutingInputPort
	{
		/// <summary>
		/// Provides feedback outputs for video statuses associated with this port.
		/// </summary>
		public VideoStatusOutputs VideoStatus { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingInputPortWithVideoStatuses"/> class.
		/// </summary>
		/// <param name="key">The unique key for this port.</param>
		/// <param name="type">The signal type supported by this port.</param>
		/// <param name="connType">The physical connection type of this port.</param>
		/// <param name="selector">An object used to refer to this port in the parent device's ExecuteSwitch method.</param>
		/// <param name="parent">The <see cref="IRoutingInputs"/> device this port belongs to.</param>
		/// <param name="funcs">A <see cref="VideoStatusFuncsWrapper"/> containing delegates to retrieve video status values.</param>
		public RoutingInputPortWithVideoStatuses(string key, 
			eRoutingSignalType type, eRoutingPortConnectionType connType, object selector, 
			IRoutingInputs parent, VideoStatusFuncsWrapper funcs) :
			base(key, type, connType, selector, parent)
		{
			VideoStatus = new VideoStatusOutputs(funcs);		
		}
	}