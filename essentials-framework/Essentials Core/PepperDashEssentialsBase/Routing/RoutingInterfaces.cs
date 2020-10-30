using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// The handler type for a Room's SourceInfoChange
    /// </summary>
    public delegate void SourceInfoChangeHandler(/*EssentialsRoomBase room,*/ SourceListItem info, ChangeType type);


	//*******************************************************************************************
	// Interfaces

    /// <summary>
    /// For rooms with a single presentation source, change event
    /// </summary>
    public interface IHasCurrentSourceInfoChange
    {
        string CurrentSourceInfoKey { get; set; }
        SourceListItem CurrentSourceInfo { get; set; }
        event SourceInfoChangeHandler CurrentSourceChange;
    }

	/// <summary>
	/// Defines a class that has a collection of RoutingInputPorts
	/// </summary>
	public interface IRoutingInputs : IKeyed
	{
		RoutingPortCollection<RoutingInputPort> InputPorts { get; }
	}

	/// <summary>
	/// Defines a class that has a collection of RoutingOutputPorts
	/// </summary>

	public interface IRoutingOutputs : IKeyed
	{
		RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
	}

    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSink : IRoutingInputs, IHasCurrentSourceInfoChange
    {

    }
 
	/// <summary>
	/// For fixed-source endpoint devices
	/// </summary>
    [Obsolete]
    public interface IRoutingSinkNoSwitching : IRoutingSink
	{

	}

	/// <summary>
	/// Endpoint device like a display, that selects inputs
	/// </summary>
    public interface IRoutingSinkWithSwitching : IRoutingSink
	{
		//void ClearRoute();
		void ExecuteSwitch(object inputSelector);
	}

	/// <summary>
	/// For devices like RMCs, baluns, other devices with no switching.
	/// </summary>
	public interface IRoutingInputsOutputs : IRoutingInputs, IRoutingOutputs
	{
	}

	/// <summary>
	/// Defines a midpoint device as have internal routing.  Any devices in the middle of the
	/// signal chain, that do switching, must implement this for routing to work otherwise
	/// the routing algorithm will treat the IRoutingInputsOutputs device as a passthrough
	/// device.
	/// </summary>
	public interface IRouting : IRoutingInputsOutputs
	{
		void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType);
    }

    public interface IRoutingNumeric : IRouting
    {
        void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type);
    }

    public interface ITxRouting : IRoutingNumeric
    {
        IntFeedback VideoSourceNumericFeedback { get; }
        IntFeedback AudioSourceNumericFeedback { get; }
    }

    /// <summary>
    /// Defines a receiver that has internal routing (DM-RMC-4K-Z-SCALER-C)
    /// </summary>
    public interface IRmcRouting : IRoutingNumeric
    {
        IntFeedback AudioVideoSourceNumericFeedback { get; }
    }


    /// <summary>
    /// Defines an IRmcRouting with a feedback event 
    /// </summary>
    public interface ITxRoutingWithFeedback : ITxRouting, IRoutingNumericFeedback
    {
    }

    /// <summary>
    /// Defines an IRmcRouting with a feedback event 
    /// </summary>
    public interface IRmcRoutingWithFeedback : IRmcRouting, IRoutingNumericFeedback
    {
    }

    /// <summary>
	/// Defines an IRoutingOutputs devices as being a source - the start of the chain
	/// </summary>
	public interface IRoutingSource : IRoutingOutputs
	{
	}

    /// <summary>
    /// Defines an event structure for reporting output route data
    /// </summary>
    public interface IRoutingNumericFeedback : IKeyName
    {
        event EventHandler NumericSwitchChange;
        void OnSwitchChange(RoutingNumericEventArgs e);
    }

    /// <summary>
    /// Defines an IRoutingNumeric with a feedback event 
    /// </summary>
    public interface IRoutingNumericWithFeedback : IRoutingNumeric, IRoutingNumericFeedback
    {
    }

    public class RoutingNumericEventArgs : EventArgs
    {
        private readonly uint _output;
        private readonly uint _input;
        private readonly eRoutingSignalType _sigType;

        public uint Output
        {
            get { return _output; }
        }

        public uint Input
        {
            get { return _input; }
        }

        public eRoutingSignalType SigType
        {
            get { return _sigType; }
        }

        public RoutingNumericEventArgs(uint output, uint input, eRoutingSignalType sigType)
        {
            _output = output;
            _input = input;
            _sigType = sigType;
        }
    }
}