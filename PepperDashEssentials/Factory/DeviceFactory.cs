using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.MobileControl;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Responsible for loading all of the device types for this library
    /// </summary>
    public class DeviceFactory
	{
        public DeviceFactory()
        {
            var ampFactory = new AmplifierFactory() as IDeviceFactory;
            ampFactory.LoadTypeFactories();

            var mockDisplayFactory = new MockDisplayFactory() as IDeviceFactory;
            mockDisplayFactory.LoadTypeFactories();

            var consoleCommMockFactroy = new ConsoleCommMockDeviceFactory() as IDeviceFactory;
            consoleCommMockFactroy.LoadTypeFactories();

            var mcSystemControllerFactory = new MobileControlSystemControllerFactory() as IDeviceFactory;
            mcSystemControllerFactory.LoadTypeFactories();

            var mcSIMPLRoomBridgeFactory = new MobileControlSIMPLRoomBridgeFactory() as IDeviceFactory;
            mcSIMPLRoomBridgeFactory.LoadTypeFactories();

            var roomOnToDefSourceWhenOcc = new RoomOnToDefaultSourceWhenOccupiedFactory() as IDeviceFactory;
            roomOnToDefSourceWhenOcc.LoadTypeFactories();
        }
	}
}
