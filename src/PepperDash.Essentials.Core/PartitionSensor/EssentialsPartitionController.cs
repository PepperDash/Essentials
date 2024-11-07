using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.PartitionSensor
{
    /// <summary>
    /// Represents an abstract controller device for a partition dividing rooms that are combinable
    /// 
    /// In Auto mode, it can use a partition sensor to automatically determine whether the partition is present.
    /// 
    /// In Manual mode it accepts user input to tell it whether the partition is present.
    /// </summary>
    public class EssentialsPartitionController : IPartitionController
    {
        private IPartitionStateProvider _partitionSensor;

        public bool IsInAutoMode { get; private set; }

        private bool _partitionPresent;

        public bool PartitionPresent
        {
            get
            {
                if (IsInAutoMode)
                {
                    return _partitionSensor.PartitionPresentFeedback.BoolValue;
                }

                return _partitionPresent;
            }
            set
            {
                if (_partitionPresent == value)
                {
                    return;
                }

                _partitionPresent = value;

                if (PartitionPresentFeedback != null)
                {
                    PartitionPresentFeedback.FireUpdate();
                }
            }
        }

        public EssentialsPartitionController(string key, string name, IPartitionStateProvider sensor, bool defaultToManualMode, List<string> adjacentRoomKeys)
        {
            Key = key;

            Name = name;

            AdjacentRoomKeys = adjacentRoomKeys;

            if (sensor != null)
            {
                _partitionSensor = sensor;

                if (!defaultToManualMode)
                {
                    SetAutoMode();
                }
                else
                {
                    SetManualMode();
                }
            }
            else
            {
                SetManualMode();
            }

            PartitionPresentFeedback.FireUpdate();
        }

        private void PartitionPresentFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (IsInAutoMode)
            {
                PartitionPresent = e.BoolValue;
            }
        }

        #region IPartitionController Members

        public List<string> AdjacentRoomKeys { get; private set; }

        public void SetAutoMode()
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Setting {Key} to Auto Mode", this);

            IsInAutoMode = true;
            if (PartitionPresentFeedback != null)
            {
                PartitionPresentFeedback.SetValueFunc(() => _partitionSensor.PartitionPresentFeedback.BoolValue);
            }
            else
            {
                PartitionPresentFeedback = new BoolFeedback(() => _partitionSensor.PartitionPresentFeedback.BoolValue);
            }

            if (_partitionSensor != null)
            {
                _partitionSensor.PartitionPresentFeedback.OutputChange -= PartitionPresentFeedback_OutputChange;
                _partitionSensor.PartitionPresentFeedback.OutputChange += PartitionPresentFeedback_OutputChange;
                PartitionPresent = _partitionSensor.PartitionPresentFeedback.BoolValue;
            }

            PartitionPresentFeedback.FireUpdate();
        }

        public void SetManualMode()
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Setting {Key} to Manual Mode", this);

            IsInAutoMode = false;
            if (PartitionPresentFeedback != null)
            {
                PartitionPresentFeedback.SetValueFunc(() => _partitionPresent);
            }
            else
            {
                PartitionPresentFeedback = new BoolFeedback(() => _partitionPresent);
            }

            if (_partitionSensor != null)
            {
                _partitionSensor.PartitionPresentFeedback.OutputChange -= PartitionPresentFeedback_OutputChange;
                PartitionPresent = _partitionSensor.PartitionPresentFeedback.BoolValue;
            }

            PartitionPresentFeedback.FireUpdate();
        }


        public void SetPartitionStatePresent()
        {
            if (!IsInAutoMode)
            {
                PartitionPresent = true;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        public void SetPartitionStateNotPresent()
        {
            if (!IsInAutoMode)
            {
                PartitionPresent = false;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        public void ToggglePartitionState()
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Toggling Partition State for {Key}", this);
            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"IsInAutoMode: {IsInAutoMode}", this);

            if (!IsInAutoMode)
            {
                PartitionPresent = !PartitionPresent;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        #endregion

        #region IPartitionStateProvider Members

        public BoolFeedback PartitionPresentFeedback { get; private set; }

        #endregion

        #region IKeyName Members

        public string Name { get; private set; }

        #endregion

        #region IKeyed Members

        public string Key { get; private set; }

        #endregion
    }
}