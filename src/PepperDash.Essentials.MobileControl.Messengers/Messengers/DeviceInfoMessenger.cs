using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Facilitates communication of device information by providing mechanisms for status updates and  device
    /// information reporting.
    /// </summary>
    /// <remarks>The <see cref="DeviceInfoMessenger"/> class integrates with an <see
    /// cref="IDeviceInfoProvider"/> to  manage device-specific information. It uses a debounce timer to limit the
    /// frequency of updates,  ensuring efficient communication. The timer is initialized with a 1-second interval and
    /// is disabled  by default. This class also subscribes to device information change events and provides actions for
    /// reporting full device status and triggering updates.</remarks>
    public class DeviceInfoMessenger : MessengerBase
    {
        private readonly IDeviceInfoProvider _deviceInfoProvider;

        private readonly Timer debounceTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInfoMessenger"/> class, which facilitates communication
        /// of device information.
        /// </summary>
        /// <remarks>The messenger uses a debounce timer to limit the frequency of certain operations. The
        /// timer is initialized with a 1-second interval and is disabled by default.</remarks>
        /// <param name="key">A unique identifier for the messenger instance.</param>
        /// <param name="messagePath">The path used for sending and receiving messages.</param>
        /// <param name="device">An implementation of <see cref="IDeviceInfoProvider"/> that provides device-specific information.</param>
        public DeviceInfoMessenger(string key, string messagePath, IDeviceInfoProvider device) : base(key, messagePath, device as Device)
        {
            _deviceInfoProvider = device;

            debounceTimer = new Timer(1000)
            {
                Enabled = false,
                AutoReset = false
            };

            debounceTimer.Elapsed += DebounceTimer_Elapsed;
        }

        private void DebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PostStatusMessage(JToken.FromObject(new
            {
                deviceInfo = _deviceInfoProvider.DeviceInfo
            }));
        }

        /// <summary>
        /// Registers actions and event handlers for device information updates and status reporting.
        /// </summary>
        /// <remarks>This method sets up actions for handling device status updates and reporting full
        /// device status. It also subscribes to the <see cref="IDeviceInfoProvider.DeviceInfoChanged"/> event to
        /// trigger debounced updates when the device information changes.</remarks>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            _deviceInfoProvider.DeviceInfoChanged += (o, a) =>
            {
                debounceTimer.Stop();
                debounceTimer.Start();
            };

            AddAction("/fullStatus", (id, context) => SendFullStatus(id));

            AddAction("/deviceInfo", (id, content) => SendFullStatus(id));

            AddAction("/update", (id, context) => _deviceInfoProvider.UpdateDeviceInfo());
        }

        private void SendFullStatus(string id = null)
        {
            PostStatusMessage(new DeviceInfoStateMessage
            {
                DeviceInfo = _deviceInfoProvider.DeviceInfo
            }, id);
        }
    }

    /// <summary>
    /// Represents a message containing the state information of a device, including detailed device information.
    /// </summary>
    /// <remarks>This class is used to encapsulate the state of a device along with its associated
    /// information. It extends <see cref="DeviceStateMessageBase"/> to provide additional details about the
    /// device.</remarks>
    /// <summary>
    /// Represents a DeviceInfoStateMessage
    /// </summary>
    public class DeviceInfoStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("deviceInfo")]
        public DeviceInfo DeviceInfo { get; set; }
    }
}
