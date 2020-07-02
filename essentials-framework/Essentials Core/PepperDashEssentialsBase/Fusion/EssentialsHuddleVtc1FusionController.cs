using System;
using System.Linq;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Fusion
{
    public class EssentialsHuddleVtc1FusionController : EssentialsHuddleSpaceFusionSystemControllerBase
    {
        private BooleanSigData _codecIsInCall;

        private readonly EssentialsHuddleVtc1Room _room;

        public EssentialsHuddleVtc1FusionController(EssentialsHuddleVtc1Room room, uint ipId)
            : base(room, ipId)
        {
            _room = room;
        }

        /// <summary>
        /// Called in base class constructor before RVI and GUID files are built
        /// </summary>
        protected override void ExecuteCustomSteps()
        {
            SetUpCodec();
            base.ExecuteCustomSteps();
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
                var codecHasIpInfo = false;
                var codecComm = codec.Communication;

                var codecIpAddress = string.Empty;
                var codecIpPort = 0;

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
                    var codecIpAddressSig = FusionRoom.CreateOffsetStringSig(121, "IP Address - VC",
                        eSigIoMask.InputSigOnly);
                    codecIpAddressSig.InputSig.StringValue = codecIpAddress;

                    var codecIpPortSig = FusionRoom.CreateOffsetStringSig(150, "IP Port - VC",
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

        #region Overrides of EssentialsHuddleSpaceFusionSystemControllerBase

        protected override void SetUpDisplay()
        {
            base.SetUpDisplay();

            var defaultDisplay = _room.DefaultDisplay as DisplayBase;

            if (defaultDisplay == null)
            {
                Debug.Console(1, this, "Cannot link null display to Fusion because default display is null");
                return;
            }

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

        #endregion

        private void codec_CallStatusChange(object sender, Devices.Codec.CodecCallStatusItemChangeEventArgs e)
        {
            var codec = _room.VideoCodec;

            _codecIsInCall.InputSig.BoolValue = codec.IsInCall;
        }
    }
}