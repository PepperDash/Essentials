namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingNumeric
    /// </summary>
    public interface IRoutingNumeric : IRouting
    {
        void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type);
    }
}