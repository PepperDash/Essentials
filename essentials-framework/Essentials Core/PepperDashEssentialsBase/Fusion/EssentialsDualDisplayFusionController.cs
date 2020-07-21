using System;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.Fusion
{
    public class EssentialsDualDisplayFusionController:EssentialsHuddleVtc1FusionController
    {
        private readonly EssentialsDualDisplayRoom _room;

        public EssentialsDualDisplayFusionController(EssentialsDualDisplayRoom room, uint ipId) : base(room, ipId)
        {
            _room = room;
        }

        #region Overrides of EssentialsHuddleVtc1FusionController

        protected override void ExecuteCustomSteps()
        {
            SetUpDisplays();
            base.ExecuteCustomSteps();
        }

        #endregion

        private void SetUpDisplays()
        {
            if (_room == null) return;

            var leftDisplay = _room.LeftDisplay as DisplayBase;
            var rightDisplay = _room.RightDisplay as DisplayBase;

            SetUpDisplay(leftDisplay);
            SetUpDisplay(rightDisplay);
        }

        protected override void SetUpDefaultDisplay()
        {
            Debug.Console(0, this, "No default display for Dual Display Room");
        }

        #region Overrides of EssentialsFusionSystemControllerBase

        protected override void SetUpDefaultDisplayAsset()
        {
            Debug.Console(0, this, "No default display for Dual Display Room");
        }

        #endregion

        private void SetUpDisplay(DisplayBase display)
        {
            FusionAsset tempAsset;

            display.UsageTracker = new UsageTracking(display){UsageIsTracked = true};
            display.UsageTracker.DeviceUsageEnded += UsageTrackerOnDeviceUsageEnded;

            var config = ConfigReader.ConfigObject.Devices.SingleOrDefault(d => d.Key == display.Key);

            if (!FusionStaticAssets.TryGetValue(config.Uid, out tempAsset))
            {
                tempAsset = new FusionAsset(FusionRoomGuids.GetNextAvailableAssetNumber(FusionRoom), display.Name,
                    "Display", "");
                FusionStaticAssets.Add(config.Uid, tempAsset);
            }

            var displayAsset = FusionRoom.CreateStaticAsset(tempAsset.SlotNumber, tempAsset.Name, "Display",
                tempAsset.InstanceId);

            displayAsset.PowerOn.OutputSig.UserObject = new Action<bool>(b => { if (b) display.PowerOn(); });
            displayAsset.PowerOff.OutputSig.UserObject = new Action<bool>(b => { if (b) display.PowerOff(); });

            if (!(display is ICommunicationMonitor))
            {
                return;
            }

            var displayCommMonitor = display as ICommunicationMonitor;

            displayAsset.Connected.InputSig.BoolValue = displayCommMonitor.CommunicationMonitor.Status ==
                                                        MonitorStatus.IsOk;
            displayCommMonitor.CommunicationMonitor.StatusChange += (o, a) =>
            {
                displayAsset.Connected.InputSig.BoolValue = displayCommMonitor.CommunicationMonitor.Status ==
                                                            MonitorStatus.IsOk;
            };
        }

        private void UsageTrackerOnDeviceUsageEnded(object sender, DeviceUsageEventArgs deviceUsageEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}