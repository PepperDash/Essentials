using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.VC
{
    public abstract class VcCodecBase : Device, IHasFeedback, IRoutingSinkWithSwitching, IUsageTracking, IHasDialer//, ICodecAudio
    {
        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion

        #region IRoutingInputs Members

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        #endregion

        public BoolFeedback InCallFeedback { get; protected set; }

        abstract protected Func<bool> InCallFeedbackFunc { get; }
        abstract protected Func<bool> TransmitMuteFeedbackFunc { get; }
        abstract protected Func<bool> ReceiveMuteFeedbackFunc { get; }
        abstract protected Func<bool> PrivacyModeFeedbackFunc { get; }

        public VcCodecBase(string key, string name)
            : base(key, name)
        {
            InCallFeedback = new BoolFeedback(InCallFeedbackFunc);
            ReceiveMuteIsOnFeedback = new BoolFeedback(ReceiveMuteFeedbackFunc);
            TransmitMuteIsOnFeedback = new BoolFeedback(TransmitMuteFeedbackFunc);
            PrivacyModeIsOnFeedback = new BoolFeedback(PrivacyModeFeedbackFunc);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();

            InCallFeedback.OutputChange += new EventHandler<EventArgs>(InCallFeedback_OutputChange);
        }

        void InCallFeedback_OutputChange(object sender, EventArgs e)
        {
            if (UsageTracker != null)
            {
                if (InCallFeedback.BoolValue)
                    UsageTracker.StartDeviceUsage();
                else
                    UsageTracker.EndDeviceUsage();
            }
        }

        public abstract void Dial();
        public abstract void EndCall();

        public virtual List<Feedback> Feedbacks
        {
            get
            {
                return new List<Feedback>
				{
					InCallFeedback,
                    ReceiveMuteIsOnFeedback,
                    TransmitMuteIsOnFeedback,
                    PrivacyModeIsOnFeedback
				};
            }
        }

        public abstract void ExecuteSwitch(object selector);

        #region ICodecAudio Members

        public IntFeedback ReceiveLevelFeedback { get; private set; }
        public BoolFeedback ReceiveMuteIsOnFeedback { get; private set; }
        public abstract void ReceiveMuteOff();
        public abstract void ReceiveMuteOn();
        public abstract void ReceiveMuteToggle();
        public abstract void SetReceiveVolume(ushort level);

        public IntFeedback TransmitLevelFeedback { get; private set; }
        public BoolFeedback TransmitMuteIsOnFeedback { get; private set; }
        public abstract void TransmitMuteOff();
        public abstract void TransmitMuteOn();
        public abstract void TransmitMuteToggle();
        public abstract void SetTransmitVolume(ushort level);

        public abstract void PrivacyModeOn();
        public abstract void PrivacyModeOff();
        public abstract void PrivacyModeToggle();
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

        #endregion
    }
}