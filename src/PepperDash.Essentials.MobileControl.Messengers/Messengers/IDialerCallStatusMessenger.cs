using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for an IDialerCallStatus device
    /// </summary>
    public class IDialerCallStatusMessenger : MessengerBase
    {
        /// <summary>
        /// Device being bridged
        /// </summary>
        public IDialerCallStatus Codec { get; private set; }

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="codec"></param>
        /// <param name="messagePath"></param>
        public IDialerCallStatusMessenger(string key, IDialerCallStatus codec, string messagePath)
            : base(key, messagePath, codec as IKeyName)
        {
            Codec = codec ?? throw new ArgumentNullException("codec");
            codec.CallStatusChange += Codec_CallStatusChange;
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendAtcFullMessageObject(id));

            AddAction("/audioDialerStatus", (id, content) => SendAtcFullMessageObject(id));

            AddAction("/dial", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();

                Codec.Dial(msg.Value);
            });

            AddAction("/endCallById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();

                var call = GetCallWithId(msg.Value);
                if (call != null)
                    Codec.EndCall(call);
            });

            AddAction("/endAllCalls", (id, content) => Codec.EndAllCalls());
            AddAction("/dtmf", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();

                Codec.SendDtmf(msg.Value);
            });

            AddAction("/rejectById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();

                var call = GetCallWithId(msg.Value);

                if (call != null)
                    Codec.RejectCall(call);
            });

            AddAction("/acceptById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(msg.Value);
                if (call != null)
                    Codec.AcceptCall(call);
            });
        }

        /// <summary>
        /// Helper to grab a call with string ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private CodecActiveCallItem GetCallWithId(string id)
        {
            return Codec.ActiveCalls.FirstOrDefault(c => c.Id == id);
        }

        private void Codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            SendAtcFullMessageObject();
        }

        /// <summary>
        /// Helper method to build call status for vtc
        /// </summary>
        /// <returns></returns>
        private void SendAtcFullMessageObject(string id = null)
        {
            try
            {
                var info = Codec.CodecInfo;

                PostStatusMessage(JToken.FromObject(new
                {
                    isInCall = Codec.IsInCall,
                    calls = Codec.ActiveCalls,
                    info = new
                    {
                        phoneNumber = info?.PhoneNumber
                    }
                }), id
                );
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending dialer call status full message");
            }
        }
    }
}