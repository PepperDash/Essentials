


using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

public interface IAudioCodecPhonebook : IHasDialer
{
    /// <summary>
    /// Gets the list of phonebook entries for the device.
    /// </summary>
    List<CodecPhonebookEntry> PhonebookEntries { get; }
}