using Newtonsoft.Json;
using System;
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
    /// Raised when the list of phonebook entries changes. The event args contain the updated list of entries.
    /// </summary>
    event EventHandler<PhonebookListChangedEventArgs> ListChanged;

    /// <summary>
    /// Sets a phonebook entry at the specified index. The implementation of this method is up to the device, but it should update the phonebook entry at the specified index with the provided name and number.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="name"></param>
    /// <param name="number"></param>
    void SetPhonebookEntry(int index, string name, string number);

    /// <summary>
    /// Dials the phonebook entry at the specified index.
    /// </summary>
    /// <param name="index"></param>
    void DialPhonebookEntry(int index);

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

/// <summary>
/// Provides the updated list of phonebook entries for the <see cref="IAudioCodecPhonebook.ListChanged"/> event.
/// </summary>
public class PhonebookListChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the updated list of phonebook entries.
    /// </summary>
    public List<CodecPhonebookEntry> Entries { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhonebookListChangedEventArgs"/> class.
    /// </summary>
    /// <param name="entries">The updated list of phonebook entries.</param>
    public PhonebookListChangedEventArgs(List<CodecPhonebookEntry> entries)
    {
        Entries = entries;
    }
}