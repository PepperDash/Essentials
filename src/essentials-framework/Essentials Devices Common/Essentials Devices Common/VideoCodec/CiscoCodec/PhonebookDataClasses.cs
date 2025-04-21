﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Devices.Common.Codec;

namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public class CiscoCodecPhonebook
    {
        public class Offset
        {
            public string Value { get; set; }
        }

        public class Limit
        {
            public string Value { get; set; }
        }

        public class TotalRows
        {
            public string Value { get; set; }
        }

        public class ResultInfo
        {
            public Offset Offset { get; set; }
            public Limit Limit { get; set; }
            public TotalRows TotalRows { get; set; }
        }

        public class LocalId
        {
            public string Value { get; set; }
        }

        public class FolderId
        {
            public string Value { get; set; }
        }

        public class ParentFolderId
        {
            public string Value { get; set; }
        }

        public class Name
        {
            public string Value { get; set; }
        }

        public class Folder
        {
            public string id { get; set; }
            public LocalId LocalId { get; set; }
            public FolderId FolderId { get; set; }
            public Name Name { get; set; }
            public ParentFolderId ParentFolderId { get; set; }
        }

        public class Name2
        {
            public string Value { get; set; }
        }

        public class ContactId
        {
            public string Value { get; set; }
        }

        public class FolderId2
        {
            public string Value { get; set; }
        }

        public class Title
        {
            public string Value { get; set; }
        }

        public class ContactMethodId
        {
            public string Value { get; set; }
        }

        public class Number
        {
            public string Value { get; set; }
        }

        public class Device
        {
            public string Value { get; set; }
        }

        public class CallType
        {
            public string Value { get; set; }
        }

        public class ContactMethod
        {
            public string id { get; set; }
            public ContactMethodId ContactMethodId { get; set; }
            public Number Number { get; set; }
            public Device Device { get; set; }
            public CallType CallType { get; set; }

            public ContactMethod()
            {
                ContactMethodId = new ContactMethodId();
                Number = new Number();
                Device = new Device();
                CallType = new CallType();
            }
        }

        public class Contact
        {
            public string id { get; set; }
            public Name2 Name { get; set; }
            public ContactId ContactId { get; set; }
            public FolderId2 FolderId { get; set; }
            public Title Title { get; set; }
            public List<ContactMethod> ContactMethod { get; set; }

            public Contact()
            {
                Name = new Name2();
                ContactId = new ContactId();
                FolderId = new FolderId2();
                Title = new Title();
                ContactMethod = new List<ContactMethod>();
            }
        }

        public class PhonebookSearchResult
        {
            public string status { get; set; }
            public ResultInfo ResultInfo { get; set; }
            public List<Folder> Folder { get; set; }
            public List<Contact> Contact { get; set; }

            public PhonebookSearchResult()
            {
                Folder = new List<Folder>();
                Contact = new List<Contact>();
                ResultInfo = new ResultInfo();
            }
        }

        public class CommandResponse
        {
            public PhonebookSearchResult PhonebookSearchResult { get; set; }
        }

        public class RootObject
        {
            public CommandResponse CommandResponse { get; set; }

        }


        /// <summary>
        /// Extracts the folders with no ParentFolder and returns them sorted alphabetically
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static List<DirectoryItem> GetRootFoldersFromSearchResult(PhonebookSearchResult result)
        {
            var rootFolders = new List<DirectoryItem>();

            if (result.Folder.Count == 0)
            {
                return null;
            }
            else if (result.Folder.Count > 0)
            {
                if (Debug.Level > 0)
                    Debug.Console(1, "Phonebook Folders:\n");

                foreach (Folder f in result.Folder)
                {
                    var folder = new DirectoryFolder();

                    folder.Name = f.Name.Value;
                    folder.FolderId = f.FolderId.Value;

                    if (f.ParentFolderId == null)
                        rootFolders.Add(folder);
                   
                    if (Debug.Level > 0)
                        Debug.Console(1, "+ {0}", folder.Name);
                }
            }

            rootFolders.OrderBy(f => f.Name);

            return rootFolders;
        }


        /// <summary>
        /// Extracts the contacts with no FolderId and returns them sorted alphabetically
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static List<DirectoryItem> GetRootContactsFromSearchResult(PhonebookSearchResult result)
        {
            var rootContacts = new List<DirectoryItem>();

            if (result.Contact.Count == 0)
            {
                return null;
            }
            else if (result.Contact.Count > 0)
            {
                if (Debug.Level > 0)
                    Debug.Console(1, "Root Contacts:\n");

                foreach (Contact c in result.Contact)
                {
                    var contact = new DirectoryContact();

                    if (string.IsNullOrEmpty(c.FolderId.Value))
                    {
                        contact.Name = c.Name.Value;
                        contact.ContactId = c.ContactId.Value;

                        if(!string.IsNullOrEmpty(c.Title.Value))
                            contact.Title = c.Title.Value;

                        if (Debug.Level > 0)
                            Debug.Console(1, "{0}\nContact Methods:", contact.Name);

                        foreach (ContactMethod m in c.ContactMethod)
                        {

                            var tempContactMethod = new PepperDash.Essentials.Devices.Common.Codec.ContactMethod();

                            eContactMethodCallType callType = eContactMethodCallType.Unknown;
                            if (!string.IsNullOrEmpty(m.CallType.Value))
                            {
                                if (!string.IsNullOrEmpty(m.CallType.Value))
                                {
                                    if (m.CallType.Value.ToLower() == "audio")
                                        callType = eContactMethodCallType.Audio;
                                    else if (m.CallType.Value.ToLower() == "video")
                                        callType = eContactMethodCallType.Video;

                                    tempContactMethod.CallType = callType;
                                }
                            }

                            eContactMethodDevice device = eContactMethodDevice.Unknown;
                            if (!string.IsNullOrEmpty(m.Device.Value))
                            {

                                if (m.Device.Value.ToLower() == "mobile")
                                    device = eContactMethodDevice.Mobile;
                                else if (m.Device.Value.ToLower() == "telephone")
                                    device = eContactMethodDevice.Telephone;
                                else if (m.Device.Value.ToLower() == "video")
                                    device = eContactMethodDevice.Video;
                                else if (m.Device.Value.ToLower() == "other")
                                    device = eContactMethodDevice.Other;

                                tempContactMethod.Device = device;
                            }

                            if (Debug.Level > 0)
                                Debug.Console(1, "Number: {0}", m.Number.Value);

                            tempContactMethod.Number = m.Number.Value;
                            tempContactMethod.ContactMethodId = m.ContactMethodId.Value;

                            contact.ContactMethods.Add(tempContactMethod);
                        }
                        rootContacts.Add(contact);
                    }
                }
            }

            rootContacts.OrderBy(f => f.Name);

            return rootContacts;
        }


        /// <summary>
        /// Converts data returned from a cisco codec to the generic Directory format.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="resultFolder"></param>
        /// <returns></returns>
        public static CodecDirectory ConvertCiscoPhonebookToGeneric(PhonebookSearchResult result)
        {
            var directory = new Codec.CodecDirectory();

            var folders = new List<Codec.DirectoryItem>();

            var contacts = new List<Codec.DirectoryItem>();

            try
            {
                if (result.Folder.Count > 0)
                {
                    foreach (Folder f in result.Folder)
                    {
                        var folder = new DirectoryFolder();

                        folder.Name = f.Name.Value;
                        folder.FolderId = f.FolderId.Value;

                        if (f.ParentFolderId != null)
                        {
                            folder.ParentFolderId = f.ParentFolderId.Value;
                        }

                        folders.Add(folder);
                    }

                    folders.OrderBy(f => f.Name);

                    directory.AddFoldersToDirectory(folders);
                }

                if (result.Contact.Count > 0)
                {
                    foreach (Contact c in result.Contact)
                    {
                        var contact = new DirectoryContact();

                        contact.Name = c.Name.Value;
                        contact.ContactId = c.ContactId.Value;
                        if (!string.IsNullOrEmpty(c.Title.Value))
                            contact.Title = c.Title.Value;

                        if (c.FolderId != null)
                        {
                            contact.FolderId = c.FolderId.Value;
                        }

                        foreach (ContactMethod m in c.ContactMethod)
                        {
                            eContactMethodCallType callType = eContactMethodCallType.Unknown;
                            if (!string.IsNullOrEmpty(m.CallType.Value))
                            {
                                if (m.CallType.Value.ToLower() == "audio")
                                    callType = eContactMethodCallType.Audio;
                                else if (m.CallType.Value.ToLower() == "video")
                                    callType = eContactMethodCallType.Video;
                            }

                            eContactMethodDevice device = eContactMethodDevice.Unknown;

                            if (!string.IsNullOrEmpty(m.Device.Value))
                            {
                                if (m.Device.Value.ToLower() == "mobile")
                                    device = eContactMethodDevice.Mobile;
                                else if (m.Device.Value.ToLower() == "telephone")
                                    device = eContactMethodDevice.Telephone;
                                else if (m.Device.Value.ToLower() == "video")
                                    device = eContactMethodDevice.Video;
                                else if (m.Device.Value.ToLower() == "other")
                                    device = eContactMethodDevice.Other;
                            }

                            contact.ContactMethods.Add(new PepperDash.Essentials.Devices.Common.Codec.ContactMethod()
                            {
                                Number = m.Number.Value,
                                ContactMethodId = m.ContactMethodId.Value,
                                CallType = callType,
                                Device = device
                            });
                        }
                        contacts.Add(contact);
                    }

                    contacts.OrderBy(c => c.Name);

                    directory.AddContactsToDirectory(contacts);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, "Error converting Cisco Phonebook results to generic: {0}", e);
            }

            return directory;
        }
    }
}