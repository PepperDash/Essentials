

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Codec
{
	/// <summary>
	/// Defines the API for codecs with a directory
	/// </summary>
    public interface IHasDirectory
    {
        event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        CodecDirectory DirectoryRoot { get; }

        CodecDirectory CurrentDirectoryResult { get; }

        CodecPhonebookSyncState PhonebookSyncState { get; }

        void SearchDirectory(string searchString);

        void GetDirectoryFolderContents(string folderId);

        void SetCurrentDirectoryToRoot();

        void GetDirectoryParentFolderContents();

        BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; }
    }

    /// <summary>
    /// Defines the contract for IHasDirectoryHistoryStack
    /// </summary>
    public interface IHasDirectoryHistoryStack : IHasDirectory
    {
        Stack<CodecDirectory> DirectoryBrowseHistoryStack { get; } 
    }


    /// <summary>
    /// Represents a DirectoryEventArgs
    /// </summary>
    public class DirectoryEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the Directory
        /// </summary>
        public CodecDirectory Directory { get; set; }
        /// <summary>
        /// Gets or sets the DirectoryIsOnRoot
        /// </summary>
        public bool DirectoryIsOnRoot { get; set; }
    }

	/// <summary>
	/// Represents a codec directory
	/// </summary>
    public class CodecDirectory
    {
        /// <summary>
        /// Represents the contents of the directory
        /// We don't want to serialize this for messages to MobileControl.  MC can combine Contacts and Folders to get the same data
        /// </summary>
		[JsonIgnore]
        public List<DirectoryItem> CurrentDirectoryResults { get; private set; }

        [JsonProperty("contacts")]
        public List<DirectoryItem> Contacts
        {
            get
            {
                return CurrentDirectoryResults.OfType<DirectoryContact>().Cast<DirectoryItem>().ToList();
            }
        }

        [JsonProperty("folders")]
        public List<DirectoryItem> Folders
        {
            get
            {
                return CurrentDirectoryResults.OfType<DirectoryFolder>().Cast<DirectoryItem>().ToList();
            }
        }

        /// <summary>
        /// Used to store the ID of the current folder for CurrentDirectoryResults
        /// </summary>
		[JsonProperty("resultsFolderId")]
        /// <summary>
        /// Gets or sets the ResultsFolderId
        /// </summary>
        public string ResultsFolderId { get; set; }

        public CodecDirectory()
        {
            CurrentDirectoryResults = new List<DirectoryItem>();
        }

        /// <summary>
        /// Adds folders to the directory
        /// </summary>
        /// <param name="folders"></param>
        /// <summary>
        /// AddFoldersToDirectory method
        /// </summary>
        public void AddFoldersToDirectory(List<DirectoryItem> folders)
        {
            if(folders != null)
                CurrentDirectoryResults.AddRange(folders);

            SortDirectory();
        }

        /// <summary>
        /// Adds contacts to the directory
        /// </summary>
        /// <param name="contacts"></param>
        /// <summary>
        /// AddContactsToDirectory method
        /// </summary>
        public void AddContactsToDirectory(List<DirectoryItem> contacts)
        {
            if(contacts != null)
                CurrentDirectoryResults.AddRange(contacts);

            SortDirectory();
        }

        /// <summary>
        /// Filters the CurrentDirectoryResults by the predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <summary>
        /// FilterContacts method
        /// </summary>
        public void FilterContacts(Func<DirectoryItem, bool> predicate)
        {
            CurrentDirectoryResults = CurrentDirectoryResults.Where(predicate).ToList();
        }

        /// <summary>
        /// Sorts the DirectoryResults list to display all folders alphabetically, then all contacts alphabetically
        /// </summary>
        private void SortDirectory()
        {
            var sortedFolders = new List<DirectoryItem>();

            sortedFolders.AddRange(CurrentDirectoryResults.Where(f => f is DirectoryFolder));

            sortedFolders.OrderBy(f => f.Name);

            var sortedContacts = new List<DirectoryItem>();

            sortedContacts.AddRange(CurrentDirectoryResults.Where(c => c is DirectoryContact));

            sortedFolders.OrderBy(c => c.Name);

            CurrentDirectoryResults.Clear();

            CurrentDirectoryResults.AddRange(sortedFolders);

            CurrentDirectoryResults.AddRange(sortedContacts);
        }

    }

    /// <summary>
    /// Defines the contract for IInvitableContact
    /// </summary>
    public interface IInvitableContact
    {
        bool IsInvitableContact { get; }
    }

    public class InvitableDirectoryContact : DirectoryContact, IInvitableContact
    {
        [JsonProperty("isInvitableContact")]
        public bool IsInvitableContact
        {
            get
            {
                return this is IInvitableContact;
            }
        }
    }

    /// <summary>
    /// Represents a DirectoryItem
    /// </summary>
    public class DirectoryItem : ICloneable
    {
        /// <summary>
        /// Clone method
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        [JsonProperty("folderId")]
        public string FolderId { get; set; }

		[JsonProperty("name")]	
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        [JsonProperty("parentFolderId")]
        /// <summary>
        /// Gets or sets the ParentFolderId
        /// </summary>
        public string ParentFolderId { get; set; }
    }

    /// <summary>
    /// Represents a DirectoryFolder
    /// </summary>
    public class DirectoryFolder : DirectoryItem
    {
		[JsonProperty("contacts")]
        /// <summary>
        /// Gets or sets the Contacts
        /// </summary>
        public List<DirectoryContact> Contacts { get; set; }


        public DirectoryFolder()
        {
            Contacts = new List<DirectoryContact>();
        }
    }

    /// <summary>
    /// Represents a DirectoryContact
    /// </summary>
    public class DirectoryContact : DirectoryItem
    {
		[JsonProperty("contactId")]
        /// <summary>
        /// Gets or sets the ContactId
        /// </summary>
        public string ContactId { get; set; } 

		[JsonProperty("title")]
        public string Title { get; set; }

		[JsonProperty("contactMethods")]
        public List<ContactMethod> ContactMethods { get; set; }

        public DirectoryContact()
        {
            ContactMethods = new List<ContactMethod>();
        }
    }

    /// <summary>
    /// Represents a ContactMethod
    /// </summary>
    public class ContactMethod
    {
		[JsonProperty("contactMethodId")]
        /// <summary>
        /// Gets or sets the ContactMethodId
        /// </summary>
        public string ContactMethodId { get; set; }

		[JsonProperty("number")]
        public string Number { get; set; }
        
		[JsonProperty("device")]
		[JsonConverter(typeof(StringEnumConverter))]
  /// <summary>
  /// Gets or sets the Device
  /// </summary>
		public eContactMethodDevice Device { get; set; }

		[JsonProperty("callType")]
		[JsonConverter(typeof(StringEnumConverter))]
  /// <summary>
  /// Gets or sets the CallType
  /// </summary>
		public eContactMethodCallType CallType { get; set; }
    }

    /// <summary>
    /// Enumeration of eContactMethodDevice values
    /// </summary>
    public enum eContactMethodDevice
    {
        Unknown = 0,
        Mobile,
        Other,
        Telephone,
        Video
    }
	
    /// <summary>
    /// Enumeration of eContactMethodCallType values
    /// </summary>
    public enum eContactMethodCallType
    {
        Unknown = 0,
        Audio,
        Video
    }
}