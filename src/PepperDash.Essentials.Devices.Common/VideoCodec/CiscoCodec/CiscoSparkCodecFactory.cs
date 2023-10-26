using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    public class CiscoSparkCodecFactory : EssentialsDeviceFactory<CiscoSparkCodec>
    {
        public CiscoSparkCodecFactory()
        {
            TypeNames = new List<string>() { "ciscospark", "ciscowebex", "ciscowebexpro", "ciscoroomkit", "ciscosparkpluscodec" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Cisco Codec Device");

            var comm = CommFactory.CreateCommForDevice(dc);
            return new VideoCodec.Cisco.CiscoSparkCodec(dc, comm);
        }
    }
}