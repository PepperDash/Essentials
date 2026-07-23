namespace PepperDash.Essentials.Devices.Common.AudioCodec;

/// <summary>
/// Interface for devices that can decode DTMF digits
/// </summary>
public interface IDtmfDecode
{
    /// <summary>
    /// Event fired when a DTMF digit is received
    /// </summary>
    event EventHandler<DtmfReceivedEventArgs> DtmfReceived;
}

/// <summary>
/// Event args for DTMF received event
/// </summary>
public class DtmfReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The DTMF digit that was received
    /// </summary>
    public string Digit { get; private set; }

    /// <summary>
    /// Event args for DTMF received event
    /// </summary>
    /// <param name="digit">The DTMF digit that was received</param>
    public DtmfReceivedEventArgs(string digit)
    {
        Digit = digit;
    }
}