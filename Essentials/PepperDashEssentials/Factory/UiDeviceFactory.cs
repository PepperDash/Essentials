using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.PageManagers;


namespace PepperDash.Essentials
{
    public class UiDeviceFactory
    {
        public static IKeyed GetUiDevice(DeviceConfig config)
        {
            var comm = CommFactory.GetControlPropertiesConfig(config);

            var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(config.Properties.ToString());

            EssentialsTouchpanelController panelController = new EssentialsTouchpanelController(config.Key, config.Name, config.Type, props, comm.IpIdInt);

            panelController.AddPostActivationAction(() =>
                {
                     var mainDriver = new EssentialsPanelMainInterfaceDriver(panelController.Panel, props);
                    // Then the sub drivers

                    // spin up different room drivers depending on room type
                    var room = DeviceManager.GetDeviceForKey(props.DefaultRoomKey);
                    if (room is EssentialsHuddleSpaceRoom)
                    {

                        // Header Driver
                        Debug.Console(0, panelController, "Adding header driver");
                        mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, props);
                        
                        // AV Driver
                        Debug.Console(0, panelController, "Adding huddle space AV driver");
                        var avDriver = new EssentialsHuddlePanelAvFunctionsDriver(mainDriver, props);
                        avDriver.CurrentRoom = room as EssentialsHuddleSpaceRoom;
                        avDriver.DefaultRoomKey = props.DefaultRoomKey;
                        mainDriver.AvDriver = avDriver;

                        // Environment Driver
                        if (avDriver.CurrentRoom.Config.Environment != null && avDriver.CurrentRoom.Config.Environment.DeviceKeys.Count > 0)
                        {
                            Debug.Console(0, panelController, "Adding environment driver");
                            mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, props);

                            mainDriver.EnvironmentDriver.GetDevicesFromConfig(avDriver.CurrentRoom.Config.Environment);
                        }

                        mainDriver.HeaderDriver.SetupHeaderButtons(avDriver, avDriver.CurrentRoom);

                        panelController.LoadAndShowDriver(mainDriver);  // This is a little convoluted.

                        if (panelController.Panel is TswFt5ButtonSystem)
                        {
                            var tsw = panelController.Panel as TswFt5ButtonSystem;
                            // Wire up hard keys
                            tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.PowerButtonPressed(); });
                            //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                            if(mainDriver.EnvironmentDriver != null)
                                tsw.Lights.UserObject = new Action<bool>(b => 
                                {
                                    if (!b)
                                    {
                                        //mainDriver.AvDriver.PopupInterlock.ShowInterlockedWithToggle(mainDriver.EnvironmentDriver.BackgroundSubpageJoin);
                                        mainDriver.EnvironmentDriver.Toggle();
                                    }
                                });
                            tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                            tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                        }
                    }
                    //else if (room is EssentialsPresentationRoom)
                    //{
                    //    Debug.Console(0, panelController, "Adding presentation room driver");
                    //    var avDriver = new EssentialsPresentationPanelAvFunctionsDriver(mainDriver, props);
                    //    avDriver.CurrentRoom = room as EssentialsPresentationRoom;
                    //    avDriver.DefaultRoomKey = props.DefaultRoomKey;
                    //    mainDriver.AvDriver = avDriver ;
                    //    mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, props);
                    //    panelController.LoadAndShowDriver(mainDriver);

                    //    if (panelController.Panel is TswFt5ButtonSystem)
                    //    {
                    //        var tsw = panelController.Panel as TswFt5ButtonSystem;
                    //        // Wire up hard keys
                    //        tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.PowerButtonPressed(); });
                    //        //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                    //        tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                    //        tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                    //    }
                    //}
                    else if (room is EssentialsHuddleVtc1Room)
                    {
                        Debug.Console(0, panelController, "Adding huddle space VTC AV driver");

                        // Header Driver
                        mainDriver.HeaderDriver = new EssentialsHeaderDriver(mainDriver, props);

                        // AV Driver
                        var avDriver = new EssentialsHuddleVtc1PanelAvFunctionsDriver(mainDriver, props);

                        var codecDriver = new PepperDash.Essentials.UIDrivers.VC.EssentialsVideoCodecUiDriver(panelController.Panel, avDriver,
                            (room as EssentialsHuddleVtc1Room).VideoCodec, mainDriver.HeaderDriver);
                        avDriver.SetVideoCodecDriver(codecDriver);
                        avDriver.CurrentRoom = room as EssentialsHuddleVtc1Room;
                        avDriver.DefaultRoomKey = props.DefaultRoomKey;
                        mainDriver.AvDriver = avDriver;

                        // Environment Driver
                        if (avDriver.CurrentRoom.Config.Environment != null && avDriver.CurrentRoom.Config.Environment.DeviceKeys.Count > 0)
                        {
                            Debug.Console(0, panelController, "Adding environment driver");
                            mainDriver.EnvironmentDriver = new EssentialsEnvironmentDriver(mainDriver, props);

                            mainDriver.EnvironmentDriver.GetDevicesFromConfig(avDriver.CurrentRoom.Config.Environment);
                        }

                        mainDriver.HeaderDriver.SetupHeaderButtons(avDriver, avDriver.CurrentRoom);

                        panelController.LoadAndShowDriver(mainDriver);  // This is a little convoluted.

                        if (panelController.Panel is TswFt5ButtonSystem)
                        {
                            var tsw = panelController.Panel as TswFt5ButtonSystem;
                            // Wire up hard keys
                            tsw.Power.UserObject = new Action<bool>(b => { if (!b) avDriver.EndMeetingPress(); });
                            //tsw.Home.UserObject = new Action<bool>(b => { if (!b) HomePressed(); });
                            if (mainDriver.EnvironmentDriver != null)
                                tsw.Lights.UserObject = new Action<bool>(b =>
                                {
                                    if (!b)
                                    {
                                        //mainDriver.AvDriver.PopupInterlock.ShowInterlockedWithToggle(mainDriver.EnvironmentDriver.BackgroundSubpageJoin);
                                        mainDriver.EnvironmentDriver.Toggle();
                                    }
                                });
                            tsw.Up.UserObject = new Action<bool>(avDriver.VolumeUpPress);
                            tsw.Down.UserObject = new Action<bool>(avDriver.VolumeDownPress);
                        }
                    }
                    else
                    {
                        Debug.Console(0, panelController, "ERROR: Cannot load AvFunctionsDriver for room '{0}'", props.DefaultRoomKey);
                    }
                });

            return panelController;
        }

    }
}