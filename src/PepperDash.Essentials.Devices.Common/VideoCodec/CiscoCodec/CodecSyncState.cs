using System;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    /// <summary>
    /// Tracks the initial sycnronization state of the codec when making a connection
    /// </summary>
    public class CodecSyncState : IKeyed
    {
        bool _InitialSyncComplete;
        private readonly CiscoSparkCodec _parent;

        public event EventHandler<EventArgs> InitialSyncCompleted;
        private readonly CrestronQueue<string> _commandQueue;

        public string Key { get; private set; }

        public bool InitialSyncComplete 
        {
            get { return _InitialSyncComplete; }
            private set
            {
                if (value == true)
                {
                    var handler = InitialSyncCompleted;
                    if (handler != null)
                        handler(this, new EventArgs());
                }
                _InitialSyncComplete = value;
            }
        }

        public bool LoginMessageWasReceived { get; private set; }

        public bool JsonResponseModeSet { get; private set; }

        public bool InitialStatusMessageWasReceived { get; private set; }

        public bool InitialConfigurationMessageWasReceived { get; private set; }

        public bool FeedbackWasRegistered { get; private set; }

        public CodecSyncState(string key, CiscoSparkCodec parent)
        {
            Key           = key;
            _parent       = parent;
            _commandQueue = new CrestronQueue<string>(50);
            CodecDisconnected();
        }

        private void ProcessQueuedCommands()
        {
            while (InitialSyncComplete)
            {
                var query = _commandQueue.Dequeue();

                _parent.SendText(query);
            }
        }

        public void AddCommandToQueue(string query)
        {
            _commandQueue.Enqueue(query);
        }

        public void LoginMessageReceived()
        {
            LoginMessageWasReceived = true;
            Debug.Console(1, this, "Login Message Received.");
            CheckSyncStatus();
        }

        public void JsonResponseModeMessageReceived()
        {
            JsonResponseModeSet = true;
            Debug.Console(1, this, "Json Response Mode Message Received.");
            CheckSyncStatus();
        }

        public void InitialStatusMessageReceived()
        {
            InitialStatusMessageWasReceived = true;
            Debug.Console(1, this, "Initial Codec Status Message Received.");
            CheckSyncStatus();
        }

        public void InitialConfigurationMessageReceived()
        {
            InitialConfigurationMessageWasReceived = true;
            Debug.Console(1, this, "Initial Codec Configuration Message Received.");
            CheckSyncStatus();
        }

        public void FeedbackRegistered()
        {
            FeedbackWasRegistered = true;
            Debug.Console(1, this, "Initial Codec Feedback Registration Successful.");
            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            _commandQueue.Clear();
            LoginMessageWasReceived                = false;
            JsonResponseModeSet                    = false;
            InitialConfigurationMessageWasReceived = false;
            InitialStatusMessageWasReceived        = false;
            FeedbackWasRegistered                  = false;
            InitialSyncComplete                    = false;
        }

        void CheckSyncStatus()
        {
            if (LoginMessageWasReceived && JsonResponseModeSet  && InitialConfigurationMessageWasReceived && InitialStatusMessageWasReceived && FeedbackWasRegistered)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
                Debug.Console(1, this, "{0} Command queued. Processing now...", _commandQueue.Count);

                // Invoke a thread for the queue
                CrestronInvoke.BeginInvoke((o) => { 
                    ProcessQueuedCommands();
                });
            }
            else
                InitialSyncComplete = false;
        }
    }
}