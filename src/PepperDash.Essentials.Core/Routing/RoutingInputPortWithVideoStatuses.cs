namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A RoutingInputPort for devices like DM-TX and DM input cards. 
    /// Will provide video statistics on connected signals
    /// </summary>
    public class RoutingInputPortWithVideoStatuses : RoutingInputPort
	{
		/// <summary>
		/// Video statuses attached to this port
		/// </summary>
		public VideoStatusOutputs VideoStatus { get; private set; }

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="selector">An object used to refer to this port in the IRouting device's ExecuteSwitch method.
		/// May be string, number, whatever</param>
		/// <param name="parent">The IRoutingInputs object this lives on</param>
		/// <param name="funcs">A VideoStatusFuncsWrapper used to assign the callback funcs that will get 
		/// the values for the various stats</param>
		public RoutingInputPortWithVideoStatuses(string key, 
			eRoutingSignalType type, eRoutingPortConnectionType connType, object selector, 
			IRoutingInputs parent, VideoStatusFuncsWrapper funcs) :
			base(key, type, connType, selector, parent)
		{
			VideoStatus = new VideoStatusOutputs(funcs);		
		}
	}
}