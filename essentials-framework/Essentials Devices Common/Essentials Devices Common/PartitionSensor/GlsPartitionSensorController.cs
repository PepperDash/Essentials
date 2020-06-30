using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash_Essentials_Core.Bridges.JoinMaps;

namespace PepperDash.Essentials.Devices.Common.PartitionSensor
{
	[Description("Wrapper class for GLS Cresnet Partition Sensor")]
	public class GlsPartitionSensorController : CrestronGenericBridgeableBaseDevice
	{
		public GlsPartCn PartitionSensor { get; private set; }

		public StringFeedback NameFeedback { get; private set; }

		public BoolFeedback EnableFeedback { get; private set; }

		public BoolFeedback PartitionSensedFeedback { get; private set; }
		public BoolFeedback PartitionNotSensedFeedback { get; private set; }

		public IntFeedback SensitivityFeedback { get; private set; }

		public bool InTestMode { get; private set; }
		public bool TestEnableFeedback { get; private set; }
		public bool TestPartitionSensedFeedback { get; private set; }
		public int TestSensitivityFeedback { get; private set; }

		public Func<string> NameFeedbackFunc
		{
			get { return () => Name; }
		}

		public Func<bool> EnableFeedbackFunc
		{
			get { return () => InTestMode ? TestEnableFeedback : PartitionSensor.EnableFeedback.BoolValue; }
		}

		public Func<bool> PartitionSensedFeedbackFunc
		{
			get { return () => InTestMode ? TestPartitionSensedFeedback : PartitionSensor.PartitionSensedFeedback.BoolValue; }
		}

		public Func<bool> PartitionNotSensedFeedbackFunc
		{
			get { return () => InTestMode ? TestPartitionSensedFeedback : PartitionSensor.PartitionNotSensedFeedback.BoolValue; }
		}

		public Func<int> SensitivityFeedbackFunc
		{
			get { return () => InTestMode ? TestSensitivityFeedback : PartitionSensor.SensitivityFeedback.UShortValue; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="hardware"></param>
		public GlsPartitionSensorController(string key, string name, GlsPartCn hardware)
			: base(key, name, hardware)
		{
			PartitionSensor = hardware;

			NameFeedback = new StringFeedback(NameFeedbackFunc);
			EnableFeedback = new BoolFeedback(EnableFeedbackFunc);
			PartitionSensedFeedback = new BoolFeedback(PartitionSensedFeedbackFunc);
			PartitionNotSensedFeedback = new BoolFeedback(PartitionNotSensedFeedbackFunc);
			SensitivityFeedback = new IntFeedback(SensitivityFeedbackFunc);

			if (PartitionSensor != null) PartitionSensor.BaseEvent += PartitionSensor_BaseEvent;
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
						Debug.Console(2, this, "args.EventId: {0}", args.EventId);
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
			if (PartitionSensor == null)
				return;

			PartitionSensor.Enable.BoolValue = state;
		}

		public void IncreaseSensitivity()
		{
			if (PartitionSensor == null)
				return;

			PartitionSensor.IncreaseSensitivity();
		}

		public void DecreaseSensitivity()
		{
			if (PartitionSensor == null)
				return;

			PartitionSensor.DecreaseSensitivity();
		}

		public void SetSensitivity(ushort value)
		{
			if (PartitionSensor == null)
				return;

			PartitionSensor.Sensitivity.UShortValue = (ushort)value;
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
			PartitionNotSensedFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PartitionNotSensed.JoinNumber]);
			SensitivityFeedback.LinkInputSig(trilist.UShortInput[joinMap.Sensitivity.JoinNumber]);

			FeedbacksFireUpdates();

			// update when device is online
			PartitionSensor.OnlineStatusChange += (o, a) =>
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
	}
}