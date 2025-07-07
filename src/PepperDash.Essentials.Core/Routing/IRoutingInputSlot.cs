namespace PepperDash.Essentials.Core.Routing;

public interface IRoutingInputSlot: IRoutingSlot, IOnline, IVideoSync
{
    string TxDeviceKey { get; }
}
