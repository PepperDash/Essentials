using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Bridges
{
    public class EiscApiAdvancedFactory : EssentialsDeviceFactory<EiscApiAdvanced>
    {
        public EiscApiAdvancedFactory()
        {
            TypeNames = new List<string> { "eiscapiadv", "eiscapiadvanced", "eiscapiadvancedserver", "eiscapiadvancedclient",  "vceiscapiadv", "vceiscapiadvanced" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new EiscApiAdvanced Device");

            var controlProperties = CommFactory.GetControlPropertiesConfig(dc);

            BasicTriList eisc;

            switch (dc.Type.ToLower())
            {
                case "eiscapiadv":
                case "eiscapiadvanced":
                {
                    eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(controlProperties.IpIdInt,
                        controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                    break;
                }
                case "eiscapiadvancedserver":
                {
                    eisc = new EISCServer(controlProperties.IpIdInt, Global.ControlSystem);
                    break;
                }
                case "eiscapiadvancedclient":
                {
                    eisc = new EISCClient(controlProperties.IpIdInt, controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                    break;
                }
                case "vceiscapiadv":
                case "vceiscapiadvanced":
                {
                    if (string.IsNullOrEmpty(controlProperties.RoomId))
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to build VC-4 EISC Client for device {0}. Room ID is missing or empty", dc.Key);
                        eisc = null;
                        break;
                    }
                    eisc = new VirtualControlEISCClient(controlProperties.IpIdInt, controlProperties.RoomId,
                        Global.ControlSystem);
                    break;
                }
                default:
                    eisc = null;
                    break;
            }

            if (eisc == null)
            {
                return null;
            }

            return new EiscApiAdvanced(dc, eisc);
        }
    }
}