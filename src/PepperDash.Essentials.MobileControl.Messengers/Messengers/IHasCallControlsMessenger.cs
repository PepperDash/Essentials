using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Provides a messaging bridge for devices implementing <see cref="ICodecCallControls"/>
    /// </summary>
    public class ICallControlsMessenger : MessengerBase
    {
        private readonly ICodecCallControls _callControls;

        /// Initializes a new instance of the <see cref="ICallControlsMessenger"/> class.
        public ICallControlsMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _callControls = device as ICodecCallControls ?? throw new ArgumentNullException(nameof(device));
            _callControls.CallStatusChange += CallControls_CallStatusChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus(id));

            AddAction("/callControlsStatus", (id, content) => SendFullStatus(id));

            AddAction("/dialMeeting", (id, content) =>
                _callControls.Dial(content.ToObject<Meeting>()));

            AddAction("/endCallById", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(s.Value);
                if (call != null)
                    _callControls.EndCall(call);
            });

            AddAction("/rejectById", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(s.Value);
                if (call != null)
                    _callControls.RejectCall(call);
            });

            AddAction("/acceptById", (id, content) =>
            {
                var s = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(s.Value);
                if (call != null)
                    _callControls.AcceptCall(call);
            });
        }

        private void CallControls_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            try
            {
                PostStatusMessage(BuildState());
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting call controls status");
            }
        }

        private void SendFullStatus(string id = null)
        {
            try
            {
                Task.Run(() => PostStatusMessage(BuildState(), id));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending call controls full status");
            }
        }

        private ICallControlsStateMessage BuildState()
        {
            return new ICallControlsStateMessage
            {
                Calls = _callControls.ActiveCalls,
            };
        }

        private CodecActiveCallItem GetCallWithId(string id)
        {
            return _callControls.ActiveCalls?.FirstOrDefault(c => c.Id == id);
        }
    }

    /// <summary>
    /// State message for <see cref="ICodecCallControls"/>
    /// </summary>
    public class ICallControlsStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the list of active calls. Null if unknown or not applicable.
        /// </summary>
        [JsonProperty("calls", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodecActiveCallItem> Calls { get; set; }

    }
}
