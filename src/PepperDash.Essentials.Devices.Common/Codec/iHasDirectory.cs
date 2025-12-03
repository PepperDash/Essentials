

using System;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines the API for codecs with a directory
    /// </summary>
    public interface IHasDirectory
    {
        /// <summary>
        /// Event that fires when a directory result is returned from the codec
        /// </summary>
        event EventHandler<DirectoryEventArgs> DirectoryResultReturned;

        /// <summary>
        /// Gets the DirectoryRoot
        /// </summary>
        CodecDirectory DirectoryRoot { get; }

        /// <summary>
        /// Gets the CurrentDirectoryResult
        /// </summary>
        CodecDirectory CurrentDirectoryResult { get; }

        /// <summary>
        /// Gets the PhonebookSyncState
        /// </summary>
        CodecPhonebookSyncState PhonebookSyncState { get; }

        /// <summary>
        /// Method to initiate a search of the directory on the server
        /// </summary>
        void SearchDirectory(string searchString);

        /// <summary>
        /// Method to get the contents of a specific folder in the directory on the server
        /// </summary>
        void GetDirectoryFolderContents(string folderId);

        /// <summary>
        /// Method to set the current directory to the root folder
        /// </summary>
        void SetCurrentDirectoryToRoot();

        /// <summary>
        /// Method to get the contents of the parent folder in the directory on the server
        /// </summary>
        void GetDirectoryParentFolderContents();

        /// <summary>
        /// Gets the CurrentDirectoryResultIsNotDirectoryRoot
        /// </summary>
        BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; }
    }
}