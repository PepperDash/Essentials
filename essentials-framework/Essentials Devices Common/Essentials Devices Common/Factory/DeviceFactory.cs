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
        public DeviceFactory()
        {
            var appleTVFactory = new AppleTVFactory() as IDeviceFactory;
            appleTVFactory.LoadTypeFactories();

            var analogWayLCFactory = new AnalogWayLiveCoreFactory() as IDeviceFactory;
            analogWayLCFactory.LoadTypeFactories();

            var basicIrDisplayFactory = new BasicIrDisplayFactory() as IDeviceFactory;
            basicIrDisplayFactory.LoadTypeFactories();

            var biampTesiraFactory = new BiampTesiraForteDspFactory() as IDeviceFactory;
            biampTesiraFactory.LoadTypeFactories();

            var cameraViscaFactory = new Cameras.CameraViscaFactory() as IDeviceFactory;
            cameraViscaFactory.LoadTypeFactories();

            var cenRfGwFactory = new CenRfgwControllerFactory() as IDeviceFactory;
            cenRfGwFactory.LoadTypeFactories();

            var irBlurayFactory = new IRBlurayBaseFactory() as IDeviceFactory;
            irBlurayFactory.LoadTypeFactories();

            var digitalLoggerFactory = new DigitalLoggerFactory() as IDeviceFactory;
            digitalLoggerFactory.LoadTypeFactories();

            var genericAudioOutFactory = new GenericAudioOutWithVolumeFactory() as IDeviceFactory;
            genericAudioOutFactory.LoadTypeFactories();

            var genericSourceFactory = new GenericSourceFactory() as IDeviceFactory;
            genericSourceFactory.LoadTypeFactories();

            var inRoomPcFactory = new Core.Devices.InRoomPcFactory() as IDeviceFactory;
            inRoomPcFactory.LoadTypeFactories();

            var laptopFactory = new Core.Devices.LaptopFactory() as IDeviceFactory;
            laptopFactory.LoadTypeFactories();

            var blueJeansPcFactory = new SoftCodec.BlueJeansPcFactory() as IDeviceFactory;
            blueJeansPcFactory.LoadTypeFactories();

            var mockAcFactory = new AudioCodec.MockACFactory() as IDeviceFactory;
            mockAcFactory.LoadTypeFactories();

            var mockVcFactory = new VideoCodec.MockVCFactory() as IDeviceFactory;
            mockVcFactory.LoadTypeFactories();

            var ciscoCodecFactory = new VideoCodec.Cisco.CiscoSparkCodecFactory() as IDeviceFactory;
            ciscoCodecFactory.LoadTypeFactories();

            var zoomRoomFactory = new VideoCodec.ZoomRoom.ZoomRoomFactory() as IDeviceFactory;
            zoomRoomFactory.LoadTypeFactories();

            var digitalInputFactory = new GenericDigitalInputDeviceFactory() as IDeviceFactory;
            digitalInputFactory.LoadTypeFactories();

            var relayFactory = new GenericRelayDeviceFactory() as IDeviceFactory;
            relayFactory.LoadTypeFactories();

            var micPrivacyFactory = new Core.Privacy.MicrophonePrivacyControllerFactory() as IDeviceFactory;
            micPrivacyFactory.LoadTypeFactories();

            var rokuFactory = new Roku2Factory() as IDeviceFactory;
            rokuFactory.LoadTypeFactories();

            var setTopBoxFactory = new IRSetTopBoxBaseFactory() as IDeviceFactory;
            setTopBoxFactory.LoadTypeFactories();

            var tvOneCorioFactory = new TVOneCorioFactory() as IDeviceFactory;
            tvOneCorioFactory.LoadTypeFactories();

            var glsOccSensorBaseFactory = new GlsOccupancySensorBaseControllerFactory() as IDeviceFactory;
            glsOccSensorBaseFactory.LoadTypeFactories();

            var glsOdtOccSensorFactory = new GlsOdtOccupancySensorControllerFactory() as IDeviceFactory;
            glsOdtOccSensorFactory.LoadTypeFactories();

            var lutronQuantumFactory = new Environment.Lutron.LutronQuantumAreaFactory() as IDeviceFactory;
            lutronQuantumFactory.LoadTypeFactories();

            var din8sw8ControllerFactory = new Environment.Lighting.Din8sw8ControllerFactory() as IDeviceFactory;
            din8sw8ControllerFactory.LoadTypeFactories();

            var shadeControllerFactory = new Core.Shades.ShadeControllerFactory() as IDeviceFactory;
            shadeControllerFactory.LoadTypeFactories();

            var relayControlledShadeFactory = new Environment.Somfy.RelayControlledShadeFactory() as IDeviceFactory;
            relayControlledShadeFactory.LoadTypeFactories();
        }
	}
}