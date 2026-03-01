namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingInputSlot
    /// </summary>
    public interface IRoutingInputSlot: IRoutingSlot, IOnline, IVideoSync
    {
        /// <summary>
        /// Gets the Tx device key
        /// </summary>
        string TxDeviceKey { get; }
    }
}
