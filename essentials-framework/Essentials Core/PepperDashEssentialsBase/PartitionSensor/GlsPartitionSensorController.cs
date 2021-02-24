using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Bridges.JoinMaps;

using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core
{
	[Description("Wrapper class for GLS Cresnet Partition Sensor")]
	public class GlsPartitionSensorController : CrestronGenericBridgeableBaseDevice
	{
		private GlsPartCn _partitionSensor;

		public StringFeedback NameFeedback { get; private set; }
		public BoolFeedback EnableFeedback { get; private set; }
		public BoolFeedback PartitionSensedFeedback { get; private set; }
		public BoolFeedback PartitionNotSensedFeedback { get; private set; }
		public IntFeedback SensitivityFeedback { get; private set; }

		public bool InTestMode { get; private set; }
		public bool TestEnableFeedback { get; private set; }
		public bool TestPartitionSensedFeedback { get; private set; }
		public int TestSensitivityFeedback { get; private set; }


		public GlsPartitionSensorController(string key, Func<DeviceConfig, GlsPartCn> preActivationFunc, DeviceConfig config)
			: base(key, config.Name)
		{
            AddPreActivationAction(() =>
            {
                _partitionSensor = preActivationFunc(config);

                RegisterCrestronGenericBase(_partitionSensor);

                NameFeedback = new StringFeedback(() => Name);
                EnableFeedback = new BoolFeedback(() => _partitionSensor.EnableFeedback.BoolValue);
                PartitionSensedFeedback = new BoolFeedback(() => _partitionSensor.PartitionSensedFeedback.BoolValue);
                PartitionNotSensedFeedback = new BoolFeedback(() => _partitionSensor.PartitionNotSensedFeedback.BoolValue);
                SensitivityFeedback = new IntFeedback(() => _partitionSensor.SensitivityFeedback.UShortValue);

                if (_partitionSensor != null) _partitionSensor.BaseEvent += PartitionSensor_BaseEvent;
            });
		}

		private void PartitionSensor_BaseEvent(GenericBase device, BaseEventArgs args)
		{
			Debug.Console(2, this, "EventId: {0}, Index: {1}", args.EventId, args.Index);

			switch (args.EventId)
			{
				case (GlsPartCn.EnableFeedbackEventId):
					{
						EnableFeedback.FireUpdate();
						break;
					}
				case (GlsPartCn.PartitionSensedFeedbackEventId):
					{
						PartitionSensedFeedback.FireUpdate();
						break;
					}
				case (GlsPartCn.PartitionNotSensedFeedbackEventId):
					{
						PartitionNotSensedFeedback.FireUpdate();
						break;
					}
				case (GlsPartCn.SensitivityFeedbackEventId):
					{
						SensitivityFeedback.FireUpdate();
						break;
					}
				default:
					{
						Debug.Console(2, this, "Unhandled args.EventId: {0}", args.EventId);
						break;
					}
			}
		}

		public void SetTestMode(bool mode)
		{
			InTestMode = mode;
			Debug.Console(1, this, "InTestMode: {0}", InTestMode.ToString());
		}

		public void SetTestEnableState(bool state)
		{
			if (InTestMode)
			{
				TestEnableFeedback = state;
				Debug.Console(1, this, "TestEnableFeedback: {0}", TestEnableFeedback.ToString());
				return;
			}

			Debug.Console(1, this, "InTestMode: {0}, unable to set enable state: {1}", InTestMode.ToString(), state.ToString());
		}

		public void SetTestPartitionSensedState(bool state)
		{
			if (InTestMode)
			{
				TestPartitionSensedFeedback = state;
				Debug.Console(1, this, "TestPartitionSensedFeedback: {0}", TestPartitionSensedFeedback.ToString());
				return;
			}

			Debug.Console(1, this, "InTestMode: {0}, unable to set partition state: {1}", InTestMode.ToString(), state.ToString());
		}

		public void SetTestSensitivityValue(int value)
		{
			if (InTestMode)
			{
				TestSensitivityFeedback = value;
				Debug.Console(1, this, "TestSensitivityFeedback: {0}", TestSensitivityFeedback);
				return;
			}

			Debug.Console(1, this, "InTestMode: {0}, unable to set sensitivity value: {1}", InTestMode.ToString(), value);
		}

		public void SetEnableState(bool state)
		{
			if (_partitionSensor == null)
				return;

			_partitionSensor.Enable.BoolValue = state;
		}

		public void IncreaseSensitivity()
		{
			if (_partitionSensor == null)
				return;

			_partitionSensor.IncreaseSensitivity();
		}

		public void DecreaseSensitivity()
		{
			if (_partitionSensor == null)
				return;

			_partitionSensor.DecreaseSensitivity();
		}

		public void SetSensitivity(ushort value)
		{
			if (_partitionSensor == null)
				return;

			_partitionSensor.Sensitivity.UShortValue = value;
		}

		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new GlsPartitionSensorJoinMap(joinStart);
			var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<GlsPartitionSensorJoinMap>(joinMapSerialized);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}
			else
			{
				Debug.Console(0, this, "Please update config to use 'type': 'EiscApiAdvanced' to get all join map features for this device");
			}

			Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, this, "Linking to Bridge Type {0}", GetType().Name);

			// link input from simpl
			trilist.SetSigTrueAction(joinMap.Enable.JoinNumber, () => SetEnableState(true));
			trilist.SetSigFalseAction(joinMap.Enable.JoinNumber, () => SetEnableState(false));
			trilist.SetSigTrueAction(joinMap.IncreaseSensitivity.JoinNumber, IncreaseSensitivity);
			trilist.SetSigTrueAction(joinMap.DecreaseSensitivity.JoinNumber, DecreaseSensitivity);
			trilist.SetUShortSigAction(joinMap.Sensitivity.JoinNumber, SetSensitivity);

			// link output to simpl
			IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			EnableFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Enable.JoinNumber]);
			PartitionSensedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PartitionSensed.JoinNumber]);
			PartitionNotSensedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PartitionNotSensed.JoinNumber]);
			SensitivityFeedback.LinkInputSig(trilist.UShortInput[joinMap.Sensitivity.JoinNumber]);

			FeedbacksFireUpdates();

			// update when device is online
			_partitionSensor.OnlineStatusChange += (o, a) =>
			{
				if (a.DeviceOnLine)
				{
					FeedbacksFireUpdates();
				}
			};

			// update when trilist is online
			trilist.OnlineStatusChange += (o, a) =>
			{
				if (a.DeviceOnLine)
				{
					FeedbacksFireUpdates();
				}
			};
		}

		private void FeedbacksFireUpdates()
		{
			IsOnline.FireUpdate();
			NameFeedback.FireUpdate();
			EnableFeedback.FireUpdate();
			PartitionSensedFeedback.FireUpdate();
			PartitionNotSensedFeedback.FireUpdate();
			SensitivityFeedback.FireUpdate();
		}

        #region PreActivation

        private static GlsPartCn GetGlsPartCnDevice(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsPartCn", parentKey);
                return new GlsPartCn(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new GlsPartCn", parentKey);
                return new GlsPartCn(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
        #endregion


        public class GlsPartitionSensorControllerFactory : EssentialsDeviceFactory<GlsPartitionSensorController>
        {
            public GlsPartitionSensorControllerFactory()
            {
                TypeNames = new List<string> { "glspartcn" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new C2N-RTHS Device");

                return new GlsPartitionSensorController(dc.Key, GetGlsPartCnDevice, dc);
            }
        }

	}
}