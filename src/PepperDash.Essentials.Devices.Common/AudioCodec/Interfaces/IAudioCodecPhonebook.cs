using Newtonsoft.Json;
using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec;

/// <summary>
/// Defines the contract for a device that has a phonebook. 
/// This is used to provide a common interface for devices that have phonebook functionality
/// </summary>
public interface IAudioCodecPhonebook : IHasDialer
{
    /// <summary>
    /// Sets a phonebook entry at the specified index. The implementation of this method is up to the device, but it should update the phonebook entry at the specified index with the provided name and number.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="name"></param>
    /// <param name="number"></param>
    void SetPhonebookEntry(int index, string name, string number);

    /// <summary>
    /// Gets the list of phonebook entries for the device.
    /// </summary>
    List<CodecPhonebookEntry> PhonebookEntries { get; }
}

/// <summary>
/// Defines a phonebook entry for the audio codec phonebook. 
/// This is used to provide a common data structure for phonebook entries across different devices.
/// </summary>
public class CodecPhonebookEntry
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }
}