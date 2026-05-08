using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices implementing <see cref="IHasStandbyMode"/>
    /// </summary>
    public class IHasStandbyModeMessenger : MessengerBase
    {
        private readonly IHasStandbyMode _standby;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasStandbyModeMessenger"/> class.
        /// </summary>
        public IHasStandbyModeMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _standby = device as IHasStandbyMode ?? throw new ArgumentNullException(nameof(device));
            _standby.StandbyIsOnFeedback.OutputChange += StandbyIsOnFeedback_OutputChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));
            AddAction("/standbyStatus", (id, content) => SendFullStatus(id));

            AddAction("/standbyOn", (id, content) => _standby.StandbyActivate());
            AddAction("/standbyOff", (id, content) => _standby.StandbyDeactivate());
        }

        private void StandbyIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasStandbyModeStateMessage
                {
                    StandbyIsOn = e.BoolValue
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting standby state");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasStandbyModeStateMessage
                {
                    StandbyIsOn = _standby.StandbyIsOnFeedback.BoolValue
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending standby full status");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IHasStandbyMode"/>
    /// </summary>
    public class IHasStandbyModeStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Indicates whether the device is in standby mode. Null if unknown or not applicable.
         /// </summary>
        [JsonProperty("standbyIsOn", NullValueHandling = NullValueHandling.Ignore)]
        public bool? StandbyIsOn { get; set; }
    }
}
