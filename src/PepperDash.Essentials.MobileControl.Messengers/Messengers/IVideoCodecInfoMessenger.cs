using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IVideoCodecInfo"/>
    /// </summary>
    public class IVideoCodecInfoMessenger : MessengerBase
    {
        private readonly IVideoCodecInfo _codecInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IVideoCodecInfoMessenger"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IVideoCodecInfoMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _codecInfo = device as IVideoCodecInfo ?? throw new ArgumentNullException(nameof(device));
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
                var state = new iVideoCodecInfoStateMessage
                {
                    Info = _codecInfo.CodecInfo
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending codec info full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IVideoCodecInfo"/>
    /// </summary>
    public class iVideoCodecInfoStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the codec information. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public VideoCodecInfo Info { get; set; }
    }
}
