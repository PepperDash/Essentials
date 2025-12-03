using System;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
  /// <summary>
  /// Represents a CodecPhonebookSyncState
  /// </summary>
  public class CodecPhonebookSyncState : IKeyed
  {
    private bool _InitialSyncComplete;

    /// <summary>
    /// Constructor for CodecPhonebookSyncState
    /// </summary>
    /// <param name="key">Key for the codec phonebook sync state</param>
    public CodecPhonebookSyncState(string key)
    {
      Key = key;

      CodecDisconnected();
    }

    /// <summary>
    /// Gets or sets the InitialSyncComplete
    /// </summary>
    public bool InitialSyncComplete
    {
      get { return _InitialSyncComplete; }
      private set
      {
        if (value == true)
        {
          InitialSyncCompleted?.Invoke(this, new EventArgs());
        }
        _InitialSyncComplete = value;
      }
    }

    /// <summary>
    /// Gets or sets the InitialPhonebookFoldersWasReceived
    /// </summary>
    public bool InitialPhonebookFoldersWasReceived { get; private set; }

    /// <summary>
    /// Gets or sets the NumberOfContactsWasReceived
    /// </summary>
    public bool NumberOfContactsWasReceived { get; private set; }

    /// <summary>
    /// Gets or sets the PhonebookRootEntriesWasRecieved
    /// </summary>
    public bool PhonebookRootEntriesWasRecieved { get; private set; }

    /// <summary>
    /// Gets or sets the PhonebookHasFolders
    /// </summary>
    public bool PhonebookHasFolders { get; private set; }

    /// <summary>
    /// Gets or sets the NumberOfContacts
    /// </summary>
    public int NumberOfContacts { get; private set; }

    #region IKeyed Members

    /// <summary>
    /// Gets or sets the Key
    /// </summary>
    public string Key { get; private set; }

    #endregion

    /// <summary>
    /// Event InitialSyncCompleted
    /// </summary>
    public event EventHandler<EventArgs> InitialSyncCompleted;

    /// <summary>
    /// InitialPhonebookFoldersReceived method
    /// </summary>
    public void InitialPhonebookFoldersReceived()
    {
      InitialPhonebookFoldersWasReceived = true;

      CheckSyncStatus();
    }

    /// <summary>
    /// PhonebookRootEntriesReceived method
    /// </summary>
    public void PhonebookRootEntriesReceived()
    {
      PhonebookRootEntriesWasRecieved = true;

      CheckSyncStatus();
    }

    /// <summary>
    /// SetPhonebookHasFolders method
    /// </summary>
    public void SetPhonebookHasFolders(bool value)
    {
      PhonebookHasFolders = value;

      Debug.LogMessage(LogEventLevel.Debug, this, "Phonebook has folders: {0}", PhonebookHasFolders);
    }

    /// <summary>
    /// SetNumberOfContacts method
    /// </summary>
    public void SetNumberOfContacts(int contacts)
    {
      NumberOfContacts = contacts;
      NumberOfContactsWasReceived = true;

      Debug.LogMessage(LogEventLevel.Debug, this, "Phonebook contains {0} contacts.", NumberOfContacts);

      CheckSyncStatus();
    }

    /// <summary>
    /// CodecDisconnected method
    /// </summary>
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
        Debug.LogMessage(LogEventLevel.Debug, this, "Initial Phonebook Sync Complete!");
      }
      else
      {
        InitialSyncComplete = false;
      }
    }
  }



}