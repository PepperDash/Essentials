using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface iHasDirectory
    {
        CodecDirectory Directory { get; }

        /// <summary>
        /// Searches the directory and returns a result 
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="key"></param>
        void SearchDirectory(string searchString, string key);
    }

    public class CodecDirectory
    {
        public List<DirectoryFolder> Folders {get; private set;}

        public List<DirectoryContact> Contacts { get; private set; }

        public int Offset { get; private set; }

        public int Limit { get; private set; }

        public CodecDirectory()
        {
            Folders = new List<DirectoryFolder>();
            Contacts = new List<DirectoryContact>();
        }
    }

    public class DirectoryFolder
    {
        public List<DirectoryContact> Contacts { get; set; }
        public string FolderId { get; set; }
        public string Name { get; set; }
        public DirectoryFolder ParentFolder { get; set; }

        public DirectoryFolder()
        {
            Contacts = new List<DirectoryContact>();
            ParentFolder = new DirectoryFolder();
        }
    }

    public class DirectoryContact
    {
        public string ContactId { get; set; }
        public DirectoryFolder Folder { get; set; }   
        public string Name { get; set; }
        public string Title { get; set; }
        public List<ContactMethod> ContactMethods { get; set; }
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