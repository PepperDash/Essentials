namespace PepperDash.Essentials.Devices.Common.AudioCodec;

public interface IDtmfDecode
{
    /// <summary>
    /// Event fired when a DTMF digit is received
    /// </summary>
    event EventHandler<DtmfReceivedEventArgs> DtmfReceived;
}

public class DtmfReceivedEventArgs : EventArgs
{
    public string Digit { get; private set; }

    public DtmfReceivedEventArgs(string digit)
    {
        Digit = digit;
    }
}