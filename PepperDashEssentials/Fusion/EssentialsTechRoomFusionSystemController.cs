using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Fusion;

namespace PepperDash.Essentials.Fusion
{
    public class EssentialsTechRoomFusionSystemController : EssentialsHuddleSpaceFusionSystemControllerBase
    {
        public EssentialsTechRoomFusionSystemController(EssentialsTechRoom room, uint ipId, string joinMapKey)
            : base(room, ipId, joinMapKey)
        {

        }

        protected override void SetUpDisplay()
        {
            try
            {
                var displays = (Room as EssentialsTechRoom).Displays;

                foreach (var display in displays.Cast<DisplayBase>())
                {
                    display.UsageTracker = new UsageTracking(display) { UsageIsTracked = true };
                    display.UsageTracker.DeviceUsageEnded += UsageTracker_DeviceUsageEnded;

                    var dispPowerOnAction = new Action<bool>(b =>
                    {
                        if (!b)
                        {
                            display.PowerOn();
                        }
                    });
                    var dispPowerOffAction = new Action<bool>(b =>
                    {
                        if (!b)
                        {
                            display.PowerOff();
                        }
                    });

                    var deviceConfig = ConfigReader.ConfigObject.GetDeviceForKey(display.Key);

                    FusionAsset tempAsset;

                    if (FusionStaticAssets.ContainsKey(deviceConfig.Uid))
                    {
                        tempAsset = FusionStaticAssets[deviceConfig.Uid];
                    }
                    else
                    {
                        // Create a new asset
                        tempAsset = new FusionAsset(FusionRoomGuids.GetNextAvailableAssetNumber(FusionRoom),
                            display.Name, "Display", "");
                        FusionStaticAssets.Add(deviceConfig.Uid, tempAsset);
                    }

                    var dispAsset = FusionRoom.CreateStaticAsset(tempAsset.SlotNumber, tempAsset.Name, "Display",
                        tempAsset.InstanceId);
                    dispAsset.PowerOn.OutputSig.UserObject = dispPowerOnAction;
                    dispAsset.PowerOff.OutputSig.UserObject = dispPowerOffAction;

                    var defaultTwoWayDisplay = display as IHasPowerControlWithFeedback;
                    if (defaultTwoWayDisplay != null)
                    {
                        defaultTwoWayDisplay.PowerIsOnFeedback.LinkInputSig(FusionRoom.DisplayPowerOn.InputSig);
                        if (display is IDisplayUsage)
                        {
                            (display as IDisplayUsage).LampHours.LinkInputSig(FusionRoom.DisplayUsage.InputSig);
                        }

                        defaultTwoWayDisplay.PowerIsOnFeedback.LinkInputSig(dispAsset.PowerOn.InputSig);
                    }

                    // Use extension methods
                    dispAsset.TrySetMakeModel(display);
                    dispAsset.TryLinkAssetErrorToCommunication(display);
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error setting up displays in Fusion: {0}", e);
            }
        }
    }
}