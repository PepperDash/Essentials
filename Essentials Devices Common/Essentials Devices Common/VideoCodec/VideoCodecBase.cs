using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public abstract class VideoCodecBase : Device, IRoutingInputsOutputs, IUsageTracking, IHasDialer, IHasSharing, ICodecAudio, iCodecInfo
    {
        /// <summary>
        /// Fires when the status of any active, dialing, or incoming call changes or is new
        /// </summary>
        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        public event EventHandler<EventArgs> IsReadyChange;

        #region IUsageTracking Members

        /// <summary>
        /// This object can be added by outside users of this class to provide usage tracking
        /// for various services
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        /// <summary>
        /// Returns true when any call is not in state Unknown, Disconnecting, Disconnected
        /// </summary>
        public bool IsInCall { get { return ActiveCalls.Any(c => c.IsActiveCall); } }

        public BoolFeedback IncomingCallFeedback { get; private set; }

        abstract protected Func<bool> IncomingCallFeedbackFunc { get; }
        abstract protected Func<bool> PrivacyModeIsOnFeedbackFunc { get; }
        abstract protected Func<int> VolumeLevelFeedbackFunc { get; }
        abstract protected Func<bool> MuteFeedbackFunc { get; }
        abstract protected Func<string> SharingSourceFeedbackFunc { get; }

        public List<CodecActiveCallItem> ActiveCalls { get; set; }

        public VideoCodecInfo CodecInfo { get; protected set; }

        public bool IsReady { get; protected set; }

        public VideoCodecBase(string key, string name)
            : base(key, name)
        {
            IncomingCallFeedback = new BoolFeedback(IncomingCallFeedbackFunc);
            PrivacyModeIsOnFeedback = new BoolFeedback(PrivacyModeIsOnFeedbackFunc);
            VolumeLevelFeedback = new IntFeedback(VolumeLevelFeedbackFunc);
            MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
            SharingSourceFeedback = new StringFeedback(SharingSourceFeedbackFunc);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ActiveCalls = new List<CodecActiveCallItem>();
        }

        #region IHasDialer Members

        public abstract void Dial(string s);
        public abstract void EndCall(CodecActiveCallItem call);
        public abstract void EndAllCalls();
        public abstract void AcceptCall(CodecActiveCallItem call);
        public abstract void RejectCall(CodecActiveCallItem call);
        public abstract void SendDtmf(string s);

        #endregion

        public virtual List<Feedback> Feedbacks
        {
            get
            {
                return new List<Feedback>
				{
                    IncomingCallFeedback,
                    PrivacyModeIsOnFeedback,
                    SharingSourceFeedback
				};
            }
        }

        public abstract void ExecuteSwitch(object selector);

        /// <summary>
        /// Helper method to fire CallStatusChange event with old and new status
        /// </summary>
        protected void SetNewCallStatusAndFireCallStatusChange(eCodecCallStatus newStatus, CodecActiveCallItem call)
        {
            var prevStatus = call.Status;
            call.Status = newStatus;
            OnCallStatusChange(prevStatus, newStatus, call);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousStatus"></param>
        /// <param name="newStatus"></param>
        /// <param name="item"></param>
        protected void OnCallStatusChange(eCodecCallStatus previousStatus, eCodecCallStatus newStatus, CodecActiveCallItem item)
        {
            var handler = CallStatusChange;
            if (handler != null)
                handler(this, new CodecCallStatusItemChangeEventArgs(previousStatus, newStatus, item));
        }

        /// <summary>
        /// Sets IsReady property and fires the event. Used for dependent classes to sync up their data.
        /// </summary>
        protected void SetIsReady()
        {
            IsReady = true;
            var h = IsReadyChange;
            if(h != null)
                h(this, new EventArgs());
        }

        #region ICodecAudio Members

        public abstract void PrivacyModeOn();
        public abstract void PrivacyModeOff();
        public abstract void PrivacyModeToggle();
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }


        public BoolFeedback MuteFeedback { get; private set; }

        public abstract void MuteOff();

        public abstract void MuteOn();

        public abstract void SetVolume(ushort level);

        public IntFeedback VolumeLevelFeedback { get; private set; }

        public abstract void MuteToggle();

        public abstract void VolumeDown(bool pressRelease);


        public abstract void VolumeUp(bool pressRelease);

        #endregion

        #region IHasSharing Members

        public abstract void StartSharing();
        public abstract void StopSharing();

        public StringFeedback SharingSourceFeedback { get; private set; }

        #endregion

        // **** DEBUGGING THINGS ****
        /// <summary>
        /// 
        /// </summary>
        public virtual void ListCalls()
        {
            var sb = new StringBuilder();
            foreach (var c in ActiveCalls)
                sb.AppendFormat("{0} {1} -- {2} {3}\r", c.Id, c.Number, c.Name, c.Status);
            Debug.Console(1, "{0}", sb.ToString());
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class CodecCallStatusItemChangeEventArgs : EventArgs
    {
        public CodecActiveCallItem CallItem { get; private set; }

        public eCodecCallStatus PreviousStatus { get; private set; }

        public eCodecCallStatus NewStatus { get; private set; }

        public CodecCallStatusItemChangeEventArgs(eCodecCallStatus previousStatus,
             eCodecCallStatus newStatus, CodecActiveCallItem item)
        {
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            CallItem = item;
        }
    }
}