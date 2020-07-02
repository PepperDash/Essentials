using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Fusion;

namespace PepperDash.Essentials.Fusion
{
    public class EssentialsHuddleVtc1FusionController : EssentialsHuddleSpaceFusionSystemControllerBase
    {
        private BooleanSigData _codecIsInCall;

        public EssentialsHuddleVtc1FusionController(EssentialsHuddleVtc1Room room, uint ipId)
            : base(room, ipId)
        {
        }

        /// <summary>
        /// Called in base class constructor before RVI and GUID files are built
        /// </summary>
        protected override void ExecuteCustomSteps()
        {
            SetUpCodec();
        }

        /// <summary>
        /// Creates a static asset for the codec and maps the joins to the main room symbol
        /// </summary>
        private void SetUpCodec()
        {
            try
            {
                var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;
                if (essentialsHuddleVtc1Room == null)
                {
                    return;
                }

                var codec = essentialsHuddleVtc1Room.VideoCodec;

                if (codec == null)
                {
                    Debug.Console(1, this, "Cannot link codec to Fusion because codec is null");
                    return;
                }

                codec.UsageTracker = new UsageTracking(codec) {UsageIsTracked = true};
                codec.UsageTracker.DeviceUsageEnded += UsageTracker_DeviceUsageEnded;

                var codecPowerOnAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        codec.StandbyDeactivate();
                    }
                });
                var codecPowerOffAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        codec.StandbyActivate();
                    }
                });

                // Map FusionRoom Attributes:

                // Codec volume
                var codecVolume = FusionRoom.CreateOffsetUshortSig(50, "Volume - Fader01", eSigIoMask.InputOutputSig);
                codecVolume.OutputSig.UserObject =
                    new Action<ushort>(b => (codec as IBasicVolumeWithFeedback).SetVolume(b));
                (codec as IBasicVolumeWithFeedback).VolumeLevelFeedback.LinkInputSig(codecVolume.InputSig);

                // In Call Status
                _codecIsInCall = FusionRoom.CreateOffsetBoolSig(69, "Conf - VC 1 In Call", eSigIoMask.InputSigOnly);
                codec.CallStatusChange += codec_CallStatusChange;

                // Online status
                if (codec is ICommunicationMonitor)
                {
                    var c = codec as ICommunicationMonitor;
                    var codecOnline = FusionRoom.CreateOffsetBoolSig(122, "Online - VC 1", eSigIoMask.InputSigOnly);
                    codecOnline.InputSig.BoolValue = c.CommunicationMonitor.Status == MonitorStatus.IsOk;
                    c.CommunicationMonitor.StatusChange +=
                        (o, a) => { codecOnline.InputSig.BoolValue = a.Status == MonitorStatus.IsOk; };
                    Debug.Console(0, this, "Linking '{0}' communication monitor to Fusion '{1}'", codec.Key,
                        "Online - VC 1");
                }

                // Codec IP Address
                bool codecHasIpInfo = false;
                var codecComm = codec.Communication;

                string codecIpAddress = string.Empty;
                int codecIpPort = 0;

                if (codecComm is GenericSshClient)
                {
                    codecIpAddress = (codecComm as GenericSshClient).Hostname;
                    codecIpPort = (codecComm as GenericSshClient).Port;
                    codecHasIpInfo = true;
                }
                else if (codecComm is GenericTcpIpClient)
                {
                    codecIpAddress = (codecComm as GenericTcpIpClient).Hostname;
                    codecIpPort = (codecComm as GenericTcpIpClient).Port;
                    codecHasIpInfo = true;
                }

                if (codecHasIpInfo)
                {
                    StringSigData codecIpAddressSig = FusionRoom.CreateOffsetStringSig(121, "IP Address - VC",
                        eSigIoMask.InputSigOnly);
                    codecIpAddressSig.InputSig.StringValue = codecIpAddress;

                    StringSigData codecIpPortSig = FusionRoom.CreateOffsetStringSig(150, "IP Port - VC",
                        eSigIoMask.InputSigOnly);
                    codecIpPortSig.InputSig.StringValue = codecIpPort.ToString();
                }

                FusionAsset tempAsset;

                var deviceConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault(c => c.Key.Equals(codec.Key));

                if (FusionStaticAssets.ContainsKey(deviceConfig.Uid))
                {
                    tempAsset = FusionStaticAssets[deviceConfig.Uid];
                }
                else
                {
                    // Create a new asset
                    tempAsset = new FusionAsset(FusionRoomGuids.GetNextAvailableAssetNumber(FusionRoom), codec.Name,
                        "Codec", "");
                    FusionStaticAssets.Add(deviceConfig.Uid, tempAsset);
                }

                var codecAsset = FusionRoom.CreateStaticAsset(tempAsset.SlotNumber, tempAsset.Name, "Display",
                    tempAsset.InstanceId);
                codecAsset.PowerOn.OutputSig.UserObject = codecPowerOnAction;
                codecAsset.PowerOff.OutputSig.UserObject = codecPowerOffAction;
                codec.StandbyIsOnFeedback.LinkComplementInputSig(codecAsset.PowerOn.InputSig);

                // TODO: Map relevant attributes on asset symbol

                codecAsset.TrySetMakeModel(codec);
                codecAsset.TryLinkAssetErrorToCommunication(codec);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error setting up codec in Fusion: {0}", e);
            }
        }

        private void codec_CallStatusChange(object sender, Core.Devices.Codec.CodecCallStatusItemChangeEventArgs e)
        {
            var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;
            if (essentialsHuddleVtc1Room == null)
            {
                return;
            }
            var codec = essentialsHuddleVtc1Room.VideoCodec;

            _codecIsInCall.InputSig.BoolValue = codec.IsInCall;
        }

        // These methods are overridden because they access the room class which is of a different type

        protected override void CreateSymbolAndBasicSigs(uint ipId)
        {
            Debug.Console(1, this, "Creating Fusion Room symbol with GUID: {0}", RoomGuid);

            FusionRoom = new FusionRoom(ipId, Global.ControlSystem, Room.Name, RoomGuid);
            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();
            FusionRoom.ExtenderFusionRoomDataReservedSigs.Use();

            FusionRoom.Register();

            FusionRoom.FusionStateChange += FusionRoom_FusionStateChange;

            FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.DeviceExtenderSigChange +=
                FusionRoomSchedule_DeviceExtenderSigChange;
            FusionRoom.ExtenderFusionRoomDataReservedSigs.DeviceExtenderSigChange +=
                ExtenderFusionRoomDataReservedSigs_DeviceExtenderSigChange;
            FusionRoom.OnlineStatusChange += FusionRoom_OnlineStatusChange;

            CrestronConsole.AddNewConsoleCommand(RequestFullRoomSchedule, "FusReqRoomSchedule",
                "Requests schedule of the room for the next 24 hours", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ModifyMeetingEndTimeConsoleHelper, "FusReqRoomSchMod",
                "Ends or extends a meeting by the specified time", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(CreateAdHocMeeting, "FusCreateMeeting",
                "Creates and Ad Hoc meeting for on hour or until the next meeting",
                ConsoleAccessLevelEnum.AccessOperator);

            // Room to fusion room
            Room.OnFeedback.LinkInputSig(FusionRoom.SystemPowerOn.InputSig);

            // Moved to 
            CurrentRoomSourceNameSig = FusionRoom.CreateOffsetStringSig(84, "Display 1 - Current Source",
                eSigIoMask.InputSigOnly);
            // Don't think we need to get current status of this as nothing should be alive yet. 

            var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;
            if (essentialsHuddleVtc1Room != null)
            {
                essentialsHuddleVtc1Room.CurrentSourceChange += Room_CurrentSourceInfoChange;
                FusionRoom.SystemPowerOn.OutputSig.SetSigFalseAction(
    essentialsHuddleVtc1Room.PowerOnToDefaultOrLastSource);
                FusionRoom.SystemPowerOff.OutputSig.SetSigFalseAction(
                    () => essentialsHuddleVtc1Room.RunRouteAction("roomOff", Room.SourceListKey));
            }

            CrestronEnvironment.EthernetEventHandler += CrestronEnvironment_EthernetEventHandler;
        }

        protected override void SetUpSources()
        {
            // Sources
            var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;

            if (essentialsHuddleVtc1Room == null)
            {
                return;
            }

            var dict = ConfigReader.ConfigObject.GetSourceListForKey(essentialsHuddleVtc1Room.SourceListKey);
            if (dict != null)
            {
                // NEW PROCESS:
                // Make these lists and insert the fusion attributes by iterating these
                var setTopBoxes = dict.Where(d => d.Value.SourceDevice is ISetTopBoxControls);
                uint i = 1;
                foreach (var kvp in setTopBoxes)
                {
                    TryAddRouteActionSigs("Display 1 - Source TV " + i, 188 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 5) // We only have five spots
                    {
                        break;
                    }
                }

                var discPlayers = dict.Where(d => d.Value.SourceDevice is IDiscPlayerControls);
                i = 1;
                foreach (var kvp in discPlayers)
                {
                    TryAddRouteActionSigs("Display 1 - Source DVD " + i, 181 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 5) // We only have five spots
                    {
                        break;
                    }
                }

                var laptops = dict.Where(d => d.Value.SourceDevice is Core.Devices.Laptop);
                i = 1;
                foreach (var kvp in laptops)
                {
                    TryAddRouteActionSigs("Display 1 - Source Laptop " + i, 166 + i, kvp.Key, kvp.Value.SourceDevice);
                    i++;
                    if (i > 10) // We only have ten spots???
                    {
                        break;
                    }
                }

                foreach (var usageDevice in dict.Select(kvp => kvp.Value.SourceDevice).OfType<IUsageTracking>())
                {
                    usageDevice.UsageTracker = new UsageTracking(usageDevice as Device) {UsageIsTracked = true};
                    usageDevice.UsageTracker.DeviceUsageEnded +=
                        UsageTracker_DeviceUsageEnded;
                }
            }
            else
            {
                Debug.Console(1, this, "WARNING: Config source list '{0}' not found for room '{1}'",
                    essentialsHuddleVtc1Room.SourceListKey, Room.Key);
            }
        }

        protected override void SetUpDisplay()
        {
            try
            {
                //Setup Display Usage Monitoring

                var displays = DeviceManager.AllDevices.Where(d => d is DisplayBase);

                //  Consider updating this in multiple display systems

                foreach (var display in displays.Cast<DisplayBase>())
                {
                    display.UsageTracker = new UsageTracking(display) {UsageIsTracked = true};
                    display.UsageTracker.DeviceUsageEnded +=
                        UsageTracker_DeviceUsageEnded;
                }

                var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;
                if (essentialsHuddleVtc1Room == null)
                {
                    return;
                }

                var defaultDisplay = essentialsHuddleVtc1Room.DefaultDisplay as DisplayBase;
                if (defaultDisplay == null)
                {
                    Debug.Console(1, this, "Cannot link null display to Fusion because default display is null");
                    return;
                }

                var dispPowerOnAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        defaultDisplay.PowerOn();
                    }
                });
                var dispPowerOffAction = new Action<bool>(b =>
                {
                    if (!b)
                    {
                        defaultDisplay.PowerOff();
                    }
                });

                // Display to fusion room sigs
                FusionRoom.DisplayPowerOn.OutputSig.UserObject = dispPowerOnAction;
                FusionRoom.DisplayPowerOff.OutputSig.UserObject = dispPowerOffAction;
                defaultDisplay.PowerIsOnFeedback.LinkInputSig(FusionRoom.DisplayPowerOn.InputSig);
                if (defaultDisplay is IDisplayUsage)
                {
                    (defaultDisplay as IDisplayUsage).LampHours.LinkInputSig(FusionRoom.DisplayUsage.InputSig);
                }


                MapDisplayToRoomJoins(1, 158, defaultDisplay);


                var deviceConfig =
                    ConfigReader.ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(defaultDisplay.Key));

                //Check for existing asset in GUIDs collection

                FusionAsset tempAsset;

                if (FusionStaticAssets.ContainsKey(deviceConfig.Uid))
                {
                    tempAsset = FusionStaticAssets[deviceConfig.Uid];
                }
                else
                {
                    // Create a new asset
                    tempAsset = new FusionAsset(FusionRoomGuids.GetNextAvailableAssetNumber(FusionRoom),
                        defaultDisplay.Name, "Display", "");
                    FusionStaticAssets.Add(deviceConfig.Uid, tempAsset);
                }

                var dispAsset = FusionRoom.CreateStaticAsset(tempAsset.SlotNumber, tempAsset.Name, "Display",
                    tempAsset.InstanceId);
                dispAsset.PowerOn.OutputSig.UserObject = dispPowerOnAction;
                dispAsset.PowerOff.OutputSig.UserObject = dispPowerOffAction;
                defaultDisplay.PowerIsOnFeedback.LinkInputSig(dispAsset.PowerOn.InputSig);
                // NO!! display.PowerIsOn.LinkComplementInputSig(dispAsset.PowerOff.InputSig);
                // Use extension methods
                dispAsset.TrySetMakeModel(defaultDisplay);
                dispAsset.TryLinkAssetErrorToCommunication(defaultDisplay);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error setting up display in Fusion: {0}", e);
            }
        }

        protected override void MapDisplayToRoomJoins(int displayIndex, int joinOffset, DisplayBase display)
        {
            var displayName = string.Format("Display {0} - ", displayIndex);


            var essentialsHuddleVtc1Room = Room as EssentialsHuddleVtc1Room;
            if (essentialsHuddleVtc1Room == null || display != essentialsHuddleVtc1Room.DefaultDisplay)
            {
                return;
            }
            // Power on
            var defaultDisplayPowerOn = FusionRoom.CreateOffsetBoolSig((uint) joinOffset, displayName + "Power On",
                eSigIoMask.InputOutputSig);
            defaultDisplayPowerOn.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (!b)
                {
                    display.PowerOn();
                }
            });
            display.PowerIsOnFeedback.LinkInputSig(defaultDisplayPowerOn.InputSig);

            // Power Off

            FusionRoom.CreateOffsetBoolSig((uint) joinOffset + 1,
                displayName + "Power Off", eSigIoMask.InputOutputSig);
            defaultDisplayPowerOn.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (!b)
                {
                    display.PowerOff();
                }
            });
            
            display.PowerIsOnFeedback.LinkInputSig(defaultDisplayPowerOn.InputSig);

            // Current Source
            var defaultDisplaySourceNone = FusionRoom.CreateOffsetBoolSig((uint) joinOffset + 8,
                displayName + "Source None", eSigIoMask.InputOutputSig);
            defaultDisplaySourceNone.OutputSig.UserObject = new Action<bool>(b =>
            {
                if (b)
                {
                    return;
                }
                var huddleVtc1Room = Room as EssentialsHuddleVtc1Room;
                if (huddleVtc1Room != null)
                {
                    huddleVtc1Room.RunRouteAction("roomOff", Room.SourceListKey);
                }
            });
        }
    }
}