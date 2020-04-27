using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.AudioCodec
{
    public abstract class AudioCodecBase : EssentialsDevice, IHasDialer, IUsageTracking, IAudioCodecInfo
    {

        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        public AudioCodecInfo CodecInfo { get; protected set; }

        #region IUsageTracking Members

        /// <summary>
        /// This object can be added by outside users of this class to provide usage tracking
        /// for various services
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
        public List<CodecActiveCallItem> ActiveCalls { get; set; }

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
        /// 
        /// </summary>
        /// <param name="previousStatus"></param>
        /// <param name="newStatus"></param>
        /// <param name="item"></param>
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

        public abstract void Dial(string number);

        public abstract void EndCall(CodecActiveCallItem activeCall);

        public abstract void EndAllCalls();

        public abstract void AcceptCall(CodecActiveCallItem item);

        public abstract void RejectCall(CodecActiveCallItem item);

        public abstract void SendDtmf(string digit);

        #endregion
    }
}