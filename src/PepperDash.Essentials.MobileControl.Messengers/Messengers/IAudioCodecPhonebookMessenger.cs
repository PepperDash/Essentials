using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using System.Collections.Generic;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IAudioCodecPhonebook"/>
    /// </summary>
    public class IAudioCodecPhonebookMessenger : MessengerBase
    {
        private readonly IAudioCodecPhonebook _phonebook;

        /// <summary>
        /// Initializes a new instance of the <see cref="IAudioCodecPhonebookMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IAudioCodecPhonebookMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _phonebook = device as IAudioCodecPhonebook ?? throw new ArgumentNullException(nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/phonebookStatus", (id, content) => SendFullStatus(id));

            AddAction("/setEntry", (id, content) =>
            {
                var entry = content.ToObject<SetPhonebookEntryContent>();
                _phonebook.SetPhonebookEntry(entry.Index, entry.Name, entry.Number);
                SendFullStatus();
            });
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IAudioCodecPhonebookStateMessage
                {
                    PhonebookEntries = _phonebook.PhonebookEntries
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending phonebook full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IAudioCodecPhonebook"/>
    /// </summary>
    public class IAudioCodecPhonebookStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of phonebook entries
        /// </summary>
        [JsonProperty("phonebookEntries", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecPhonebookEntry> PhonebookEntries { get; set; }
    }

    /// <summary>
    /// Content model for the setEntry action
    /// </summary>
    public class SetPhonebookEntryContent
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
