using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using Crestron.SimplSharpPro.DM;


using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    [Description("Wrapper class for DGE-100")]    
    public class Dge100Controller : CrestronGenericBaseDevice, IComPorts, IIROutputPorts, IHasBasicTriListWithSmartObject, ICec, IDeviceInfoProvider
    {
        private const int CtpPort = 41795;
        private readonly Dge100 _dge;

        private readonly TsxCcsUcCodec100EthernetReservedSigs _dgeEthernetInfo;

        public BasicTriListWithSmartObject Panel { get { return _dge; } }

        private DeviceConfig _dc;

        CrestronTouchpanelPropertiesConfig PropertiesConfig;

        public Dge100Controller(string key, string name, Dge100 device, DeviceConfig dc, CrestronTouchpanelPropertiesConfig props)
            :base(key, name, device)
        {
            _dge = device;
            _dgeEthernetInfo = _dge.ExtenderEthernetReservedSigs;
            _dgeEthernetInfo.DeviceExtenderSigChange += (extender, args) => UpdateDeviceInfo();
            _dgeEthernetInfo.Use();

            DeviceInfo = new DeviceInfo();

            _dc = dc;

            PropertiesConfig = props;
        }

        #region IComPorts Members

        public CrestronCollection<ComPort> ComPorts
        {
            get { return _dge.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return _dge.NumberOfComPorts; }
        }

        #endregion

        #region IIROutputPorts Members

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _dge.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return _dge.NumberOfIROutputPorts; }
        }

        #endregion

        #region ICec Members
        public Cec StreamCec { get { return _dge.HdmiOut.StreamCec; } }
        #endregion

        #region Implementation of IDeviceInfoProvider

        public DeviceInfo DeviceInfo { get; private set; }

        public event EventHandler<DeviceInfoEventArgs> DeviceInfoChanged;

        public void UpdateDeviceInfo()
        {
            DeviceInfo.IpAddress = _dgeEthernetInfo.IpAddressFeedback.StringValue;
            DeviceInfo.MacAddress = _dgeEthernetInfo.MacAddressFeedback.StringValue;

            GetFirmwareAndSerialInfo();

            OnDeviceInfoChange();
        }

        private void GetFirmwareAndSerialInfo()
        {
            if (String.IsNullOrEmpty(_dgeEthernetInfo.IpAddressFeedback.StringValue))
            {
                Debug.Console(1, this, "IP Address information not yet received. No device is online");
                return;
            }

            var tcpClient = new GenericTcpIpClient("", _dgeEthernetInfo.IpAddressFeedback.StringValue, CtpPort, 1024);

            var gather = new CommunicationGather(tcpClient, "\r\n\r\n");

            tcpClient.ConnectionChange += (sender, args) =>
            {
                if (!args.Client.IsConnected)
                {
                    return;
                }

                args.Client.SendText("ver\r\n");
            };

            gather.LineReceived += (sender, args) =>
            {
                if (args.Text.ToLower().Contains("host"))
                {
                    DeviceInfo.HostName = args.Text.Split(';')[1].Trim();

                    tcpClient.Disconnect();
                    return;
                }

                //ignore console prompt
                if (args.Text.ToLower().Contains(">"))
                {
                    return;
                }

                if (!args.Text.ToLower().Contains("dge"))
                {
                    return;
                }

                DeviceInfo.SerialNumber = args.Text.Split('[')[1].Split(' ')[4].Replace("#", "");
                DeviceInfo.FirmwareVersion = args.Text.Split('[')[1].Split(' ')[1];

                tcpClient.SendText("host\r\n");
            };

            tcpClient.Connect();
        }

        private void OnDeviceInfoChange()
        {
            var handler = DeviceInfoChanged;

            if (handler == null) return;

            handler(this, new DeviceInfoEventArgs(DeviceInfo));
        }

        #endregion
    }

    public class Dge100ControllerFactory : EssentialsDeviceFactory<Dge100Controller>
    {
        public Dge100ControllerFactory()
        {
            TypeNames = new List<string>() { "dge100" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var typeName = dc.Type.ToLower();
            var comm = CommFactory.GetControlPropertiesConfig(dc);
            var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

            Debug.Console(1, "Factory Attempting to create new DgeController Device");

            Dge100 dgeDevice = null;
            if (typeName == "dge100")
                dgeDevice = new Dge100(comm.IpIdInt, Global.ControlSystem);

            if (dgeDevice == null)
            {
                Debug.Console(1, "Unable to create DGE device");
                return null;
            }

            var dgeController = new Dge100Controller(dc.Key, dc.Name, dgeDevice, dc, props);

            return dgeController;
        }
    }
}