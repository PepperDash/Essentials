using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge for devices that implement call status interfaces
    /// without requiring VideoCodecBase inheritance
    /// </summary>
    public class CallStatusMessenger : MessengerBase
    {
        /// <summary>
        /// Device with dialer capabilities
        /// </summary>
        protected IHasDialer Dialer { get; private set; }

        /// <summary>
        /// Device with content sharing capabilities (optional)
        /// </summary>
        protected IHasContentSharing ContentSharing { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dialer"></param>
        /// <param name="messagePath"></param>
        public CallStatusMessenger(string key, IHasDialer dialer, string messagePath)
            : base(key, messagePath, dialer as IKeyName)
        {
            Dialer = dialer ?? throw new ArgumentNullException(nameof(dialer));
            dialer.CallStatusChange += Dialer_CallStatusChange;

            // Check for optional content sharing interface
            if (dialer is IHasContentSharing contentSharing)
            {
                ContentSharing = contentSharing;
                contentSharing.SharingContentIsOnFeedback.OutputChange += SharingContentIsOnFeedback_OutputChange;
                contentSharing.SharingSourceFeedback.OutputChange += SharingSourceFeedback_OutputChange;
            }
        }

        /// <summary>
        /// Handles call status changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dialer_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            try
            {
                SendFullStatus();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error handling call status change: {error}", ex.Message);
            }
        }

        /// <summary>
        /// Handles content sharing status changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharingContentIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                sharingContentIsOn = e.BoolValue
            }));
        }

        /// <summary>
        /// Handles sharing source changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharingSourceFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                sharingSource = e.StringValue
            }));
        }

        /// <summary>
        /// Gets active calls from the dialer
        /// </summary>
        /// <returns></returns>
        private object GetActiveCalls()
        {
            // Try to get active calls if the dialer has an ActiveCalls property
            var dialerType = Dialer.GetType();
            var activeCallsProperty = dialerType.GetProperty("ActiveCalls");
            
            if (activeCallsProperty != null && activeCallsProperty.PropertyType == typeof(System.Collections.Generic.List<CodecActiveCallItem>))
            {
                var activeCalls = activeCallsProperty.GetValue(Dialer) as System.Collections.Generic.List<CodecActiveCallItem>;
                return activeCalls ?? new System.Collections.Generic.List<CodecActiveCallItem>();
            }
            
            // Return basic call status if no ActiveCalls property
            return new { isInCall = Dialer.IsInCall };
        }

        /// <summary>
        /// Sends full status message
        /// </summary>
        public void SendFullStatus()
        {
            var status = new
            {
                isInCall = Dialer.IsInCall,
                calls = GetActiveCalls()
            };

            // Add content sharing status if available
            if (ContentSharing != null)
            {
                var statusWithSharing = new
                {
                    isInCall = Dialer.IsInCall,
                    calls = GetActiveCalls(),
                    sharingContentIsOn = ContentSharing.SharingContentIsOnFeedback.BoolValue,
                    sharingSource = ContentSharing.SharingSourceFeedback.StringValue
                };
                PostStatusMessage(JToken.FromObject(statusWithSharing));
            }
            else
            {
                PostStatusMessage(JToken.FromObject(status));
            }
        }

        /// <summary>
        /// Registers actions for call control
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());
            
            // Basic call control actions
            AddAction("/dial", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                Dialer.Dial(msg.Value);
            });

            AddAction("/endAllCalls", (id, content) => Dialer.EndAllCalls());

            AddAction("/dtmf", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                Dialer.SendDtmf(msg.Value);
            });

            // Call-specific actions (if active calls are available)
            AddAction("/endCallById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(msg.Value);
                if (call != null)
                    Dialer.EndCall(call);
            });

            AddAction("/acceptById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(msg.Value);
                if (call != null)
                    Dialer.AcceptCall(call);
            });

            AddAction("/rejectById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                var call = GetCallWithId(msg.Value);
                if (call != null)
                    Dialer.RejectCall(call);
            });

            // Content sharing actions if available
            if (ContentSharing != null)
            {
                AddAction("/startSharing", (id, content) => ContentSharing.StartSharing());
                AddAction("/stopSharing", (id, content) => ContentSharing.StopSharing());
            }
        }

        /// <summary>
        /// Finds a call by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private CodecActiveCallItem GetCallWithId(string id)
        {
            // Try to get call using reflection for ActiveCalls property
            var dialerType = Dialer.GetType();
            var activeCallsProperty = dialerType.GetProperty("ActiveCalls");
            
            if (activeCallsProperty != null && activeCallsProperty.PropertyType == typeof(System.Collections.Generic.List<CodecActiveCallItem>))
            {
                var activeCalls = activeCallsProperty.GetValue(Dialer) as System.Collections.Generic.List<CodecActiveCallItem>;
                if (activeCalls != null)
                {
                    return activeCalls.FirstOrDefault(c => c.Id.Equals(id));
                }
            }
            
            return null;
        }
    }
}