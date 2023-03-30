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
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.DeviceInfo;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    public class DgeBaseController : CrestronGenericBridgeableBaseDevice, IComPorts, IIROutputPorts, IHasBasicTriListWithSmartObject, ICec, IDeviceInfoProvider, IDgeRoutingWithFeedback
    {
        private const int CtpPort = 41795;
        private readonly Dge100 _dge;

        public RoutingInputPort HdmiIn { get; protected set; }
        public RoutingInputPort ProjectViewIn { get; protected set; }
        public RoutingOutputPort HdmiOut { get; protected set; }

        protected readonly ushort DmInputJoin;
        protected readonly ushort HdmiInputJoin;
        protected readonly ushort ProjectViewJoin;

        private int SwitchValue;

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get;
            private set;
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get;
            private set;
        }

        private readonly TsxCcsUcCodec100EthernetReservedSigs _dgeEthernetInfo;

        public BasicTriListWithSmartObject Panel { get { return _dge; } }

        private DeviceConfig _dc;

        CrestronDgePropertiesConfig PropertiesConfig;

        public DgeBaseController(string key, string name, Dge100 device, DeviceConfig dc, CrestronDgePropertiesConfig props)
            :base(key, name, device)
        {
            _dge = device;
            _dc = dc;

            PropertiesConfig = props;

            DmInputJoin = (ushort)PropertiesConfig.DmInputJoin;
            HdmiInputJoin = (ushort)PropertiesConfig.HdmiInputJoin;
            ProjectViewJoin = (ushort)PropertiesConfig.ProjectViewVirtualInputJoin;

            // Set Ports for CEC
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this);

            HdmiOut.Port = _dge.HdmiOut;


            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };


            _dgeEthernetInfo = _dge.ExtenderEthernetReservedSigs;
            //_dgeEthernetInfo.DeviceExtenderSigChange += (extender, args) => UpdateDeviceInfo();
            _dgeEthernetInfo.Use();

            DeviceInfo = new DeviceInfo();

            AudioVideoSourceNumericFeedback = new IntFeedback(() => SwitchValue);


            _dge.OnlineStatusChange += (currentDevice, args) => { if (args.DeviceOnLine) UpdateDeviceInfo(); };

            _dge.SigChange += TrilistChange;

        }

        public virtual void TrilistChange(BasicTriList currentDevice, SigEventArgs args)
        {

        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {

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

        public event DeviceInfoChangeHandler DeviceInfoChanged;

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

            var tcpClient = new GenericTcpIpClient("", _dgeEthernetInfo.IpAddressFeedback.StringValue, CtpPort, 1024) { AutoReconnect = false };

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
                try
                {
                    Debug.Console(1, this, "{0}", args.Text);

                    if (args.Text.ToLower().Contains("host"))
                    {
                        DeviceInfo.HostName = args.Text.Split(':')[1].Trim();

                        Debug.Console(1, this, "hostname: {0}", DeviceInfo.HostName);
                        tcpClient.Disconnect();
                        return;
                    }

                    if (!args.Text.Contains('['))
                    {
                        return;
                    }
                    var splitResponse = args.Text.Split('[');

                    foreach (string t in splitResponse)
                    {
                        Debug.Console(1, this, "{0}", t);
                    }

                    DeviceInfo.SerialNumber = splitResponse[1].Split(' ')[4].Replace("#", "");
                    DeviceInfo.FirmwareVersion = splitResponse[1].Split(' ')[0];

                    Debug.Console(1, this, "Firmware: {0} SerialNumber: {1}", DeviceInfo.FirmwareVersion,
                        DeviceInfo.SerialNumber);

                    tcpClient.SendText("host\r\n");
                }
                catch (Exception ex)
                {
                    Debug.Console(0, this, "Exception getting data: {0}", ex.Message);
                    Debug.Console(0, this, "response: {0}", args.Text);
                }
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




        #region IDgeRouting Members

        public IntFeedback AudioVideoSourceNumericFeedback { get; set; }

        #endregion

        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType type)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}