using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;


namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    public static class MockVideoCodecDirectory
    {
        public enum eFolderId
        {
            UnitedStates,
            Canada,
            NewYork,
            Boston,
            SanFrancisco,
            Denver,
            Austin,
            Calgary
        }


        /// <summary>
        /// Aggregates the directory items for all directories into a single directory for searching purposes
        /// </summary>
        public static CodecDirectory CompleteDirectory
        {
            get
            {
                var completeDirectory = new CodecDirectory();

                completeDirectory.AddContactsToDirectory(DirectoryRoot.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(UnitedStatesFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(CanadaFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(NewYorkFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(BostonFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(DenverFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(AustinFolderContents.CurrentDirectoryResults);
                completeDirectory.AddContactsToDirectory(CalgaryFolderContents.CurrentDirectoryResults);

                return completeDirectory;
            }
        }

        public static CodecDirectory DirectoryRoot 
        {
            get
            {
                var directory = new CodecDirectory();
                
                directory.AddFoldersToDirectory
                ( 
                    new List<DirectoryItem>()
                    {                      
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.UnitedStates.ToString(),
                            Name = "United States",
                            ParentFolderId = "",
                            Contacts = null
                        },
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.Canada.ToString(),
                            Name = "Canada",
                            ParentFolderId = "",
                            Contacts = null
                        }
                    }
                );
                        
                directory.AddContactsToDirectory
                (
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            Name = "Corporate Bridge",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "c_1",
                                    Number = "site.corp.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }
                    }
                );              
                
                return directory;
            }     
        }

        public static CodecDirectory UnitedStatesFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.UnitedStates.ToString();
                directory.AddFoldersToDirectory 
                (                
                    new List<DirectoryItem>()
                    { 
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.NewYork.ToString(),
                            Name = "New York",
                            ParentFolderId = eFolderId.UnitedStates.ToString(),
                            Contacts = null
                        },
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.Boston.ToString(),
                            Name = "Boston",
                            ParentFolderId = eFolderId.UnitedStates.ToString(),
                            Contacts = null
                        },
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.SanFrancisco.ToString(),
                            Name = "San Francisco",
                            ParentFolderId = eFolderId.UnitedStates.ToString(),
                            Contacts = null
                        },
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.Denver.ToString(),
                            Name = "Denver",
                            ParentFolderId = eFolderId.UnitedStates.ToString(),
                            Contacts = null
                        },
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.Austin.ToString(),
                            Name = "Austin",
                            ParentFolderId = eFolderId.UnitedStates.ToString(),
                            Contacts = null
                        }
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory NewYorkFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.NewYork.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "nyc_1",
                            Name = "Meeting Room",
                            Title = @"",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "nycmeetingroom.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        },
                        new DirectoryContact()
                        {
                            ContactId = "nyc_2",
                            Name = "Sumanth Rayancha",
                            Title = @"CTO",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "srayancha.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        },
                        new DirectoryContact()
                        {
                            ContactId = "nyc_3",
                            Name = "Justin Gordon",
                            Title = @"Software Developer",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "jgordon.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory BostonFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.Boston.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "bos_1",
                            Name = "Board Room",
                            Title = @"",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "bosboardroom.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory SanFranciscoFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.SanFrancisco.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "sfo_1",
                            Name = "David Huselid",
                            Title = @"Cive President, COO",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "dhuselid.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }   
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory DenverFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.Denver.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "den_1",
                            Name = "Heath Volmer",
                            Title = @"Software Developer",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "hvolmer.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }   
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory AustinFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.Austin.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "atx_1",
                            Name = "Vincent Longano",
                            Title = @"Product Development Manager",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "vlongano.pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }   
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory CanadaFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.Canada.ToString();
                directory.AddFoldersToDirectory
                (                    
                    new List<DirectoryItem>()
                    { 
                        new DirectoryFolder()
                        {
                            FolderId = eFolderId.Calgary.ToString(),
                            Name = "Calgary",
                            ParentFolderId = eFolderId.Canada.ToString(),
                            Contacts = null
                        }
                    }
                );

                return directory;
            }
        }

        public static CodecDirectory CalgaryFolderContents
        {
            get
            {
                var directory = new CodecDirectory();

                directory.ResultsFolderId = eFolderId.Calgary.ToString();
                directory.AddContactsToDirectory
                (                    
                    new List<DirectoryItem>()
                    {
                        new DirectoryContact()
                        {
                            ContactId = "cdn_1",
                            Name = "Neil Dorin",
                            Title = @"Software Developer /SC",
                            ContactMethods = new List<ContactMethod>()
                            {
                                new ContactMethod()
                                {
                                    ContactMethodId = "cid_1",
                                    Number = "ndorin@pepperdash.com",
                                    Device = eContactMethodDevice.Video,
                                    CallType = eContactMethodCallType.Video
                                }
                            }
                        }  
                    }
                );

                return directory;
            }
        }


    }
}