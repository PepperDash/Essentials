using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsTechRoom:EssentialsRoomBase
    {
        private readonly Dictionary<string, IRSetTopBoxBase> _tuners;
        private readonly Dictionary<string, TwoWayDisplayBase> _displays;

        private readonly DevicePresetsModel _tunerPresets;

        private readonly EssentialsTechRoomConfig _config;

        public DevicePresetsModel TunerPresets
        {
            get
            {
                return _tunerPresets;
            }
        }

        public Dictionary<string, IRSetTopBoxBase> Tuners
        {
            get { return _tuners; }
        }

        public Dictionary<string, TwoWayDisplayBase> Displays
        {
            get { return _displays; }
        }

        public BoolFeedback RoomPowerIsOnFeedback { get; private set; }

        public bool RoomPowerIsOn
        {
            get { return _displays.All(kv => kv.Value.PowerIsOnFeedback.BoolValue); }
        }

        public EssentialsTechRoom(DeviceConfig config) : base(config)
        {
            _config = config.Properties.ToObject<EssentialsTechRoomConfig>();

            _tunerPresets = new DevicePresetsModel(String.Format("{0}-presets", config.Key), _config.PresetsFileName);

            _tunerPresets.LoadChannels();

            _tuners = GetDevices<IRSetTopBoxBase>(_config.Tuners);
            _displays = GetDevices<TwoWayDisplayBase>(_config.Displays);

            RoomPowerIsOnFeedback = new BoolFeedback(() => RoomPowerIsOn);
        }

        private void SubscribeToDisplayFeedbacks()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerIsOnFeedback.OutputChange += (sender, args) => RoomPowerIsOnFeedback.InvokeFireUpdate();
            }
        }

        public void RoomPowerOn()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerOn();
            }
        }

        public void RoomPowerOff()
        {
            foreach (var display in _displays)
            {
                display.Value.PowerOff();
            }
        }

        private Dictionary<string, T> GetDevices<T>(ICollection<string> config) where T:IKeyed
        {
            try
            {
                var returnValue = DeviceManager.AllDevices.OfType<T>()
                    .Where(d => config.Contains(d.Key))
                    .ToDictionary(d => d.Key, d => d);

                return returnValue;
            }
            catch
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Error getting devices. Check Essentials Configuration");
                return null;
            }
        }

        

        #region Overrides of EssentialsRoomBase

        protected override Func<bool> IsWarmingFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override Func<bool> IsCoolingFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override Func<bool> OnFeedbackFunc
        {
            get { throw new NotImplementedException(); }
        }

        protected override void EndShutdown()
        {
            throw new NotImplementedException();
        }

        public override void SetDefaultLevels()
        {
            throw new NotImplementedException();
        }

        public override void PowerOnToDefaultOrLastSource()
        {
            throw new NotImplementedException();
        }

        public override bool RunDefaultPresentRoute()
        {
            throw new NotImplementedException();
        }

        public override void RoomVacatedForTimeoutPeriod(object o)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}