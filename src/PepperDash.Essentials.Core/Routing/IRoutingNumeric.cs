namespace PepperDash.Essentials.Core
{
    public interface IRoutingNumeric : IRouting
    {
        void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type);
    }
}