using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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

        private bool isInAutoMode;

        private bool _partitionPresent;

        public bool PartitionPresent
        {
            get
            {
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

        void PartitionPresentFeedback_OutputChange(object sender, FeedbackEventArgs e)
        {
            if (isInAutoMode)
            {
                PartitionPresentFeedback.FireUpdate();
            }
        }

        #region IPartitionController Members

        public List<string> AdjacentRoomKeys { get; private set; }

        public void SetAutoMode()
        {
            isInAutoMode = true;
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
                _partitionSensor.PartitionPresentFeedback.OutputChange += PartitionPresentFeedback_OutputChange;
            }
        }

        public void SetManualMode()
        {
            isInAutoMode = false;
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
            }
        }


        public void SetPartitionStatePresent()
        {
            if (!isInAutoMode)
            {
                PartitionPresent = true;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        public void SetPartitionStateNotPresent()
        {
            if (!isInAutoMode)
            {
                PartitionPresent = false;
                PartitionPresentFeedback.FireUpdate();
            }
        }

        public void ToggglePartitionState()
        {
            if (!isInAutoMode)
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