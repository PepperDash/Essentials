

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
using Serilog.Events;

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>
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
        /// StartSequence method
        /// </summary>
        public void StartSequence()
        {
            if (_worker !=null && _worker.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Thread already running.  Cannot Start Sequence");
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, "Starting Action Sequence");
            _allowActionsToExecute = true;
            AddActionsToQueue();
            _worker = new Thread(ProcessActions, null, Thread.eThreadStartOptions.Running);
        }

        /// <summary>
        /// StopSequence method
        /// </summary>
        public void StopSequence()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Stopping Action Sequence");
            _allowActionsToExecute = false;
            _worker.Abort();
        }

        /// <summary>
        /// Populates the queue from the configuration information
        /// </summary>
        private void AddActionsToQueue()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Adding {0} actions to queue", _propertiesConfig.ActionSequence.Count);

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
                Debug.LogMessage(LogEventLevel.Verbose, this, "Error Executing Action: {0}", e);
            }
        }
    }

    /// <summary>
    /// Represents a ActionSequencePropertiesConfig
    /// </summary>
    public class ActionSequencePropertiesConfig
    {
        /// <summary>
        /// Gets or sets the ActionSequence
        /// </summary>
        [JsonProperty("actionSequence")]
        public List<SequencedDeviceActionWrapper> ActionSequence { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionSequencePropertiesConfig()
        {
            ActionSequence = new List<SequencedDeviceActionWrapper>();
        }
    }

    /// <summary>
    /// Represents a SequencedDeviceActionWrapper
    /// </summary>
    public class SequencedDeviceActionWrapper : DeviceActionWrapper
    {
        /// <summary>
        /// Gets or sets the DelayMs
        /// </summary>
        [JsonProperty("delayMs")]
        public int DelayMs { get; set; }
    }

    /// <summary>
    /// Represents a ActionSequenceFactory
    /// </summary>
    public class ActionSequenceFactory : EssentialsDeviceFactory<ActionSequence>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ActionSequenceFactory()
        {
            TypeNames = new List<string>() { "actionsequence" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <param name="dc">device config</param>
        /// <returns></returns>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new ActionSequence Device");

            return new ActionSequence(dc.Key, dc);
        }
    }

}