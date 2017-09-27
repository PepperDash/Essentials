using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface iHasDirectory
    {
        event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        CodecDirectory DirectoryRoot { get; }

        void SearchDirectory(string searchString);

        void GetDirectoryFolderContents(string folderId);
    }

    public class DirectoryEventArgs : EventArgs
    {
        public CodecDirectory Directory { get; set; }
    }

    public class CodecDirectory
    {
        public List<DirectoryItem> DirectoryResults { get; private set; }

        //public int Offset { get; private set; }

        //public int Limit { get; private set; }

        public CodecDirectory()
        {
            DirectoryResults = new List<DirectoryItem>();
        }

        public void AddFoldersToDirectory(List<DirectoryItem> folders)
        {
            DirectoryResults.AddRange(folders);

            SortDirectory();
        }

        public void AddContactsToDirectory(List<DirectoryItem> contacts)
        {
            DirectoryResults.AddRange(contacts);

            SortDirectory();
        }

        /// <summary>
        /// Formats the DirectoryResults list to display all folders alphabetically, then all contacts alphabetically
        /// </summary>
        private void SortDirectory()
        {
            var sortedFolders = new List<DirectoryItem>();

            sortedFolders.AddRange(DirectoryResults.Where(f => f is DirectoryFolder));

            sortedFolders.OrderBy(f => f.Name);

            var sortedContacts = new List<DirectoryItem>();

            sortedContacts.AddRange(DirectoryResults.Where(c => c is DirectoryContact));

            sortedFolders.OrderBy(c => c.Name);

            DirectoryResults.Clear();

            DirectoryResults.AddRange(sortedFolders);

            DirectoryResults.AddRange(sortedContacts);
        }
    }

    public class DirectoryItem
    {
        public string Name { get; set; }
    }

    public class DirectoryFolder : DirectoryItem
    {
        public List<DirectoryContact> Contacts { get; set; }
        public string FolderId { get; set; }
        public string ParentFolderId { get; set; }

        public DirectoryFolder()
        {
            Contacts = new List<DirectoryContact>();
        }
    }

    public class DirectoryContact : DirectoryItem
    {
        public string ContactId { get; set; }
        public string FolderId { get; set; }   
        public string Title { get; set; }
        public List<ContactMethod> ContactMethods { get; set; }

        public DirectoryContact()
        {
            ContactMethods = new List<ContactMethod>();
        }
    }

    public class ContactMethod
    {
        public string ContactMethodId { get; set; }
        public string Number { get; set; }
        public eContactMethodDevice Device { get; set; }
        public eContactMethodCallType CallType { get; set; }
    }

    public enum eContactMethodDevice
    {
        Unknown = 0,
        Mobile,
        Other,
        Telephone,
        Video
    }

    public enum eContactMethodCallType
    {
        Unknown = 0,
        Audio,
        Video
    }

    public class DirectorySearchResultEventArgs : EventArgs
    {
        public CodecDirectory Directory { get; set; }
    }
}