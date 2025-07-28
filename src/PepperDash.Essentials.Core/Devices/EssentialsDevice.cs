using System;
using System.Threading.Tasks;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Defines the basic needs for an EssentialsDevice to enable it to be build by an IDeviceFactory class
    /// </summary>
    [Description("The base Essentials Device Class")]
    public abstract class EssentialsDevice : Device
    {
        /// <summary>
        /// Event raised when the device is initialized.
        /// </summary>
        public event EventHandler Initialized;

        private bool _isInitialized;

        /// <summary>
        /// Gets a value indicating whether the device is initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                if (_isInitialized == value) return;

                _isInitialized = value;

                if (_isInitialized)
                {
                    Initialized?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the EssentialsDevice class.
        /// </summary>
        /// <param name="key">The unique identifier for the device.</param>
        protected EssentialsDevice(string key)
            : base(key)
        {
            SubscribeToActivateComplete();
        }

        /// <summary>
        /// Initializes a new instance of the EssentialsDevice class.
        /// </summary>
        /// <param name="key">The unique identifier for the device.</param>
        /// <param name="name">The name of the device.</param>
        protected EssentialsDevice(string key, string name)
            : base(key, name)
        {
            SubscribeToActivateComplete();
        }

        private void SubscribeToActivateComplete()
        {
            DeviceManager.AllDevicesActivated += DeviceManagerOnAllDevicesActivated;
        }

        private void DeviceManagerOnAllDevicesActivated(object sender, EventArgs eventArgs)
        {
            Task.Run(() =>
            {
                try
                {
                    Initialize();

                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(LogEventLevel.Error, this, "Exception initializing device: {0}", ex.Message);
                    Debug.LogMessage(LogEventLevel.Debug, this, "Stack Trace: {0}", ex.StackTrace);
                }
            });
        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
        {
            CreateMobileControlMessengers();

            return base.CustomActivate();
        }

        /// <summary>
        /// Override this method to build and create custom Mobile Control Messengers during the Activation phase
        /// </summary>
        protected virtual void CreateMobileControlMessengers()
        {

        }
    }
}