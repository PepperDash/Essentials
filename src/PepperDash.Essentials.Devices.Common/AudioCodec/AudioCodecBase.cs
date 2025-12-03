using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    /// <summary>
    /// Abstract base class for audio codec devices
    /// </summary>
    public abstract class AudioCodecBase : EssentialsDevice, IHasDialer, IUsageTracking, IAudioCodecInfo
    {

        /// <summary>
        /// Event fired when call status changes
        /// </summary>
        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        /// <summary>
        /// Gets or sets the CodecInfo
        /// </summary>
        public AudioCodecInfo CodecInfo { get; protected set; }

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion

        /// <summary>
        /// Returns true when any call is not in state Unknown, Disconnecting, Disconnected
        /// </summary>
        public bool IsInCall
        {
            get
            {
                bool value;

                if (ActiveCalls != null)
                    value = ActiveCalls.Any(c => c.IsActiveCall);
                else
                    value = false;
                return value;
            }
        }

        // In most cases only a single call can be active
        /// <summary>
        /// Gets or sets the ActiveCalls
        /// </summary>
        public List<CodecActiveCallItem> ActiveCalls { get; set; }

        /// <summary>
        /// Constructor for AudioCodecBase
        /// </summary>
        /// <param name="key">Device key</param>
        /// <param name="name">Device name</param>
        public AudioCodecBase(string key, string name)
            : base(key, name)
        {
            ActiveCalls = new List<CodecActiveCallItem>();
        }

        /// <summary>
        /// Helper method to fire CallStatusChange event with old and new status
        /// </summary>
        protected void SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus newStatus, CodecActiveCallItem call)
        {
            call.Status = newStatus;

            OnCallStatusChange(call);

        }

        /// <summary>
        /// Handles call status change events
        /// </summary>
        /// <param name="item">The call item that changed status</param>
        protected void OnCallStatusChange(CodecActiveCallItem item)
        {
            var handler = CallStatusChange;
            if (handler != null)
                handler(this, new CodecCallStatusItemChangeEventArgs(item));

            if (UsageTracker != null)
            {
                if (IsInCall && !UsageTracker.UsageTrackingStarted)
                    UsageTracker.StartDeviceUsage();
                else if (UsageTracker.UsageTrackingStarted && !IsInCall)
                    UsageTracker.EndDeviceUsage();
            }
        }

        #region IHasDialer Members

        /// <inheritdoc />
        public abstract void Dial(string number);

        /// <inheritdoc />
        public abstract void EndCall(CodecActiveCallItem activeCall);

        /// <inheritdoc />
        public abstract void EndAllCalls();

        /// <inheritdoc />
        public abstract void AcceptCall(CodecActiveCallItem item);

        /// <inheritdoc />
        public abstract void RejectCall(CodecActiveCallItem item);

        /// <inheritdoc />
        public abstract void SendDtmf(string digit);

        #endregion
    }
}