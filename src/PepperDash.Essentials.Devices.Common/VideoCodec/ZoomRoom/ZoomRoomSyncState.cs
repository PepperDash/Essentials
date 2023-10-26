using System;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    /// <summary>
    /// Tracks the initial sycnronization state when establishing a new connection
    /// </summary>
    public class ZoomRoomSyncState : IKeyed
    {
        private readonly ZoomRoom _parent;
        private readonly CrestronQueue<string> _syncQueries;
        private bool _initialSyncComplete;

        public ZoomRoomSyncState(string key, ZoomRoom parent)
        {
            _parent      = parent;
            Key          = key;
            _syncQueries = new CrestronQueue<string>(50);
            CodecDisconnected();
        }

        public bool InitialSyncComplete
        {
            get { return _initialSyncComplete; }
            private set
            {
                if (value)
                {
                    var handler = InitialSyncCompleted;
                    if (handler != null)
                    {
                        handler(this, new EventArgs());
                    }
                }
                _initialSyncComplete = value;
            }
        }

        public bool LoginResponseWasReceived { get; private set; }

        public bool FirstJsonResponseWasReceived { get; private set; }

        public bool InitialQueryMessagesWereSent { get; private set; }

        public bool LastQueryResponseWasReceived { get; private set; }

        public bool CamerasHaveBeenSetUp { get; private set; }

        #region IKeyed Members

        public string Key { get; private set; }

        #endregion

        public event EventHandler<EventArgs> InitialSyncCompleted;

        public event EventHandler FirstJsonResponseReceived;

        public void StartSync()
        {
            DequeueQueries();
        }

        private void DequeueQueries()
        {
            while (!_syncQueries.IsEmpty)
            {
                var query = _syncQueries.Dequeue();

                _parent.SendText(query);
            }

            InitialQueryMessagesSent();
        }

        public void AddQueryToQueue(string query)
        {
            _syncQueries.Enqueue(query);
        }

        public void LoginResponseReceived()
        {
            LoginResponseWasReceived = true;
            Debug.Console(1, this, "Login Rsponse Received.");
            CheckSyncStatus();
        }

        public void ReceivedFirstJsonResponse()
        {
            FirstJsonResponseWasReceived = true;
            Debug.Console(1, this, "First JSON Response Received.");

            var handler = FirstJsonResponseReceived;
            if (handler != null)
            {
                handler(this, null);
            }
            CheckSyncStatus();
        }

        public void InitialQueryMessagesSent()
        {
            InitialQueryMessagesWereSent = true;
            Debug.Console(1, this, "Query Messages Sent.");
            CheckSyncStatus();
        }

        public void LastQueryResponseReceived()
        {
            LastQueryResponseWasReceived = true;
            Debug.Console(1, this, "Last Query Response Received.");
            CheckSyncStatus();
        }

        public void CamerasSetUp()
        {
            CamerasHaveBeenSetUp = true;
            Debug.Console(1, this, "Cameras Set Up.");
            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            _syncQueries.Clear();
            LoginResponseWasReceived     = false;
            FirstJsonResponseWasReceived = false;
            InitialQueryMessagesWereSent = false;
            LastQueryResponseWasReceived = false;
            CamerasHaveBeenSetUp         = false;
            InitialSyncComplete          = false;
        }

        private void CheckSyncStatus()
        {
            if (LoginResponseWasReceived && FirstJsonResponseWasReceived && InitialQueryMessagesWereSent && LastQueryResponseWasReceived &&
                CamerasHaveBeenSetUp)
            {
                InitialSyncComplete = true;
                Debug.Console(1, this, "Initial Codec Sync Complete!");
            }
            else
            {
                InitialSyncComplete = false;
            }
        }
    }
}