extern alias Full;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Full.Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Bridges.JoinMaps;

using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Config;
using PepperDash_Essentials_Core.PartitionSensor;

namespace PepperDash.Essentials.Core
{
	[Description("Wrapper class for GLS Cresnet Partition Sensor")]
	public class GlsPartitionSensorController : CrestronGenericBridgeableBaseDevice, IPartitionStateProvider
	{

        public GlsPartitionSensorPropertiesConfig PropertiesConfig { get; private set; }
        
        private GlsPartCn _partitionSensor;

		public BoolFeedback EnableFeedback { get; private set; }
		public BoolFeedback PartitionPresentFeedback { get; private set; }
		public BoolFeedback PartitionNotSensedFeedback { get; private set; }
		public IntFeedback SensitivityFeedback { get; private set; }

		public bool InTestMode { get; private set; }
		public bool TestEnableFeedback { get; private set; }
		public bool TestPartitionSensedFeedback { get; private set; }
		public int TestSensitivityFeedback { get; private set; }


		public GlsPartitionSensorController(string key, Func<DeviceConfig, GlsPartCn> preActivationFunc, DeviceConfig config)
			: base(key, config.Name)
		{

		    var props = config.Properties.ToObject<GlsPartitionSensorPropertiesConfig>();
		    if (props != null)
		    {
		        PropertiesConfig = props;
		    }
		    else
		    {
		        Debug.Console(1, this, "props are null.  Unable to deserialize into GlsPartSensorPropertiesConfig");
		    }

            AddPreActivationAction(() =>
            {
                _partitionSensor = preActivationFunc(config);
                
                RegisterCrestronGenericBase(_partitionSensor);
                                
                EnableFeedback = new BoolFeedback(() => InTestMode ? TestEnableFeedback : _partitionSensor.EnableFeedback.BoolValue);
                PartitionPresentFeedback = new BoolFeedback(() => InTestMode ? TestPartitionSensedFeedback : _partitionSensor.PartitionSensedFeedback.BoolValue);
                PartitionNotSensedFeedback = new BoolFeedback(() => InTestMode ? !TestPartitionSensedFeedback : _partitionSensor.PartitionNotSensedFeedback.BoolValue);
                SensitivityFeedback = new IntFeedback(() => InTestMode ? TestSensitivityFeedback : _partitionSensor.SensitivityFeedback.UShortValue);

                if (_partitionSensor != null)
                {
                    _partitionSensor.BaseEvent += PartitionSensor_BaseEvent;                    
                }
            });

            AddPostActivationAction(() =>
            {
                _partitionSensor.OnlineStatusChange += (o, a) =>
                {
                    if (a.DeviceOnLine)
                    {
                        ApplySettingsToSensorFromConfig();
                    }
                };

                if (_partitionSensor.IsOnline)
                {
                    ApplySettingsToSensorFromConfig();
                }
            });
		}        

	    private void ApplySettingsToSensorFromConfig()
	    {
	        if (_partitionSensor.IsOnline == false) return;

	        Debug.Console(1, this, "Attempting to apply settings to sensor from config");            

	        if (PropertiesConfig.Sensitivity != null)
	        {
	            Debug.Console(1, this, "Sensitivity found, attempting to set value '{0}' from config",
	                PropertiesConfig.Sensitivity);
                _partitionSensor.Sensitivity.UShortValue = (ushort) PropertiesConfig.Sensitivity;
	        }	        
	        else
	        {
	            Debug.Console(1, this, "Sensitivity null, no value specified in config");
	        }

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
                        Debug.Console(1, this, "Partition Sensed State: {0}", _partitionSensor.PartitionSensedFeedback.BoolValue);
						PartitionPresentFeedback.FireUpdate();
						break;
					}
				case (GlsPartCn.PartitionNotSensedFeedbackEventId):
					{
                        Debug.Console(1, this, "Partition Not Sensed State: {0}", _partitionSensor.PartitionNotSensedFeedback.BoolValue);
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

			    EnableFeedback.FireUpdate();

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

                PartitionPresentFeedback.FireUpdate();
                PartitionNotSensedFeedback.FireUpdate();

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

                SensitivityFeedback.FireUpdate();
				Debug.Console(1, this, "TestSensitivityFeedback: {0}", TestSensitivityFeedback);
				return;
			}

			Debug.Console(1, this, "InTestMode: {0}, unable to set sensitivity value: {1}", InTestMode.ToString(), value);
		}

	    public void GetSettings()
	    {
	        var dash = new string('*', 50);
	        CrestronConsole.PrintLine(string.Format("{0}\n", dash));

	        Debug.Console(0, this, "Enabled State: {0}", _partitionSensor.EnableFeedback.BoolValue);

	        Debug.Console(0, this, "Partition Sensed State: {0}", _partitionSensor.PartitionSensedFeedback.BoolValue);
	        Debug.Console(0, this, "Partition Not Sensed State: {0}", _partitionSensor.PartitionNotSensedFeedback.BoolValue);

	        Debug.Console(0, this, "Sensitivity Value: {0}", _partitionSensor.SensitivityFeedback.UShortValue);

	        CrestronConsole.PrintLine(string.Format("{0}\n", dash));
	    }

	    public void SetEnableState(bool state)
		{
            Debug.Console(2, this, "Sensor is {0}, SetEnableState: {1}", _partitionSensor == null ? "null" : "not null", state);
			if (_partitionSensor == null)
				return;

			_partitionSensor.Enable.BoolValue = state;
		}

		public void IncreaseSensitivity()
		{
            Debug.Console(2, this, "Sensor is {0}, IncreaseSensitivity", _partitionSensor == null ? "null" : "not null");
			if (_partitionSensor == null)
				return;

			_partitionSensor.IncreaseSensitivity();
		}

		public void DecreaseSensitivity()
		{
            Debug.Console(2, this, "Sensor is {0}, DecreaseSensitivity", _partitionSensor == null ? "null" : "not null");
			if (_partitionSensor == null)
				return;

			_partitionSensor.DecreaseSensitivity();
		}

		public void SetSensitivity(ushort value)
		{
            Debug.Console(2, this, "Sensor is {0}, SetSensitivity: {1}", _partitionSensor == null ? "null" : "not null", value);
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

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);            
		    trilist.StringInput[joinMap.Name.JoinNumber].StringValue = _partitionSensor.Name;
			
		    trilist.SetBoolSigAction(joinMap.Enable.JoinNumber, SetEnableState);
            EnableFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Enable.JoinNumber]);

            PartitionPresentFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PartitionSensed.JoinNumber]);
            PartitionNotSensedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PartitionNotSensed.JoinNumber]);

			trilist.SetSigTrueAction(joinMap.IncreaseSensitivity.JoinNumber, IncreaseSensitivity);
			trilist.SetSigTrueAction(joinMap.DecreaseSensitivity.JoinNumber, DecreaseSensitivity);

            SensitivityFeedback.LinkInputSig(trilist.UShortInput[joinMap.Sensitivity.JoinNumber]);
            trilist.SetUShortSigAction(joinMap.Sensitivity.JoinNumber, SetSensitivity);

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
				    trilist.StringInput[joinMap.Name.JoinNumber].StringValue = _partitionSensor.Name;
					FeedbacksFireUpdates();
				}
			};
		}

		private void FeedbacksFireUpdates()
		{
			IsOnline.FireUpdate();			
			EnableFeedback.FireUpdate();
			PartitionPresentFeedback.FireUpdate();
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
                Debug.Console(1, "Factory Attempting to create new GlsPartitionSensorController Device");

                return new GlsPartitionSensorController(dc.Key, GetGlsPartCnDevice, dc);
            }
        }

	}
}