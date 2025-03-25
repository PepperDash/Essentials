using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;


namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MobileControlBridgeBase : MessengerBase, IMobileControlRoomMessenger
    {
        public event EventHandler<EventArgs> UserCodeChanged;

        public event EventHandler<EventArgs> UserPromptedForCode;

        public event EventHandler<EventArgs> ClientJoined;

        public event EventHandler<EventArgs> AppUrlChanged;

        public IMobileControl Parent { get; private set; }

        public string AppUrl { get; private set; }
        public string UserCode { get; private set; }

        public string QrCodeUrl { get; protected set; }

        public string QrCodeChecksum { get; protected set; }

        public string McServerUrl { get; private set; }

        public abstract string RoomName { get; }

        public abstract string RoomKey { get; }

        protected MobileControlBridgeBase(string key, string messagePath)
            : base(key, messagePath)
        {
        }

        protected MobileControlBridgeBase(string key, string messagePath, IKeyName device)
            : base(key, messagePath, device)
        {
        }

        /// <summary>
        /// Set the parent.  Does nothing else.  Override to add functionality such
        /// as adding actions to parent
        /// </summary>
        /// <param name="parent"></param>
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
            Debug.Console(1, this, "Server user code changed: {0}", UserCode);

            var qrUrl = string.Format($"{Parent.Host}/api/rooms/{Parent.SystemUuid}/{RoomKey}/qr?x={new Random().Next()}");
            QrCodeUrl = qrUrl;

            Debug.Console(1, this, "Server user code changed: {0} - {1}", UserCode, qrUrl);

            OnUserCodeChanged();
        }

        protected void OnUserCodeChanged()
        {
            UserCodeChanged?.Invoke(this, new EventArgs());
        }

        protected void OnUserPromptedForCode()
        {
            UserPromptedForCode?.Invoke(this, new EventArgs());
        }

        protected void OnClientJoined()
        {
            ClientJoined?.Invoke(this, new EventArgs());
        }
    }
}