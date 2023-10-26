using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
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
            NumberOfContacts            = contacts;
            NumberOfContactsWasReceived = true;

            Debug.Console(1, this, "Phonebook contains {0} contacts.", NumberOfContacts);

            CheckSyncStatus();
        }

        public void CodecDisconnected()
        {
            InitialPhonebookFoldersWasReceived = false;
            PhonebookHasFolders                = false;
            NumberOfContacts                   = 0;
            NumberOfContactsWasReceived        = false;
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