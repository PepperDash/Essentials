extern alias Full;

using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
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
}