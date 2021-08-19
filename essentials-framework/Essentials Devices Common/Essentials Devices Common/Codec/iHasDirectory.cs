using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


using PepperDash.Core;
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

        /// <summary>
        /// Tracks the directory browse history when browsing beyond the root directory
        /// </summary>
        [Obsolete("Please use the Stack-based history instead")]
        List<CodecDirectory> DirectoryBrowseHistory { get; }
    }

    public interface IHasDirectoryHistoryStack : IHasDirectory
    {
        Stack<CodecDirectory> DirectoryBrowseHistoryStack { get; } 
    }

	/// <summary>
	/// 
	/// </summary>
    public class DirectoryEventArgs : EventArgs
    {
        public CodecDirectory Directory { get; set; }
        public bool DirectoryIsOnRoot { get; set; }
    }

	/// <summary>
	/// Represents a codec directory
	/// </summary>
    public class CodecDirectory
    {
        /// <summary>
        /// Represents the contents of the directory
        /// </summary>
		[JsonProperty("directoryResults")]
        public List<DirectoryItem> CurrentDirectoryResults { get; private set; }

        public List<DirectoryItem> Contacts
        {
            get
            {
                return CurrentDirectoryResults.OfType<DirectoryContact>().Cast<DirectoryItem>().ToList();
            }
        }

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
        public string ResultsFolderId { get; set; }

        public CodecDirectory()
        {
            CurrentDirectoryResults = new List<DirectoryItem>();
        }

        /// <summary>
        /// Adds folders to the directory
        /// </summary>
        /// <param name="folders"></param>
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
    /// Used to decorate a contact to indicate it can be invided to a meeting
    /// </summary>
    public interface IInvitableContact
    {

    }

	/// <summary>
	/// Represents an item in the directory
	/// </summary>
    public class DirectoryItem : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        [JsonProperty("folderId")]
        public string FolderId { get; set; }

		[JsonProperty("name")]	
        public string Name { get; set; }

        [JsonProperty("parentFolderId")]
        public string ParentFolderId { get; set; }
    }

	/// <summary>
	/// Represents a folder type DirectoryItem
	/// </summary>
    public class DirectoryFolder : DirectoryItem
    {
		[JsonProperty("contacts")]
        public List<DirectoryContact> Contacts { get; set; }


        public DirectoryFolder()
        {
            Contacts = new List<DirectoryContact>();
        }
    }

	/// <summary>
	/// Represents a contact type DirectoryItem
	/// </summary>
    public class DirectoryContact : DirectoryItem
    {
		[JsonProperty("contactId")]
        public string ContactId { get; set; } 

		[JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("isInvitableContact")]
        public bool IsInvitableContact
        {
            get 
            {
                return this is IInvitableContact;
            }
        }

		[JsonProperty("contactMethods")]
        public List<ContactMethod> ContactMethods { get; set; }

        public DirectoryContact()
        {
            ContactMethods = new List<ContactMethod>();
        }
    }

	/// <summary>
	/// Represents a method of contact for a contact
	/// </summary>
    public class ContactMethod
    {
		[JsonProperty("contactMethodId")]
        public string ContactMethodId { get; set; }

		[JsonProperty("number")]
        public string Number { get; set; }
        
		[JsonProperty("device")]
		[JsonConverter(typeof(StringEnumConverter))]
		public eContactMethodDevice Device { get; set; }

		[JsonProperty("callType")]
		[JsonConverter(typeof(StringEnumConverter))]
		public eContactMethodCallType CallType { get; set; }
    }

	/// <summary>
	/// 
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
	/// 
	/// </summary>
    public enum eContactMethodCallType
    {
        Unknown = 0,
        Audio,
        Video
    }
}