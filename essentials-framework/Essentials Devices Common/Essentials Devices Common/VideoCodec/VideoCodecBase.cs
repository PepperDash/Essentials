using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Codec;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public abstract class VideoCodecBase : ReconfigurableDevice, IRoutingInputsOutputs,
        IUsageTracking, IHasDialer, IHasContentSharing, ICodecAudio, iVideoCodecInfo, IBridgeAdvanced
    {
        protected VideoCodecBase(DeviceConfig config)
            : base(config)
        {
            StandbyIsOnFeedback = new BoolFeedback(StandbyIsOnFeedbackFunc);
            PrivacyModeIsOnFeedback = new BoolFeedback(PrivacyModeIsOnFeedbackFunc);
            VolumeLevelFeedback = new IntFeedback(VolumeLevelFeedbackFunc);
            MuteFeedback = new BoolFeedback(MuteFeedbackFunc);
            SharingSourceFeedback = new StringFeedback(SharingSourceFeedbackFunc);
            SharingContentIsOnFeedback = new BoolFeedback(SharingContentIsOnFeedbackFunc);

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ActiveCalls = new List<CodecActiveCallItem>();
        }

        public IBasicCommunication Communication { get; protected set; }

        /// <summary>
        /// An internal pseudo-source that is routable and connected to the osd input
        /// </summary>
        public DummyRoutingInputsDevice OsdSource { get; protected set; }

        public BoolFeedback StandbyIsOnFeedback { get; private set; }

        protected abstract Func<bool> PrivacyModeIsOnFeedbackFunc { get; }
        protected abstract Func<int> VolumeLevelFeedbackFunc { get; }
        protected abstract Func<bool> MuteFeedbackFunc { get; }
        protected abstract Func<bool> StandbyIsOnFeedbackFunc { get; }

        public List<CodecActiveCallItem> ActiveCalls { get; set; }

        public bool ShowSelfViewByDefault { get; protected set; }


        public bool IsReady { get; protected set; }

        public virtual List<Feedback> Feedbacks
        {
            get
            {
                return new List<Feedback>
                {
                    PrivacyModeIsOnFeedback,
                    SharingSourceFeedback
                };
            }
        }

        protected abstract Func<string> SharingSourceFeedbackFunc { get; }
        protected abstract Func<bool> SharingContentIsOnFeedbackFunc { get; }

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

        #region IHasContentSharing Members

        public abstract void StartSharing();
        public abstract void StopSharing();

        public bool AutoShareContentWhileInCall { get; protected set; }

        public StringFeedback SharingSourceFeedback { get; private set; }
        public BoolFeedback SharingContentIsOnFeedback { get; private set; }

        #endregion

        #region IHasDialer Members

        /// <summary>
        /// Fires when the status of any active, dialing, or incoming call changes or is new
        /// </summary>
        public event EventHandler<CodecCallStatusItemChangeEventArgs> CallStatusChange;

        /// <summary>
        /// Returns true when any call is not in state Unknown, Disconnecting, Disconnected
        /// </summary>
        public bool IsInCall
        {
            get
            {
                bool value;

                if (ActiveCalls != null)
                {
                    value = ActiveCalls.Any(c => c.IsActiveCall);
                }
                else
                {
                    value = false;
                }
                return value;
            }
        }

        public abstract void Dial(string number);
        public abstract void EndCall(CodecActiveCallItem call);
        public abstract void EndAllCalls();
        public abstract void AcceptCall(CodecActiveCallItem call);
        public abstract void RejectCall(CodecActiveCallItem call);
        public abstract void SendDtmf(string s);

        #endregion

        #region IRoutingInputsOutputs Members

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        #endregion

        #region IUsageTracking Members

        /// <summary>
        /// This object can be added by outside users of this class to provide usage tracking
        /// for various services
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion

        #region iVideoCodecInfo Members

        public VideoCodecInfo CodecInfo { get; protected set; }

        #endregion

        public event EventHandler<EventArgs> IsReadyChange;
        public abstract void Dial(Meeting meeting);

        public virtual void Dial(IInvitableContact contact)
        {
        }

        public abstract void ExecuteSwitch(object selector);

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
            {
                handler(this, new CodecCallStatusItemChangeEventArgs(item));
            }

            if (AutoShareContentWhileInCall)
            {
                StartSharing();
            }

            if (UsageTracker != null)
            {
                if (IsInCall && !UsageTracker.UsageTrackingStarted)
                {
                    UsageTracker.StartDeviceUsage();
                }
                else if (UsageTracker.UsageTrackingStarted && !IsInCall)
                {
                    UsageTracker.EndDeviceUsage();
                }
            }
        }

        /// <summary>
        /// Sets IsReady property and fires the event. Used for dependent classes to sync up their data.
        /// </summary>
        protected void SetIsReady()
        {
            IsReady = true;
            var h = IsReadyChange;
            if (h != null)
            {
                h(this, new EventArgs());
            }
        }

        // **** DEBUGGING THINGS ****
        /// <summary>
        /// 
        /// </summary>
        public virtual void ListCalls()
        {
            var sb = new StringBuilder();
            foreach (var c in ActiveCalls)
            {
                sb.AppendFormat("{0} {1} -- {2} {3}\n", c.Id, c.Number, c.Name, c.Status);
            }
            Debug.Console(1, this, "\n{0}\n", sb.ToString());
        }

        public abstract void StandbyActivate();

        public abstract void StandbyDeactivate();

        #region Implementation of IBridgeAdvanced

        public abstract void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge);

        #endregion
    }


    /// <summary>
    /// Used to track the status of syncronizing the phonebook values when connecting to a codec or refreshing the phonebook info
    /// </summary>
    public class CodecPhonebookSyncState : IKeyed
    {
        private bool _InitialSyncComplete;

        public CodecPhonebookSyncState(string key)
        {
            Key = key;

            CodecDisconnected();
        }

        public bool InitialSyncComplete
        {
            get { return _InitialSyncComplete; }
            private set
            {
                if (value == true)
                {
                    var handler = InitialSyncCompleted;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                }
                _InitialSyncComplete = value;
            }
        }

        public bool InitialPhonebookFoldersWasReceived { get; private set; }

        public bool NumberOfContactsWasReceived { get; private set; }

        public bool PhonebookRootEntriesWasRecieved { get; private set; }

        public bool PhonebookHasFolders { get; private set; }

        public int NumberOfContacts { get; private set; }

        #region IKeyed Members

        public string Key { get; private set; }

        #endregion

        public event EventHandler<EventArgs> InitialSyncCompleted;

        public void InitialPhonebookFoldersReceived()
        {
            InitialPhonebookFoldersWasReceived = true;

            CheckSyncStatus();
        }

        public void PhonebookRootEntriesReceived()
        {
            PhonebookRootEntriesWasRecieved = true;

            CheckSyncStatus();
        }

        public void SetPhonebookHasFolders(bool value)
        {
            PhonebookHasFolders = value;

            Debug.Console(1, this, "Phonebook has folders: {0}", PhonebookHasFolders);
        }

        public void SetNumberOfContacts(int contacts)
        {
            NumberOfContacts = contacts;
            NumberOfContactsWasReceived = true;

            Debug.Console(1, this, "Phonebook contains {0} contacts.", NumberOfContacts);

            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            InitialPhonebookFoldersWasReceived = false;
            PhonebookHasFolders = false;
            NumberOfContacts = 0;
            NumberOfContactsWasReceived = false;
        }

        private void CheckSyncStatus()
        {
            if (InitialPhonebookFoldersWasReceived && NumberOfContactsWasReceived && PhonebookRootEntriesWasRecieved)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Phonebook Sync Complete!");
            }
            else
            {
                InitialSyncComplete = false;
            }
        }
    }
}