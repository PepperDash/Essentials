using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;

using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.DSP;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.Occupancy;
using PepperDash.Essentials.Devices.Common.Environment;



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
			var propAnon = new {};
			JsonConvert.DeserializeAnonymousType(dc.Properties.ToString(), propAnon);

			var typeName = dc.Type.ToLower();
			var groupName = dc.Group.ToLower();

			if (typeName == "appletv")
			{
				var irCont = IRPortHelper.GetIrOutputPortController(dc);
				return new AppleTV(key, name, irCont);
			}
			else if (typeName == "analogwaylivecore")
			{
				var comm = CommFactory.CreateCommForDevice(dc);
				var props = JsonConvert.DeserializeObject<AnalogWayLiveCorePropertiesConfig>(
					properties.ToString());
				return new AnalogWayLiveCore(key, name, comm, props);
			}
			else if (typeName == "basicirdisplay")
			{
				var ir = IRPortHelper.GetIrPort(properties);
                if (ir != null)
                {
                    var display = new BasicIrDisplay(key, name, ir.Port, ir.FileName);
                    display.IrPulseTime = 200;       // Set default pulse time for IR commands.
                    return display;
                }
			}

            else if (typeName == "biamptesira")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<BiampTesiraFortePropertiesConfig>(
                    properties.ToString());
                return new BiampTesiraForteDsp(key, name, comm, props);
            }


			else if (typeName == "cameravisca")
			{
				var comm = CommFactory.CreateCommForDevice(dc);
				var props = JsonConvert.DeserializeObject<Cameras.CameraPropertiesConfig>(
					properties.ToString());
				return new Cameras.CameraVisca(key, name, comm, props);
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
			else if (typeName == "digitallogger")
			{
				// var comm = CommFactory.CreateCommForDevice(dc);
				var props = JsonConvert.DeserializeObject<DigitalLoggerPropertiesConfig>(
					properties.ToString());
				return new DigitalLogger(key, name, props);
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
				return new Core.Devices.InRoomPc(key, name);
			}

			else if (typeName == "laptop")
			{
				return new Core.Devices.Laptop(key, name);
			}

			else if (typeName == "mockvc")
			{
				return new VideoCodec.MockVC(dc);
			}

			else if (typeName == "mockac")
			{
				var props = JsonConvert.DeserializeObject<AudioCodec.MockAcPropertiesConfig>(properties.ToString());
				return new AudioCodec.MockAC(key, name, props);
			}

			else if (typeName.StartsWith("ciscospark"))
			{
				var comm = CommFactory.CreateCommForDevice(dc);
				return new VideoCodec.Cisco.CiscoSparkCodec(dc, comm);
			}

            else if (typeName == "zoomroom")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                return new VideoCodec.ZoomRoom.ZoomRoom(dc, comm);
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
                    Debug.Console(0, "ERROR: Unable to add digital input device with key '{0}'. Port Device does not support digital inputs", key);
                else
                {
                    var cs = (portDevice as CrestronControlSystem);
                    if (cs == null)
                    {
                        Debug.Console(0, "ERROR: Port device for [{0}] is not control system", props.PortDeviceKey);
                        return null;
                    }

                    if (cs.SupportsVersiport)
                    {
                        Debug.Console(1, "Attempting to add Digital Input device to Versiport port '{0}'", props.PortNumber);

                        if (props.PortNumber > cs.NumberOfVersiPorts)
                        {
                            Debug.Console(0, "WARNING: Cannot add Vesiport {0} on {1}. Out of range",
                                props.PortNumber, props.PortDeviceKey);
                            return null;
                        }

                        Versiport vp = cs.VersiPorts[props.PortNumber];

                        if (!vp.Registered)
                        {
                            var regSuccess = vp.Register();
                            if (regSuccess == eDeviceRegistrationUnRegistrationResponse.Success)
                            {
                                Debug.Console(1, "Successfully Created Digital Input Device on Versiport");
                                return new GenericVersiportDigitalInputDevice(key, vp, props);
                            }
                            else
                            {
                                Debug.Console(0, "WARNING: Attempt to register versiport {0} on device with key '{1}' failed: {2}",
                                    props.PortNumber, props.PortDeviceKey, regSuccess);
                                return null;
                            }
                        }
                    }
                    else if (cs.SupportsDigitalInput)
                    {
                        Debug.Console(1, "Attempting to add Digital Input device to Digital Input port '{0}'", props.PortNumber);

                        if (props.PortNumber > cs.NumberOfDigitalInputPorts)
                        {
                            Debug.Console(0, "WARNING: Cannot register DIO port {0} on {1}. Out of range",
                                props.PortNumber, props.PortDeviceKey);
                            return null;
                        }

                        DigitalInput digitalInput = cs.DigitalInputPorts[props.PortNumber];

                        if (!digitalInput.Registered)
                        {
                            if (digitalInput.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                            {
                                Debug.Console(1, "Successfully Created Digital Input Device on Digital Input");
                                return new GenericDigitalInputDevice(key, digitalInput);
                            }
                            else
                                Debug.Console(0, "WARNING: Attempt to register digital input {0} on device with key '{1}' failed.",
                                    props.PortNumber, props.PortDeviceKey);
                        }
                    }
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

                    if (cs != null)
                    {
                        // The relay is on a control system processor
                        if (!cs.SupportsRelay || props.PortNumber > cs.NumberOfRelayPorts)
                        {
                            Debug.Console(0, "Port Device: {0} does not support relays or does not have enough relays");
                            return null;
                        }
                    }
                    else
                    {
                        // The relay is on another device type

                        if (props.PortNumber > portDevice.NumberOfRelayPorts)
                        {
                            Debug.Console(0, "Port Device: {0} does not have enough relays");
                            return null;
                        }
                    }

                    Relay relay = portDevice.RelayPorts[props.PortNumber];

                    if (!relay.Registered)
                    {
                        if (relay.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                            return new GenericRelayDevice(key, relay);
                        else
                            Debug.Console(0, "Attempt to register relay {0} on device with key '{1}' failed.", props.PortNumber, props.PortDeviceKey);
                    }
                    else
                    {
                        return new GenericRelayDevice(key, relay);
                    }

                    // Future: Check if portDevice is 3-series card or other non control system that supports versiports
                }
            }

            else if (typeName == "microphoneprivacycontroller")
            {
                var props = JsonConvert.DeserializeObject<Core.Privacy.MicrophonePrivacyControllerConfig>(properties.ToString());

                return new Core.Privacy.MicrophonePrivacyController(key, props);
            }
            else if (typeName == "roku")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                return new Roku2(key, name, irCont);
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
            else if (typeName == "tvonecorio")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<TVOneCorioPropertiesConfig>(
                    properties.ToString());
                return new TVOneCorio(key, name, comm, props);
            }


            else if (typeName == "glsoirccn")
            {
                var comm = CommFactory.GetControlPropertiesConfig(dc);

                GlsOccupancySensorBase occSensor = null;

                occSensor = new GlsOirCCn(comm.CresnetIdInt, Global.ControlSystem);

                if (occSensor != null)
                    return new GlsOccupancySensorBaseController(key, name, occSensor);
                else
                    Debug.Console(0, "ERROR: Unable to create Occupancy Sensor Device. Key: '{0}'", key);
            }

            else if (typeName == "glsodtccn")
            {
                var comm = CommFactory.GetControlPropertiesConfig(dc);

                var occSensor = new GlsOdtCCn(comm.CresnetIdInt, Global.ControlSystem);

                if (occSensor != null)
                    return new GlsOdtOccupancySensorController(key, name, occSensor);
                else
                    Debug.Console(0, "ERROR: Unable to create Occupancy Sensor Device. Key: '{0}'", key);
            }

            else if (groupName == "lighting")
            {
                if (typeName == "lutronqs")
                {
                    var comm = CommFactory.CreateCommForDevice(dc);

                    var props = JsonConvert.DeserializeObject<Environment.Lutron.LutronQuantumPropertiesConfig>(properties.ToString());

                    return new Environment.Lutron.LutronQuantumArea(key, name, comm, props);
                }
                else if (typeName == "din8sw8")
                {
                    var comm = CommFactory.GetControlPropertiesConfig(dc);

                    return new Environment.Lighting.Din8sw8Controller(key, comm.CresnetIdInt);
                }

            }

            else if (groupName == "environment")
            {
                if (typeName == "shadecontroller")
                {
                    var props = JsonConvert.DeserializeObject<Core.Shades.ShadeControllerConfigProperties>(properties.ToString());

                    return new Core.Shades.ShadeController(key, name, props);
                }
                else if (typeName == "relaycontrolledshade")
                {
                    var props = JsonConvert.DeserializeObject<Environment.Somfy.RelayControlledShadeConfigProperties>(properties.ToString());

                    return new Environment.Somfy.RelayControlledShade(key, name, props);
                }

            }

			return null;
		}
	}
}