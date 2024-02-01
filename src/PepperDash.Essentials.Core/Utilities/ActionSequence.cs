

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Utilities
{
    /// <summary>
    /// A device that executes a sequence of actions with optional delays between actions
    /// </summary>
    [Description("A device that executes a sequence of actions with optional delays between actions")]
    public class ActionSequence : EssentialsDevice
    {
        private ActionSequencePropertiesConfig _propertiesConfig;

        private CrestronQueue<SequencedDeviceActionWrapper> _actionQueue;

        private Thread _worker;

        private bool _allowActionsToExecute;

        public ActionSequence(string key, DeviceConfig config)
            : base(key, config.Name)
        {
            var props = config.Properties.ToObject<ActionSequencePropertiesConfig>();
            _propertiesConfig = props;

            if (_propertiesConfig != null)
            {
                if (_propertiesConfig.ActionSequence.Count > 0)
                {
                    _actionQueue = new CrestronQueue<SequencedDeviceActionWrapper>(_propertiesConfig.ActionSequence.Count);
                }
            }
        }

        /// <summary>
        /// Starts executing the sequenced actions
        /// </summary>
        public void StartSequence()
        {
            if (_worker !=null && _worker.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                Debug.Console(1, this, "Thread already running.  Cannot Start Sequence");
                return;
            }

            Debug.Console(1, this, "Starting Action Sequence");
            _allowActionsToExecute = true;
            AddActionsToQueue();
            _worker = new Thread(ProcessActions, null, Thread.eThreadStartOptions.Running);
        }

        /// <summary>
        /// Stops executing the sequenced actions
        /// </summary>
        public void StopSequence()
        {
            Debug.Console(1, this, "Stopping Action Sequence");
            _allowActionsToExecute = false;
            _worker.Abort();
        }

        /// <summary>
        /// Populates the queue from the configuration information
        /// </summary>
        private void AddActionsToQueue()
        {
            Debug.Console(1, this, "Adding {0} actions to queue", _propertiesConfig.ActionSequence.Count);

            for (int i = 0; i < _propertiesConfig.ActionSequence.Count; i++)
            {
                _actionQueue.Enqueue(_propertiesConfig.ActionSequence[i]);
            }
        }

        private object ProcessActions(object obj)
        {
            while (_allowActionsToExecute && _actionQueue.Count > 0)
            {
                SequencedDeviceActionWrapper action = null;

                action = _actionQueue.Dequeue();
                if (action == null)
                    break;

                // Delay before executing
                if (action.DelayMs > 0)
                    Thread.Sleep(action.DelayMs);

                ExecuteAction(action);
            }

            return null;
        }

        private void ExecuteAction(DeviceActionWrapper action)
        {
            if (action == null)
                return;

            try
            {
                DeviceJsonApi.DoDeviceAction(action);
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error Executing Action: {0}", e);
            }
        }
    }

    /// <summary>
    /// Configuration Properties for ActionSequence
    /// </summary>
    public class ActionSequencePropertiesConfig
    {
        [JsonProperty("actionSequence")]
        public List<SequencedDeviceActionWrapper> ActionSequence { get; set; }

        public ActionSequencePropertiesConfig()
        {
            ActionSequence = new List<SequencedDeviceActionWrapper>();
        }
    }

    public class SequencedDeviceActionWrapper : DeviceActionWrapper
    {
        [JsonProperty("delayMs")]
        public int DelayMs { get; set; }
    }

    /// <summary>
    /// Factory class
    /// </summary>
    public class ActionSequenceFactory : EssentialsDeviceFactory<ActionSequence>
    {
        public ActionSequenceFactory()
        {
            TypeNames = new List<string>() { "actionsequence" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new ActionSequence Device");

            return new ActionSequence(dc.Key, dc);
        }
    }

}