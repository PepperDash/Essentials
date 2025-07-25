namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoutingInputsOutputs
    /// </summary>
    public interface IRoutingInputsOutputs : IRoutingInputs, IRoutingOutputs
	{
	}

/*    /// <summary>
    /// For devices like RMCs, baluns, other devices with no switching.
    /// </summary>
    public interface IRoutingInputsOutputs<TInputSelector, TOutputSelector> : IRoutingInputs<TInputSelector>, IRoutingOutputs<TOutputSelector>
    {
    }*/
}