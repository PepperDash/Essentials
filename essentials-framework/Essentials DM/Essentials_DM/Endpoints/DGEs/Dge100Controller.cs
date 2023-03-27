﻿using System;
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
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM.Endpoints.DGEs
{
    [Description("Wrapper class for DGE-100")]    
    public class Dge100Controller : CrestronGenericBaseDevice, IComPorts, IIROutputPorts, IHasBasicTriListWithSmartObject, ICec, IDeviceInfoProvider, IBridgeAdvanced
    {
        private const int CtpPort = 41795;
        private readonly Dge100 _dge;

        private readonly TsxCcsUcCodec100EthernetReservedSigs _dgeEthernetInfo;

        public BasicTriListWithSmartObject Panel { get { return _dge; } }

        private DeviceConfig _dc;

		public VideoStatusOutputs VideoStatusFeedbacks { get; private set;  }

        CrestronTouchpanelPropertiesConfig PropertiesConfig;        


        public Dge100Controller(string key, string name, Dge100 device, DeviceConfig dc, CrestronTouchpanelPropertiesConfig props)
            :base(key, name, device)
        {
            _dge = device;
            _dgeEthernetInfo = _dge.ExtenderEthernetReservedSigs;
            //_dgeEthernetInfo.DeviceExtenderSigChange += (extender, args) => UpdateDeviceInfo();
            _dgeEthernetInfo.Use();

            DeviceInfo = new DeviceInfo();

            _dge.OnlineStatusChange += (currentDevice, args) => { if (args.DeviceOnLine) UpdateDeviceInfo(); };

            _dc = dc;

            PropertiesConfig = props;

            var videoStatusFuncs = new VideoStatusFuncsWrapper
            {
                HdcpActiveFeedbackFunc = () => _dge.HdmiIn.HdcpSupportOnFeedback.BoolValue,
                VideoResolutionFeedbackFunc = () => _dge.HdmiIn.VideoAttributes.GetVideoResolutionString(),
                VideoSyncFeedbackFunc = () => _dge.HdmiIn.SyncDetectedFeedback.BoolValue, 
            };

			VideoStatusFeedbacks = new VideoStatusOutputs(videoStatusFuncs);

			_dge.HdmiIn.StreamChange += (s,a) => VideoStatusFeedbacks.FireAll();
			_dge.HdmiIn.VideoAttributes.AttributeChange += (o, a) => VideoStatusFeedbacks.FireAll();
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

            var tcpClient = new GenericTcpIpClient("", _dgeEthernetInfo.IpAddressFeedback.StringValue, CtpPort, 1024){AutoReconnect = false};

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

                    //ignore console prompt
                    /*if (args.Text.ToLower().Contains(">"))
                    {
                        Debug.Console(1, this, "Ignoring console");
                        return;
                    }

                    if (args.Text.ToLower().Contains("dge"))
                    {
                        Debug.Console(1, this, "Ignoring DGE");
                        return;
                    }*/

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

        #region IBridgeAdvanced Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DgeJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DgeJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

			//Presses
			trilist.SetSigTrueAction(joinMap.HdmiInHdcpOn.JoinNumber, () => _dge.HdmiIn.HdcpSupportOn());
			trilist.SetSigTrueAction(joinMap.HdmiInHdcpOff.JoinNumber,() => _dge.HdmiIn.HdcpSupportOff());
			trilist.SetSigTrueAction(joinMap.HdmiInHdcpToggle.JoinNumber, () => {
				if(_dge.HdmiIn.HdcpSupportOnFeedback.BoolValue)
				{
					_dge.HdmiIn.HdcpSupportOff();
					return;
				}
			
				_dge.HdmiIn.HdcpSupportOn();
			});


			// Feedbacks
			VideoStatusFeedbacks.HdcpActiveFeedback.LinkInputSig(trilist.BooleanInput[joinMap.HdmiInHdcpOn.JoinNumber]);
			VideoStatusFeedbacks.HdcpActiveFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.HdmiInHdcpOff.JoinNumber]);

			VideoStatusFeedbacks.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentInputResolution.JoinNumber]);
			VideoStatusFeedbacks.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SyncDetected.JoinNumber]);

			IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

			trilist.OnlineStatusChange += (o, a) =>
			{
				if (!a.DeviceOnLine) return;

				VideoStatusFeedbacks.FireAll();
			};
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