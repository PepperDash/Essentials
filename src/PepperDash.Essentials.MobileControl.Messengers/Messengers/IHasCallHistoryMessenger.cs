using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasCallHistory"/>
    /// </summary>
    public class IHasCallHistoryMessenger : MessengerBase
    {
        private readonly IHasCallHistory _callHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasCallHistoryMessenger"/> class.
        /// </summary>
        public IHasCallHistoryMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _callHistory = device as IHasCallHistory ?? throw new ArgumentException("device must implement IHasCallHistory", nameof(device));
            _callHistory.CallHistory.RecentCallsListHasChanged += CallHistory_RecentCallsListHasChanged;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => PostCallHistory(id));
            AddAction("/callHistoryStatus", (id, content) => PostCallHistory(id));

            AddAction("/getCallHistory", (id, content) => PostCallHistory(id));
        }

        private void CallHistory_RecentCallsListHasChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is CodecCallHistory codecCallHistory)) return;

                var recents = codecCallHistory.RecentCalls;
                if (recents != null)
                {
                    PostStatusMessage(new IHasCallHistoryStateMessage
                    {
                        RecentCalls = recents
                    });
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting call history");
            }
        }

        private void PostCallHistory(string id = null)
        {
            try
            {
                var recents = _callHistory.CallHistory.RecentCalls;
                if (recents != null)
                {
                    PostStatusMessage(new IHasCallHistoryStateMessage
                    {
                        RecentCalls = recents,
                    }, id);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting call history");
            }
        }
    }

    /// <summary>
    /// State message for <see cref="IHasCallHistory"/>
    ///
    /// </summary>
    public class IHasCallHistoryStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of recent calls. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("recentCalls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecCallHistory.CallHistoryEntry> RecentCalls { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device has call history functionality. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("hasRecents", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRecents { get; set; } = true; // Since this messenger should only be used for devices with call history, default to true unless specified otherwise.
    }
}
