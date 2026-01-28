using PepperDash.Core;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
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

        /// <summary>
        /// Indicates whether the controller is in Auto mode or Manual mode
        /// </summary>
        public bool IsInAutoMode { get; private set; }

        private bool _partitionPresent;

        /// <summary>
        /// Gets or sets the PartitionPresent state
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key for the partition controller</param>
        /// <param name="name">name of the partition controller</param>
        /// <param name="sensor">partition state provider sensor</param>
        /// <param name="defaultToManualMode">whether to default to manual mode</param>
        /// <param name="adjacentRoomKeys">list of adjacent room keys</param>
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

        /// <summary>
        /// Gets or sets the AdjacentRoomKeys
        /// </summary>
        public List<string> AdjacentRoomKeys { get; private set; }

        /// <summary>
        /// SetAutoMode method
        /// </summary>
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

        /// <summary>
        /// SetManualMode method
        /// </summary>
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


        /// <summary>
        /// SetPartitionStatePresent method
        /// </summary>
        public void SetPartitionStatePresent()
        {
            if (!IsInAutoMode)
            {
                PartitionPresent = true;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// SetPartitionStateNotPresent method
        /// </summary>
        public void SetPartitionStateNotPresent()
        {
            if (!IsInAutoMode)
            {
                PartitionPresent = false;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// ToggglePartitionState method
        /// </summary>
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

        /// <summary>
        /// Gets or sets the PartitionPresentFeedback
        /// </summary>
        public BoolFeedback PartitionPresentFeedback { get; private set; }

        #endregion

        #region IKeyName Members

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; private set; }

        #endregion

        #region IKeyed Members

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; private set; }

        #endregion
    }
}