using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Messenger for devices implementing <see cref="IHasDirectory"/>
    /// </summary>
    public class IHasDirectoryMessenger : MessengerBase
    {
        private readonly IHasDirectory _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IHasDirectoryMessenger"/> class.
        /// </summary>
        public IHasDirectoryMessenger(string key, string messagePath, EssentialsDevice device)
            : base(key, messagePath, device)
        {
            _directory = device as IHasDirectory ?? throw new ArgumentException("device must implement IHasDirectory", nameof(device));
        }

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) => SendFullStatus());

            AddAction("/directoryStatus", (id, content) => SendFullStatus());

            AddAction("/getDirectory", (id, content) => GetDirectoryRoot());

            AddAction("/directoryById", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                GetDirectory(msg.Value);
            });

            AddAction("/directorySearch", (id, content) =>
            {
                var msg = content.ToObject<MobileControlSimpleContent<string>>();
                GetDirectory(msg.Value);
            });

            AddAction("/directoryBack", (id, content) => GetPreviousDirectory());

            _directory.DirectoryResultReturned += DirectoryResultReturned;
            _directory.PhonebookSyncState.InitialSyncCompleted += PhonebookSyncState_InitialSyncCompleted;
        }

        private void DirectoryResultReturned(object sender, DirectoryEventArgs e)
        {
            SendDirectory(e.Directory);
        }

        private void SendDirectory(CodecDirectory directory)
        {
            try
            {
                this.LogVerbose("Sending Directory. Directory Item Count: {directoryItemCount}", directory.CurrentDirectoryResults.Count);
                Task.Run(() => PostStatusMessage(new IHasDirectoryStateMessage
                {
                    CurrentDirectory = directory
                }));
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error sending directory");
            }
        }

        private void PhonebookSyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            try
            {
                PostStatusMessage(new IHasDirectoryStateMessage
                {
                    InitialPhonebookSyncComplete = true
                });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error posting phonebook sync state");
            }
        }

        private void GetDirectory(string id)
        {
            _directory.GetDirectoryFolderContents(id);
        }

        private void GetDirectoryRoot()
        {
            try
            {
                if (!_directory.PhonebookSyncState.InitialSyncComplete)
                {
                    PostStatusMessage(new IHasDirectoryStateMessage
                    {
                        InitialPhonebookSyncComplete = false
                    });
                    return;
                }

                _directory.SetCurrentDirectoryToRoot();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error getting directory root");
            }
        }

        private void GetPreviousDirectory()
        {
            _directory.GetDirectoryParentFolderContents();
        }

        private void SendFullStatus()
        {
            PostStatusMessage(new IHasDirectoryStateMessage
            {
                CurrentDirectory = _directory.CurrentDirectoryResult,
                InitialPhonebookSyncComplete = _directory.PhonebookSyncState.InitialSyncComplete,
                HasDirectory = true,
                HasDirectorySearch = true,
            });
        }
    }

    public class IHasDirectoryStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("currentDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public CodecDirectory CurrentDirectory { get; set; }

        [JsonProperty("initialPhonebookSyncComplete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InitialPhonebookSyncComplete { get; set; }

        [JsonProperty("hasDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDirectory { get; set; }

        [JsonProperty("hasDirectorySearch", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDirectorySearch { get; set; }

        /// <summary>
        /// Gets or sets the DirectorySelectedFolderName
        /// </summary>
        [JsonProperty("directorySelectedFolderName", NullValueHandling = NullValueHandling.Ignore)]
        public string DirectorySelectedFolderName { get; set; }

    }
}
