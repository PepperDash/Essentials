namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Defines the contract for IRoutingSlot
    /// </summary>
    public interface IRoutingSlot:IKeyName
    {
        int SlotNumber { get; }

        eRoutingSignalType SupportedSignalTypes { get; }
    }
}
