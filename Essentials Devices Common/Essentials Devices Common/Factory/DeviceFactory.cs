using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;

using PepperDash.Essentials.Devices.Common.DSP;
using PepperDash.Essentials.Devices.Common.VideoCodec;

using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Devices.Common
{
	public class DeviceFactory
	{
		public static IKeyed GetDevice(DeviceConfig dc)
		{
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;

			var typeName = dc.Type.ToLower();
			var groupName = dc.Group.ToLower();

			if (typeName == "appletv")
			{
				//var ir = IRPortHelper.GetIrPort(properties);
				//if (ir != null)
				//    return new AppleTV(key, name, ir.Port, ir.FileName);

				var irCont = IRPortHelper.GetIrOutputPortController(dc);
				return new AppleTV(key, name, irCont);
			}

			else if (typeName == "basicirdisplay")
			{
				var ir = IRPortHelper.GetIrPort(properties);
				if (ir != null)
					return new BasicIrDisplay(key, name, ir.Port, ir.FileName);
			}

            else if (typeName == "biamptesira")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<BiampTesiraFortePropertiesConfig>(
                    properties.ToString());
                return new BiampTesiraForteDsp(key, name, comm, props);
            }

			else if (typeName == "cenrfgwex")
			{
				return CenRfgwController.GetNewExGatewayController(key, name,
					properties.Value<string>("id"), properties.Value<string>("gatewayType"));
			}

			else if (typeName == "cenerfgwpoe")
			{
				return CenRfgwController.GetNewErGatewayController(key, name,
					properties.Value<string>("id"), properties.Value<string>("gatewayType"));
			}

            else if (groupName == "discplayer") // (typeName == "irbluray")
            {
                if (properties["control"]["method"].Value<string>() == "ir")
                {
                    var irCont = IRPortHelper.GetIrOutputPortController(dc);
                    return new IRBlurayBase(key, name, irCont);
                }
                else if (properties["control"]["method"].Value<string>() == "com")
                {
                    Debug.Console(0, "[{0}] COM Device type not implemented YET!", key);
                }
            }

			else if (typeName == "genericaudiooutwithvolume")
			{
				var zone = dc.Properties.Value<uint>("zone");
				return new GenericAudioOutWithVolume(key, name,
					dc.Properties.Value<string>("volumeDeviceKey"), zone);
			}

            else if (groupName == "genericsource")
            {
                return new GenericSource(key, name);
            }

            else if (typeName == "inroompc")
            {
                return new InRoomPc(key, name);
            }

            else if (typeName == "laptop")
            {
                return new Laptop(key, name);
            }

            else if (typeName == "mockvc")
            {
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.Devices.Common.VideoCodec.MockVcPropertiesConfig>(properties.ToString());
				return new PepperDash.Essentials.Devices.Common.VideoCodec
                    .MockVC(key, name, props);
            }

            else if (typeName.StartsWith("ciscospark"))
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<Codec.CiscoSparkCodecPropertiesConfig>(properties.ToString());
                return new PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.CiscoSparkCodec(key, name, comm, props);
            }

            else if (typeName == "versiportinput")
            {
                var props = JsonConvert.DeserializeObject<IOPortConfig>(properties.ToString());

                IIOPorts portDevice;

                if (props.PortDeviceKey == "processor")
                    portDevice = Global.ControlSystem as IIOPorts;
                else
                    portDevice = DeviceManager.GetDeviceForKey(props.PortDeviceKey) as IIOPorts;

                if(portDevice == null)
                    Debug.Console(0, "Unable to add versiport device with key '{0}'. Port Device does not support versiports", key);
                else
                {
                    var cs = (portDevice as CrestronControlSystem);

                    if (cs != null)
                        if (cs.SupportsVersiport && props.PortNumber <= cs.NumberOfVersiPorts)
                        {
                            Versiport versiport = cs.VersiPorts[props.PortNumber];

                            if(!versiport.Registered)
                            {
                                if (versiport.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                                    return new GenericVersiportInputDevice(key, versiport);
                                else
                                    Debug.Console(0, "Attempt to register versiport {0} on device with key '{1}' failed.", props.PortNumber, props.PortDeviceKey);
                            }
                        }

                    // Future: Check if portDevice is 3-series card or other non control system that supports versiports
                            
                }
            }

            else if (typeName == "digitalinput")
            {
                var props = JsonConvert.DeserializeObject<IOPortConfig>(properties.ToString());

                IDigitalInputPorts portDevice;

                if (props.PortDeviceKey == "processor")
                    portDevice = Global.ControlSystem as IDigitalInputPorts;
                else
                    portDevice = DeviceManager.GetDeviceForKey(props.PortDeviceKey) as IDigitalInputPorts;

                if (portDevice == null)
                    Debug.Console(0, "Unable to add digital input device with key '{0}'. Port Device does not support digital inputs", key);
                else
                {
                    var cs = (portDevice as CrestronControlSystem);

                    if (cs != null)
                        if (cs.SupportsDigitalInput && props.PortNumber <= cs.NumberOfDigitalInputPorts)
                        {
                            DigitalInput digitalInput = cs.DigitalInputPorts[props.PortNumber];

                            if (!digitalInput.Registered)
                            {
                                if(digitalInput.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                                    return new GenericDigitalInputDevice(key, digitalInput);
                                else
                                    Debug.Console(0, "Attempt to register digital input {0} on device with key '{1}' failed.", props.PortNumber, props.PortDeviceKey);
                            }
                        }
                    // Future: Check if portDevice is 3-series card or other non control system that supports versiports
                }
            }

            else if (typeName == "relayoutput")
            {
                var props = JsonConvert.DeserializeObject<IOPortConfig>(properties.ToString());

                IRelayPorts portDevice;

                if (props.PortDeviceKey == "processor")
                    portDevice = Global.ControlSystem as IRelayPorts;
                else
                    portDevice = DeviceManager.GetDeviceForKey(props.PortDeviceKey) as IRelayPorts;

                if (portDevice == null)
                    Debug.Console(0, "Unable to add relay device with key '{0}'. Port Device does not support relays", key);
                else
                {
                    var cs = (portDevice as CrestronControlSystem);

                    if(cs != null)
                        if (cs.SupportsRelay && props.PortNumber <= cs.NumberOfRelayPorts)
                        {
                            Relay relay = cs.RelayPorts[props.PortNumber];

                            if (!relay.Registered)
                            {
                                if(relay.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                                    return new GenericRelayDevice(key, relay);
                                else
                                    Debug.Console(0, "Attempt to register relay {0} on device with key '{1}' failed.", props.PortNumber, props.PortDeviceKey);
                            }
                        }

                    // Future: Check if portDevice is 3-series card or other non control system that supports versiports
                }
            }

            else if (typeName == "microphoneprivacycontroller")
            {
                var props = JsonConvert.DeserializeObject<Microphones.MicrophonePrivacyControllerConfig>(properties.ToString());

                return new Microphones.MicrophonePrivacyController(key, props);
            }

            else if (groupName == "settopbox") //(typeName == "irstbbase")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
                var stb = new IRSetTopBoxBase(key, name, irCont, config);

                //stb.HasDvr = properties.Value<bool>("hasDvr");
                var listName = properties.Value<string>("presetsList");
                if (listName != null)
                    stb.LoadPresets(listName);
                return stb;
            }

            else if (typeName == "roku")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                return new Roku2(key, name, irCont);
            }

			return null;
		}
	}
}