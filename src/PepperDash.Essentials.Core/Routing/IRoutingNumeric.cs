namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IRoutingNumeric
    /// </summary>
    public interface IRoutingNumeric : IRouting
    {
        /// <summary>
        /// Executes a numeric switch on the device
        /// </summary>
        /// <param name="input">input selector</param>
        /// <param name="output">output selector</param>
        /// <param name="type">type of signal</param>
        void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type);
    }
}