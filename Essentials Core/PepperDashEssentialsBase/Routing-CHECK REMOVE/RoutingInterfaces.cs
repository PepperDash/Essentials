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
	//*******************************************************************************************
	// Interfaces

	public interface IRoutingInputs : IKeyed
	{
		RoutingPortCollection<RoutingInputPort> InputPorts { get; }
	}

	public interface IRoutingOutputs : IKeyed
	{
		RoutingPortCollection<RoutingOutputPort> OutputPorts { get; }
	}

	/// <summary>
	/// For fixed-source endpoint devices
	/// </summary>
	public interface IRoutingSinkNoSwitching : IRoutingInputs
	{

	}

	public interface IRoutingSinkWithSwitching : IRoutingSinkNoSwitching
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

	public interface IRouting : IRoutingInputsOutputs
	{
		//void ClearRoute(object outputSelector);
		void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType);
	}

	public interface IRoutingSource : IRoutingOutputs
	{
	}
}