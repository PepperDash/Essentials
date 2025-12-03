using System;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;


namespace PepperDash.Essentials.RoomBridges
{
    /// <summary>
    /// Base class for a Mobile Control Bridge that's used to control a room
    /// </summary>
    public abstract class MobileControlBridgeBase : MessengerBase, IMobileControlRoomMessenger
    {
        /// <summary>
        /// Triggered when the user Code changes
        /// </summary>
        public event EventHandler<EventArgs> UserCodeChanged;

        /// <summary>
        /// Triggered when a user should be prompted for the new code
        /// </summary>
        public event EventHandler<EventArgs> UserPromptedForCode;

        /// <summary>
        /// Triggered when a client joins to control this room
        /// </summary>
        public event EventHandler<EventArgs> ClientJoined;

        /// <summary>
        /// Triggered when the App URL for this room changes
        /// </summary>
        public event EventHandler<EventArgs> AppUrlChanged;

        /// <summary>
        /// Gets or sets the Parent
        /// </summary>
        public IMobileControl Parent { get; private set; }

        /// <summary>
        /// Gets or sets the AppUrl
        /// </summary>
        public string AppUrl { get; private set; }
        /// <summary>
        /// Gets or sets the UserCode
        /// </summary>
        public string UserCode { get; private set; }

        /// <summary>
        /// Gets or sets the QrCodeUrl
        /// </summary>
        public string QrCodeUrl { get; protected set; }

        /// <summary>
        /// Gets or sets the QrCodeChecksum
        /// </summary>
        public string QrCodeChecksum { get; protected set; }

        /// <summary>
        /// Gets or sets the McServerUrl
        /// </summary>
        public string McServerUrl { get; private set; }

        /// <summary>
        /// Room Name
        /// </summary>
        public abstract string RoomName { get; }

        /// <summary>
        /// Room key
        /// </summary>
        public abstract string RoomKey { get; }

        /// <summary>
        /// Create an instance of the <see cref="MobileControlBridgeBase"/> class
        /// </summary>
        /// <param name="key">The unique key for this bridge</param>
        /// <param name="messagePath">The message path for this bridge</param>
        protected MobileControlBridgeBase(string key, string messagePath)
            : base(key, messagePath)
        {
        }

        /// <summary>
        /// Create an instance of the <see cref="MobileControlBridgeBase"/> class
        /// </summary>
        /// <param name="key">The unique key for this bridge</param>
        /// <param name="messagePath">The message path for this bridge</param>
        /// <param name="device">The device associated with this bridge</param>
        protected MobileControlBridgeBase(string key, string messagePath, IKeyName device)
            : base(key, messagePath, device)
        {
        }

        /// <summary>
        /// Set the parent.  Does nothing else.  Override to add functionality such
        /// as adding actions to parent
        /// </summary>
        /// <param name="parent"></param>
        /// <summary>
        /// AddParent method
        /// </summary>
        public virtual void AddParent(IMobileControl parent)
        {
            Parent = parent;

            McServerUrl = Parent.ClientAppUrl;
        }

        /// <summary>
        /// Sets the UserCode on the bridge object. Called from controller. A changed code will
        /// fire method UserCodeChange.  Override that to handle changes
        /// </summary>
        /// <param name="code"></param>
        /// <summary>
        /// SetUserCode method
        /// </summary>
        public void SetUserCode(string code)
        {
            var changed = UserCode != code;
            UserCode = code;
            if (changed)
            {
                UserCodeChange();
            }
        }


        /// <summary>
        /// Sets the UserCode on the bridge object. Called from controller. A changed code will
        /// fire method UserCodeChange.  Override that to handle changes
        /// </summary>
        /// <param name="code"></param>
        /// <param name="qrChecksum">Checksum of the QR code. Used for Cisco codec branding command</param>
        public void SetUserCode(string code, string qrChecksum)
        {
            QrCodeChecksum = qrChecksum;

            SetUserCode(code);
        }

        /// <summary>
        /// Update the App Url with the provided URL
        /// </summary>
        /// <param name="url">The new App URL</param>
        public virtual void UpdateAppUrl(string url)
        {
            AppUrl = url;

            var handler = AppUrlChanged;

            if (handler == null) return;

            handler(this, new EventArgs());
        }

        /// <summary>
        /// Empty method in base class.  Override this to add functionality
        /// when code changes
        /// </summary>
        protected virtual void UserCodeChange()
        {
            this.LogDebug("Server user code changed: {userCode}", UserCode);

            var qrUrl = string.Format($"{Parent.Host}/api/rooms/{Parent.SystemUuid}/{RoomKey}/qr?x={new Random().Next()}");
            QrCodeUrl = qrUrl;

            this.LogDebug("Server user code changed: {userCode} - {qrCodeUrl}", UserCode, qrUrl);

            OnUserCodeChanged();
        }

        /// <summary>
        /// Trigger the UserCodeChanged event
        /// </summary>
        protected void OnUserCodeChanged()
        {
            UserCodeChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Trigger the UserPromptedForCode event
        /// </summary>
        protected void OnUserPromptedForCode()
        {
            UserPromptedForCode?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Trigger the ClientJoined event
        /// </summary>
        protected void OnClientJoined()
        {
            ClientJoined?.Invoke(this, new EventArgs());
        }
    }
}