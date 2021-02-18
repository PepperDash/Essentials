using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
	[Description("Wrapper class for Single Technology GLS Occupancy Sensors")]
    [ConfigSnippet("\"properties\": {\"control\": {\"method\": \"cresnet\",\"cresnetId\": \"97\"},\"enablePir\": true,\"enableLedFlash\": true,\"enableRawStates\":true,\"remoteTimeout\": 30,\"internalPhotoSensorMinChange\": 0,\"externalPhotoSensorMinChange\": 0}")]
	public abstract class GlsOccupancySensorBaseController : CrestronGenericBridgeableBaseDevice, IOccupancyStatusProvider
	{
        public GlsOccupancySensorPropertiesConfig PropertiesConfig { get; private set; }

	    protected GlsOccupancySensorBase OccSensor;

		public BoolFeedback RoomIsOccupiedFeedback { get; private set; }

		public BoolFeedback GraceOccupancyDetectedFeedback { get; private set; }

		public BoolFeedback RawOccupancyFeedback { get; private set; }

		public BoolFeedback PirSensorEnabledFeedback { get; private set; }

		public BoolFeedback LedFlashEnabledFeedback { get; private set; }

		public BoolFeedback ShortTimeoutEnabledFeedback { get; private set; }

		public IntFeedback PirSensitivityInVacantStateFeedback { get; private set; }

		public IntFeedback PirSensitivityInOccupiedStateFeedback { get; private set; }

		public IntFeedback CurrentTimeoutFeedback { get; private set; }

		public IntFeedback LocalTimoutFeedback { get; private set; }

		public IntFeedback InternalPhotoSensorValue { get; set; }

		public IntFeedback ExternalPhotoSensorValue { get; set; }

		// Debug properties
		public bool InTestMode { get; private set; }

		public bool TestRoomIsOccupiedFeedback { get; private set; }

		public Func<bool> RoomIsOccupiedFeedbackFunc
		{
			get
			{
				return () => InTestMode ? TestRoomIsOccupiedFeedback : OccSensor.OccupancyDetectedFeedback.BoolValue;
			}
		}

	    protected GlsOccupancySensorBaseController(string key, DeviceConfig config)
			: this(key, config.Name, config)
		{
		}

	    protected GlsOccupancySensorBaseController(string key, string name, DeviceConfig config)
            : base(key, name) 
        {

            var props = config.Properties.ToObject<GlsOccupancySensorPropertiesConfig>();

            if (props != null)
            {
                PropertiesConfig = props;
            }
            else
            {
                Debug.Console(1, this, "props are null.  Unable to deserialize into GlsOccupancySensorPropertiesConfig");
            }

            AddPostActivationAction(() =>
                {
                    OccSensor.OnlineStatusChange += (o, a) =>
                    {
                        if (a.DeviceOnLine)
                        {
                            ApplySettingsToSensorFromConfig();
                        }
                    };

                    if (OccSensor.IsOnline)
                    {
                        ApplySettingsToSensorFromConfig();

                    }
                });
        }


        /// <summary>
        /// Applies any sensor settings defined in config 
        /// </summary>
        protected virtual void ApplySettingsToSensorFromConfig()
        {
            Debug.Console(1, this, "Attempting to apply settings to sensor from config");

            if (PropertiesConfig.EnablePir != null)
            {
                Debug.Console(1, this, "EnablePir found, attempting to set value from config");
                SetPirEnable((bool)PropertiesConfig.EnablePir);
            }
            else
            {
                Debug.Console(1, this, "EnablePir null, no value specified in config");
            }

            if (PropertiesConfig.EnableLedFlash != null)
            {
                Debug.Console(1, this, "EnableLedFlash found, attempting to set value from config");
                SetLedFlashEnable((bool)PropertiesConfig.EnableLedFlash);
            }

            if (PropertiesConfig.RemoteTimeout != null)
            {
                Debug.Console(1, this, "RemoteTimeout found, attempting to set value from config");
                SetRemoteTimeout((ushort)PropertiesConfig.RemoteTimeout);
            }
            else
            {
                Debug.Console(1, this, "RemoteTimeout null, no value specified in config");
            }

            if (PropertiesConfig.ShortTimeoutState != null)
            {
                SetShortTimeoutState((bool)PropertiesConfig.ShortTimeoutState);
            }

            if (PropertiesConfig.EnableRawStates != null)
            {
                EnableRawStates((bool)PropertiesConfig.EnableRawStates);
            }

            if (PropertiesConfig.InternalPhotoSensorMinChange != null)
            {
                SetInternalPhotoSensorMinChange((ushort)PropertiesConfig.InternalPhotoSensorMinChange);
            }

            if (PropertiesConfig.ExternalPhotoSensorMinChange != null)
            {
                SetExternalPhotoSensorMinChange((ushort)PropertiesConfig.ExternalPhotoSensorMinChange);
            }
        }

		protected void RegisterGlsOccupancySensorBaseController(GlsOccupancySensorBase occSensor)
		{
			OccSensor = occSensor;

			RoomIsOccupiedFeedback = new BoolFeedback(RoomIsOccupiedFeedbackFunc);

			PirSensorEnabledFeedback = new BoolFeedback(() => OccSensor.PirEnabledFeedback.BoolValue);

			LedFlashEnabledFeedback = new BoolFeedback(() => OccSensor.LedFlashEnabledFeedback.BoolValue);

			ShortTimeoutEnabledFeedback = new BoolFeedback(() => OccSensor.ShortTimeoutEnabledFeedback.BoolValue);

			PirSensitivityInVacantStateFeedback =
				new IntFeedback(() => OccSensor.PirSensitivityInVacantStateFeedback.UShortValue);

			PirSensitivityInOccupiedStateFeedback =
				new IntFeedback(() => OccSensor.PirSensitivityInOccupiedStateFeedback.UShortValue);

			CurrentTimeoutFeedback = new IntFeedback(() => OccSensor.CurrentTimeoutFeedback.UShortValue);

			LocalTimoutFeedback = new IntFeedback(() => OccSensor.LocalTimeoutFeedback.UShortValue);

			GraceOccupancyDetectedFeedback =
				new BoolFeedback(() => OccSensor.GraceOccupancyDetectedFeedback.BoolValue);

			RawOccupancyFeedback = new BoolFeedback(() => OccSensor.RawOccupancyFeedback.BoolValue);

			InternalPhotoSensorValue = new IntFeedback(() => OccSensor.InternalPhotoSensorValueFeedback.UShortValue);

			ExternalPhotoSensorValue = new IntFeedback(() => OccSensor.ExternalPhotoSensorValueFeedback.UShortValue);

			OccSensor.BaseEvent += OccSensor_BaseEvent;

			OccSensor.GlsOccupancySensorChange += OccSensor_GlsOccupancySensorChange;
		}


		/// <summary>
		/// Catches events for feedbacks on the base class.  Any extending wrapper class should call this delegate after it checks for it's own event IDs.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void OccSensor_GlsOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
		{
			switch (args.EventId)
			{
				case GlsOccupancySensorBase.PirEnabledFeedbackEventId:
					PirSensorEnabledFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.LedFlashEnabledFeedbackEventId:
					LedFlashEnabledFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.ShortTimeoutEnabledFeedbackEventId:
					ShortTimeoutEnabledFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.PirSensitivityInOccupiedStateFeedbackEventId:
					PirSensitivityInOccupiedStateFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.PirSensitivityInVacantStateFeedbackEventId:
					PirSensitivityInVacantStateFeedback.FireUpdate();
					break;
			}
		}

		protected virtual void OccSensor_BaseEvent(Crestron.SimplSharpPro.GenericBase device, Crestron.SimplSharpPro.BaseEventArgs args)
		{
			Debug.Console(2, this, "GlsOccupancySensorChange  EventId: {0}", args.EventId);

			switch (args.EventId)
			{
				case GlsOccupancySensorBase.RoomVacantFeedbackEventId:
				case GlsOccupancySensorBase.RoomOccupiedFeedbackEventId:
					Debug.Console(1, this, "Occupancy State: {0}", OccSensor.OccupancyDetectedFeedback.BoolValue);
					RoomIsOccupiedFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.TimeoutFeedbackEventId:
					CurrentTimeoutFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.TimeoutLocalFeedbackEventId:
					LocalTimoutFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.GraceOccupancyDetectedFeedbackEventId:
					GraceOccupancyDetectedFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.RawOccupancyFeedbackEventId:
					RawOccupancyFeedback.FireUpdate();
					break;
				case GlsOccupancySensorBase.InternalPhotoSensorValueFeedbackEventId:
					InternalPhotoSensorValue.FireUpdate();
					break;
				case GlsOccupancySensorBase.ExternalPhotoSensorValueFeedbackEventId:
					ExternalPhotoSensorValue.FireUpdate();
					break;
			}
		}

		public void SetTestMode(bool mode)
		{
			InTestMode = mode;

			Debug.Console(1, this, "In Mock Mode: '{0}'", InTestMode);
		}

		public void SetTestOccupiedState(bool state)
		{
			if (!InTestMode)
				Debug.Console(1, "Mock mode not enabled");
			else
			{
				TestRoomIsOccupiedFeedback = state;

				RoomIsOccupiedFeedback.FireUpdate();
			}
		}

	    /// <summary>
	    /// Enables or disables the PIR sensor
	    /// </summary>
	    /// <param name="state"></param>
	    public void SetPirEnable(bool state)
	    {
	        Debug.Console(1, this, "Setting EnablePir to: {0}", state);

	        OccSensor.EnablePir.BoolValue = state;
	        OccSensor.DisablePir.BoolValue = !state;
	    }

	    /// <summary>
	    /// Enables or disables the LED Flash
	    /// </summary>
	    /// <param name="state"></param>
	    public void SetLedFlashEnable(bool state)
	    {
	        OccSensor.EnableLedFlash.BoolValue = state;
	        OccSensor.DisableLedFlash.BoolValue = !state;
	    }

	    /// <summary>
	    /// Enables or disables short timeout based on state
	    /// </summary>
	    /// <param name="state"></param>
	    public void SetShortTimeoutState(bool state)
	    {
	        OccSensor.EnableShortTimeout.BoolValue = state;
	        OccSensor.DisableShortTimeout.BoolValue = !state;
	    }

	    public void IncrementPirSensitivityInOccupiedState(bool pressRelease)
		{
			OccSensor.IncrementPirSensitivityInOccupiedState.BoolValue = pressRelease;
		}

		public void DecrementPirSensitivityInOccupiedState(bool pressRelease)
		{
			OccSensor.DecrementPirSensitivityInOccupiedState.BoolValue = pressRelease;
		}

		public void IncrementPirSensitivityInVacantState(bool pressRelease)
		{
			OccSensor.IncrementPirSensitivityInVacantState.BoolValue = pressRelease;
		}

		public void DecrementPirSensitivityInVacantState(bool pressRelease)
		{
			OccSensor.DecrementPirSensitivityInVacantState.BoolValue = pressRelease;
		}

        /// <summary>
        /// Pulse ForceOccupied on the sensor for .5 seconds
        /// </summary>
	    public void ForceOccupied()
	    {
	        CrestronInvoke.BeginInvoke((o) =>
	        {
	            ForceOccupied(true);
	            CrestronEnvironment.Sleep(500);
	            ForceOccupied(false);
	        });
	    }

		public void ForceOccupied(bool value)
		{
			OccSensor.ForceOccupied.BoolValue = value;
		}

        /// <summary>
        /// Pulse ForceVacant on the sensor for .5 seconds
        /// </summary>
	    public void ForceVacant()
	    {
            CrestronInvoke.BeginInvoke((o) =>
            {
                ForceVacant(true);
                CrestronEnvironment.Sleep(500);
                ForceVacant(false);
            });
	    }

		public void ForceVacant(bool value)
		{
			OccSensor.ForceVacant.BoolValue = value;
		}

		public void EnableRawStates(bool state)
		{
			OccSensor.EnableRawStates.BoolValue = state;
		}

		public void SetRemoteTimeout(ushort time)
		{
            Debug.Console(1, this, "Setting RemoteTimout to: {0}", time);

			OccSensor.RemoteTimeout.UShortValue = time;
		}

		public void SetInternalPhotoSensorMinChange(ushort value)
		{
			OccSensor.InternalPhotoSensorMinimumChange.UShortValue = value;
		}

		public void SetExternalPhotoSensorMinChange(ushort value)
		{
			OccSensor.ExternalPhotoSensorMinimumChange.UShortValue = value;
		}

	    /// <summary>
	    /// Method to print current occ settings to console.
	    /// </summary>
	    public virtual void GetSettings()
		{
			var dash = new string('*', 50);
			CrestronConsole.PrintLine(string.Format("{0}\n", dash));

            Debug.Console(0, this, "Vacancy Detected: {0}",
                OccSensor.VacancyDetectedFeedback.BoolValue);

			Debug.Console(0, this, "Timeout Current: {0} | Local: {1}",
				OccSensor.CurrentTimeoutFeedback.UShortValue,
				OccSensor.LocalTimeoutFeedback.UShortValue);

			Debug.Console(0, this, "Short Timeout Enabled: {0}",
				OccSensor.ShortTimeoutEnabledFeedback.BoolValue);

			Debug.Console(0, this, "PIR Sensor Enabled: {0} | Sensitivity Occupied: {1} | Sensitivity Vacant: {2}",
				OccSensor.PirEnabledFeedback.BoolValue,
				OccSensor.PirSensitivityInOccupiedStateFeedback.UShortValue,
				OccSensor.PirSensitivityInVacantStateFeedback.UShortValue);

			CrestronConsole.PrintLine(string.Format("{0}\n", dash));
		}

		protected void LinkOccSensorToApi(GlsOccupancySensorBaseController occController, BasicTriList trilist,
			uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new GlsOccupancySensorBaseJoinMap(joinStart);

			var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<GlsOccupancySensorBaseJoinMap>(joinMapSerialized);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}
			else
			{
				Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
			}

			Debug.Console(1, occController, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

			occController.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			trilist.StringInput[joinMap.Name.JoinNumber].StringValue = occController.Name;

			trilist.OnlineStatusChange += (d, args) =>
			{
				if (args.DeviceOnLine)
				{
					trilist.StringInput[joinMap.Name.JoinNumber].StringValue = occController.Name;
				}
			};

			LinkSingleTechSensorToApi(occController, trilist, joinMap);

			LinkDualTechSensorToApi(occController, trilist, joinMap);
		}

	    private static void LinkDualTechSensorToApi(GlsOccupancySensorBaseController occController, BasicTriList trilist,
	        GlsOccupancySensorBaseJoinMap joinMap)
	    {
	        var odtOccController = occController as GlsOdtOccupancySensorController;

	        if (odtOccController == null)
	        {
	            return;
	        }
	        // OR When Vacated
	        trilist.SetBoolSigAction(joinMap.OrWhenVacated.JoinNumber, odtOccController.SetOrWhenVacatedState);
	        odtOccController.OrWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OrWhenVacated.JoinNumber]);

	        // AND When Vacated
	        trilist.SetBoolSigAction(joinMap.AndWhenVacated.JoinNumber, odtOccController.SetAndWhenVacatedState);
	        odtOccController.AndWhenVacatedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AndWhenVacated.JoinNumber]);

	        // Ultrasonic A Sensor
	        trilist.SetSigTrueAction(joinMap.EnableUsA.JoinNumber, () => odtOccController.SetUsAEnable(true));
	        trilist.SetSigTrueAction(joinMap.DisableUsA.JoinNumber, () => odtOccController.SetUsAEnable(false));
	        odtOccController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsA.JoinNumber]);

	        // Ultrasonic B Sensor
	        trilist.SetSigTrueAction(joinMap.EnableUsB.JoinNumber, () => odtOccController.SetUsBEnable(true));
	        trilist.SetSigTrueAction(joinMap.DisableUsB.JoinNumber, () => odtOccController.SetUsBEnable(false));
	        odtOccController.UltrasonicAEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableUsB.JoinNumber]);

	        // US Sensitivity in Occupied State
	        trilist.SetBoolSigAction(joinMap.IncrementUsInOccupiedState.JoinNumber,
	            odtOccController.IncrementUsSensitivityInOccupiedState);
	        trilist.SetBoolSigAction(joinMap.DecrementUsInOccupiedState.JoinNumber,
	            odtOccController.DecrementUsSensitivityInOccupiedState);
	        odtOccController.UltrasonicSensitivityInOccupiedStateFeedback.LinkInputSig(
	            trilist.UShortInput[joinMap.UsSensitivityInOccupiedState.JoinNumber]);

	        // US Sensitivity in Vacant State
	        trilist.SetBoolSigAction(joinMap.IncrementUsInVacantState.JoinNumber,
	            odtOccController.IncrementUsSensitivityInVacantState);
	        trilist.SetBoolSigAction(joinMap.DecrementUsInVacantState.JoinNumber,
	            odtOccController.DecrementUsSensitivityInVacantState);
	        odtOccController.UltrasonicSensitivityInVacantStateFeedback.LinkInputSig(
	            trilist.UShortInput[joinMap.UsSensitivityInVacantState.JoinNumber]);

	        //Sensor Raw States
	        odtOccController.RawOccupancyPirFeedback.LinkInputSig(
	            trilist.BooleanInput[joinMap.RawOccupancyPirFeedback.JoinNumber]);
	        odtOccController.RawOccupancyUsFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyUsFeedback.JoinNumber]);
	    }

	    private static void LinkSingleTechSensorToApi(GlsOccupancySensorBaseController occController, BasicTriList trilist,
	        GlsOccupancySensorBaseJoinMap joinMap)
	    {
// Occupied status
	        trilist.SetBoolSigAction(joinMap.ForceOccupied.JoinNumber, occController.ForceOccupied);
	        trilist.SetBoolSigAction(joinMap.ForceVacant.JoinNumber, occController.ForceVacant);
	        occController.RoomIsOccupiedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RoomOccupiedFeedback.JoinNumber]);
	        occController.RoomIsOccupiedFeedback.LinkComplementInputSig(
	            trilist.BooleanInput[joinMap.RoomVacantFeedback.JoinNumber]);
	        occController.RawOccupancyFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RawOccupancyFeedback.JoinNumber]);
	        trilist.SetBoolSigAction(joinMap.EnableRawStates.JoinNumber, occController.EnableRawStates);

	        // Timouts
	        trilist.SetUShortSigAction(joinMap.Timeout.JoinNumber, occController.SetRemoteTimeout);
	        occController.CurrentTimeoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.Timeout.JoinNumber]);
	        occController.LocalTimoutFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeoutLocalFeedback.JoinNumber]);

	        // LED Flash
	        trilist.SetSigTrueAction(joinMap.EnableLedFlash.JoinNumber, () => occController.SetLedFlashEnable(true));
	        trilist.SetSigTrueAction(joinMap.DisableLedFlash.JoinNumber, () => occController.SetLedFlashEnable(false));
	        occController.LedFlashEnabledFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.EnableLedFlash.JoinNumber]);

	        // Short Timeout
	        trilist.SetSigTrueAction(joinMap.EnableShortTimeout.JoinNumber, () => occController.SetShortTimeoutState(true));
	        trilist.SetSigTrueAction(joinMap.DisableShortTimeout.JoinNumber, () => occController.SetShortTimeoutState(false));
	        occController.ShortTimeoutEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableShortTimeout.JoinNumber]);

	        // PIR Sensor
	        trilist.SetSigTrueAction(joinMap.EnablePir.JoinNumber, () => occController.SetPirEnable(true));
	        trilist.SetSigTrueAction(joinMap.DisablePir.JoinNumber, () => occController.SetPirEnable(false));
	        occController.PirSensorEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnablePir.JoinNumber]);

	        // PIR Sensitivity in Occupied State
	        trilist.SetBoolSigAction(joinMap.IncrementPirInOccupiedState.JoinNumber,
	            occController.IncrementPirSensitivityInOccupiedState);
	        trilist.SetBoolSigAction(joinMap.DecrementPirInOccupiedState.JoinNumber,
	            occController.DecrementPirSensitivityInOccupiedState);
	        occController.PirSensitivityInOccupiedStateFeedback.LinkInputSig(
	            trilist.UShortInput[joinMap.PirSensitivityInOccupiedState.JoinNumber]);

	        // PIR Sensitivity in Vacant State
	        trilist.SetBoolSigAction(joinMap.IncrementPirInVacantState.JoinNumber,
	            occController.IncrementPirSensitivityInVacantState);
	        trilist.SetBoolSigAction(joinMap.DecrementPirInVacantState.JoinNumber,
	            occController.DecrementPirSensitivityInVacantState);
	        occController.PirSensitivityInVacantStateFeedback.LinkInputSig(
	            trilist.UShortInput[joinMap.PirSensitivityInVacantState.JoinNumber]);
	    }
	}



}