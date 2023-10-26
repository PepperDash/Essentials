namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Used to decorate a contact to indicate it can be invided to a meeting
    /// </summary>
    public interface IInvitableContact
    {
        bool IsInvitableContact { get; }
    }
}