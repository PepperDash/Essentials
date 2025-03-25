using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class DeviceInfoMessenger : MessengerBase
    {
        private readonly IDeviceInfoProvider _deviceInfoProvider;
        public DeviceInfoMessenger(string key, string messagePath, IDeviceInfoProvider device) : base(key, messagePath, device as Device)
        {
            _deviceInfoProvider = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            _deviceInfoProvider.DeviceInfoChanged += (o, a) =>
            {
                PostStatusMessage(JToken.FromObject(new
                {
                    deviceInfo = a.DeviceInfo
                }));
            };

            AddAction("/fullStatus", (id, context) => PostStatusMessage(new DeviceInfoStateMessage
            {
                DeviceInfo = _deviceInfoProvider.DeviceInfo
            }));

            AddAction("/update", (id, context) => _deviceInfoProvider.UpdateDeviceInfo());
        }
    }

    public class DeviceInfoStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("deviceInfo")]
        public DeviceInfo DeviceInfo { get; set; }
    }
}
