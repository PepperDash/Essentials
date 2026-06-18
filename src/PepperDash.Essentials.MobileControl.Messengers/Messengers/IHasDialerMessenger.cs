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
    /// Provides a messaging bridge for devices implementing <see cref="IHasDialer"/>
    /// </summary>
    public class IHasDialerMessenger : MessengerBase
    {
        private readonly IHasDialer _dialer;

        ///
        public IHasDialerMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _dialer = device as IHasDialer ?? throw new ArgumentNullException(nameof(device));
            _dialer.CallStatusChange += Dialer_CallStatusChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/dialStatus", (id, content) => SendFullStatus(id));

            AddAction("/dial", (id, content) =>
            {
                var value = content.ToObject<MobileControlSimpleContent<string>>();
                _dialer.Dial(value.Value);
            });


            AddAction("/endAllCalls", (id, content) => _dialer.EndAllCalls());

            AddAction("/dtmf", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();
                _dialer.SendDtmf(s.Value);
            });

            AddAction("/acceptCall", (id, content) => 
            {
                var callItem = content.ToObject<CodecActiveCallItem>();
                _dialer.AcceptCall(callItem);                
            });

            AddAction("/rejectCall", (id, content) =>
            {
                var callItem = content.ToObject<CodecActiveCallItem>();
                _dialer.RejectCall(callItem);
            });
        }

        private void Dialer_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            try
            {
                var state = new IHasDialerStateMessage
                {
                    IsInCall = _dialer.IsInCall,
                    CallItem = e.CallItem
                };

                PostStatusMessage(state);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting dialer call status");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                var state = new IHasDialerStateMessage
                {
                    IsInCall = _dialer.IsInCall,
                };

                Task.Run(() => PostStatusMessage(state, id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending dialer full status");
            }
        }
    }

    /// <summary>
    /// Message class representing the state of a device implementing <see cref="IHasDialer"/>
    /// </summary>
    public class IHasDialerStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Indicates whether the device is currently in a call
        /// </summary>
        [JsonProperty("isInCall", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsInCall { get; set; }

        [JsonProperty("callItem", NullValueHandling = NullValueHandling.Ignore)]
        public CodecActiveCallItem CallItem { get; set; }
    }
}
