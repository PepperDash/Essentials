using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
	[Description("Wrapper class for Dual Technology GLS Occupancy Sensors")]
    [ConfigSnippet("\"properties\": {\"control\": {\"method\": \"cresnet\",\"cresnetId\": \"97\"},\"enablePir\": true,\"enableLedFlash\": true,\"enableRawStates\":true,\"remoteTimeout\": 30,\"internalPhotoSensorMinChange\": 0,\"externalPhotoSensorMinChange\": 0,\"enableUsA\": true,\"enableUsB\": true,\"orWhenVacatedState\": true}")]
    public class GlsOdtOccupancySensorController : GlsOccupancySensorBaseController
	{
	    private GlsOdtCCn _occSensor;

		public BoolFeedback OrWhenVacatedFeedback { get; private set; }

		public BoolFeedback AndWhenVacatedFeedback { get; private set; }

		public BoolFeedback UltrasonicAEnabledFeedback { get; private set; }

		public BoolFeedback UltrasonicBEnabledFeedback { get; private set; }

		public IntFeedback UltrasonicSensitivityInVacantStateFeedback { get; private set; }

		public IntFeedback UltrasonicSensitivityInOccupiedStateFeedback { get; private set; }

		public BoolFeedback RawOccupancyPirFeedback { get; private set; }

		public BoolFeedback RawOccupancyUsFeedback { get; private set; }


		public GlsOdtOccupancySensorController(string key, Func<DeviceConfig, GlsOdtCCn> preActivationFunc,
			DeviceConfig config)
			: base(key, config.Name, config)
		{
			AddPreActivationAction(() =>
			{
				_occSensor = preActivationFunc(config);

				RegisterCrestronGenericBase(_occSensor);

				RegisterGlsOccupancySensorBaseController(_occSensor);

				AndWhenVacatedFeedback = new BoolFeedback(() => _occSensor.AndWhenVacatedFeedback.BoolValue);

				OrWhenVacatedFeedback = new BoolFeedback(() => _occSensor.OrWhenVacatedFeedback.BoolValue);

				UltrasonicAEnabledFeedback = new BoolFeedback(() => _occSensor.UsAEnabledFeedback.BoolValue);

				UltrasonicBEnabledFeedback = new BoolFeedback(() => _occSensor.UsBEnabledFeedback.BoolValue);

				RawOccupancyPirFeedback = new BoolFeedback(() => _occSensor.RawOccupancyPirFeedback.BoolValue);

				RawOccupancyUsFeedback = new BoolFeedback(() => _occSensor.RawOccupancyUsFeedback.BoolValue);

				UltrasonicSensitivityInVacantStateFeedback = new IntFeedback(() => _occSensor.UsSensitivityInVacantStateFeedback.UShortValue);

				UltrasonicSensitivityInOccupiedStateFeedback = new IntFeedback(() => _occSensor.UsSensitivityInOccupiedStateFeedback.UShortValue);

			});	
		}

        protected override void ApplySettingsToSensorFromConfig()
        {
            base.ApplySettingsToSensorFromConfig();

            if (PropertiesConfig.EnableUsA != null)
            {
                Debug.Console(1, this, "EnableUsA found, attempting to set value from config");
                SetUsAEnable((bool)PropertiesConfig.EnableUsA);   
            }
            else
            {
                Debug.Console(1, this, "EnableUsA null, no value specified in config");
            }


            if (PropertiesConfig.EnableUsB != null)
            {
                Debug.Console(1, this, "EnableUsB found, attempting to set value from config");
                SetUsBEnable((bool)PropertiesConfig.EnableUsB);
            }
            else
            {
                Debug.Console(1, this, "EnablePir null, no value specified in config");
            }


            if (PropertiesConfig.OrWhenVacatedState != null)
            {
                SetOrWhenVacatedState((bool)PropertiesConfig.OrWhenVacatedState);
            }

            if (PropertiesConfig.AndWhenVacatedState != null)
            {
                SetAndWhenVacatedState((bool)PropertiesConfig.AndWhenVacatedState);
            }
        }

		/// <summary>
		/// Overrides the base class event delegate to fire feedbacks for event IDs that pertain to this extended class.
		/// Then calls the base delegate method to ensure any common event IDs are captured.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void OccSensor_GlsOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
		{
		    switch (args.EventId)
		    {
		        case GlsOccupancySensorBase.AndWhenVacatedFeedbackEventId:
		            AndWhenVacatedFeedback.FireUpdate();
		            break;
		        case GlsOccupancySensorBase.OrWhenVacatedFeedbackEventId:
		            OrWhenVacatedFeedback.FireUpdate();
		            break;
		        case GlsOccupancySensorBase.UsAEnabledFeedbackEventId:
		            UltrasonicAEnabledFeedback.FireUpdate();
		            break;
		        case GlsOccupancySensorBase.UsBEnabledFeedbackEventId:
		            UltrasonicBEnabledFeedback.FireUpdate();
		            break;
		        case GlsOccupancySensorBase.UsSensitivityInOccupiedStateFeedbackEventId:
		            UltrasonicSensitivityInOccupiedStateFeedback.FireUpdate();
		            break;
		        case GlsOccupancySensorBase.UsSensitivityInVacantStateFeedbackEventId:
		            UltrasonicSensitivityInVacantStateFeedback.FireUpdate();
		            break;
		    }

		    base.OccSensor_GlsOccupancySensorChange(device, args);
		}

	    /// <summary>
		/// Overrides the base class event delegate to fire feedbacks for event IDs that pertain to this extended class.
		/// Then calls the base delegate method to ensure any common event IDs are captured.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void OccSensor_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
	    {
	        switch (args.EventId)
	        {
	            case GlsOccupancySensorBase.RawOccupancyPirFeedbackEventId:
	                RawOccupancyPirFeedback.FireUpdate();
	                break;
	            case GlsOccupancySensorBase.RawOccupancyUsFeedbackEventId:
	                RawOccupancyUsFeedback.FireUpdate();
	                break;
	        }

	        base.OccSensor_BaseEvent(device, args);
	    }

	    /// <summary>
		/// Sets the OrWhenVacated state
		/// </summary>
		/// <param name="state"></param>
		public void SetOrWhenVacatedState(bool state)
		{
			_occSensor.OrWhenVacated.BoolValue = state;
		}

		/// <summary>
		/// Sets the AndWhenVacated state
		/// </summary>
		/// <param name="state"></param>
		public void SetAndWhenVacatedState(bool state)
		{
			_occSensor.AndWhenVacated.BoolValue = state;
		}

		/// <summary>
		/// Enables or disables the Ultrasonic A sensor
		/// </summary>
		/// <param name="state"></param>
		public void SetUsAEnable(bool state)
		{
			_occSensor.EnableUsA.BoolValue = state;
			_occSensor.DisableUsA.BoolValue = !state;
		}


		/// <summary>
		/// Enables or disables the Ultrasonic B sensor
		/// </summary>
		/// <param name="state"></param>
		public void SetUsBEnable(bool state)
		{
			_occSensor.EnableUsB.BoolValue = state;
			_occSensor.DisableUsB.BoolValue = !state;
		}

		public void IncrementUsSensitivityInOccupiedState(bool pressRelease)
		{
			_occSensor.IncrementUsSensitivityInOccupiedState.BoolValue = pressRelease;
		}

		public void DecrementUsSensitivityInOccupiedState(bool pressRelease)
		{
			_occSensor.DecrementUsSensitivityInOccupiedState.BoolValue = pressRelease;
		}

		public void IncrementUsSensitivityInVacantState(bool pressRelease)
		{
			_occSensor.IncrementUsSensitivityInVacantState.BoolValue = pressRelease;
		}

		public void DecrementUsSensitivityInVacantState(bool pressRelease)
		{
			_occSensor.DecrementUsSensitivityInVacantState.BoolValue = pressRelease;
		}

		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			LinkOccSensorToApi(this, trilist, joinStart, joinMapKey, bridge);
		}

		/// <summary>
		/// Method to print occ sensor settings to console.
		/// </summary>
		public override void GetSettings()
		{
            base.GetSettings();

			Debug.Console(0, this, "Ultrasonic Enabled A: {0} | B: {1}",
				_occSensor.UsAEnabledFeedback.BoolValue,
				_occSensor.UsBEnabledFeedback.BoolValue);

			Debug.Console(0, this, "Ultrasonic Sensitivity Occupied: {0} | Vacant: {1}",
				_occSensor.UsSensitivityInOccupiedStateFeedback.UShortValue,
				_occSensor.UsSensitivityInVacantStateFeedback.UShortValue);

            var dash = new string('*', 50);
			CrestronConsole.PrintLine(string.Format("{0}\n", dash));
		}

	}

    public class GlsOdtOccupancySensorControllerFactory : EssentialsDeviceFactory<GlsOdtOccupancySensorController>
    {
        public GlsOdtOccupancySensorControllerFactory()
        {
            TypeNames = new List<string> { "glsodtccn" };
        }


        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new GlsOccupancySensorBaseController Device");

            return new GlsOdtOccupancySensorController(dc.Key, GetGlsOdtCCn, dc);
        }

        private static GlsOdtCCn GetGlsOdtCCn(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = String.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsOdtCCn", parentKey);
                return new GlsOdtCCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
    }



}