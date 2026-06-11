using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.AudioCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IAudioCodecInfo"/>
    /// </summary>
    public class IAudioCodecInfoMessenger : MessengerBase
    {
        private readonly IAudioCodecInfo _codecInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IAudioCodecInfoMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IAudioCodecInfoMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _codecInfo = device as IAudioCodecInfo ?? throw new ArgumentNullException(nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/codecInfoStatus", (id, content) => SendFullStatus(id));
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IAudioCodecInfoStateMessage
                {
                    PhoneNumber = _codecInfo.CodecInfo?.PhoneNumber
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending audio codec info full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IAudioCodecInfo"/>
    /// </summary>
    public class IAudioCodecInfoStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the phone number of the audio codec
        /// </summary>
        [JsonProperty("phoneNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber { get; set; }
    }
}
