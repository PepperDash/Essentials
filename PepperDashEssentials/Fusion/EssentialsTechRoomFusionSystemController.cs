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

                Debug.Console(1, this, "Setting up Static Assets for {0} Displays", displays.Count);

                foreach (var display in displays.Values.Cast<DisplayBase>())
                {
                    var disp = display; // Local scope variable

                    Debug.Console(2, this, "Setting up Static Asset for {0}", disp.Key);

                    disp.UsageTracker = new UsageTracking(disp) { UsageIsTracked = true };
                    disp.UsageTracker.DeviceUsageEnded += UsageTracker_DeviceUsageEnded;

                    var dispPowerOnAction = new Action<bool>(b =>
                    {
                        if (!b)
                        {
                            disp.PowerOn();
                        }
                    });
                    var dispPowerOffAction = new Action<bool>(b =>
                    {
                        if (!b)
                        {
                            disp.PowerOff();
                        }
                    });

                    var deviceConfig = ConfigReader.ConfigObject.GetDeviceForKey(disp.Key);

                    FusionAsset tempAsset;

                    if (FusionStaticAssets.ContainsKey(deviceConfig.Uid))
                    {
                        // Used existing asset
                        tempAsset = FusionStaticAssets[deviceConfig.Uid];
                    }
                    else
                    {
                        // Create a new asset
                        tempAsset = new FusionAsset(FusionRoomGuids.GetNextAvailableAssetNumber(FusionRoom),
                            disp.Name, "Display", "");
                        FusionStaticAssets.Add(deviceConfig.Uid, tempAsset);
                    }

                    var dispAsset = FusionRoom.CreateStaticAsset(tempAsset.SlotNumber, tempAsset.Name, "Display",
                        tempAsset.InstanceId);

                    if (dispAsset != null)
                    {
                        dispAsset.PowerOn.OutputSig.UserObject = dispPowerOnAction;
                        dispAsset.PowerOff.OutputSig.UserObject = dispPowerOffAction;

                        // Use extension methods
                        dispAsset.TrySetMakeModel(disp);
                        dispAsset.TryLinkAssetErrorToCommunication(disp);
                    }

                    var defaultTwoWayDisplay = disp as IHasPowerControlWithFeedback;
                    if (defaultTwoWayDisplay != null)
                    {
                        defaultTwoWayDisplay.PowerIsOnFeedback.LinkInputSig(FusionRoom.DisplayPowerOn.InputSig);
                        if (disp is IDisplayUsage)
                        {
                            (disp as IDisplayUsage).LampHours.LinkInputSig(FusionRoom.DisplayUsage.InputSig);
                        }

                        if(dispAsset != null)
                            defaultTwoWayDisplay.PowerIsOnFeedback.LinkInputSig(dispAsset.PowerOn.InputSig);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error setting up displays in Fusion: {0}", e);
            }
        }
    }
}