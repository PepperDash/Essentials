using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingInputSlot
    /// </summary>
    public interface IRoutingInputSlot: IRoutingSlot, IOnline, IVideoSync
    {
        string TxDeviceKey { get; }
    }
}
