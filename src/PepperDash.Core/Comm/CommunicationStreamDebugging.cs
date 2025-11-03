using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash.Core
{
    /// <summary>
    /// Controls the ability to disable/enable debugging of TX/RX data sent to/from a device with a built in timer to disable
    /// </summary>
    public class CommunicationStreamDebugging
    {
        /// <summary>
        /// Device Key that this instance configures
        /// </summary>
        public string ParentDeviceKey { get; private set; }

        /// <summary>
        /// Timer to disable automatically if not manually disabled
        /// </summary>
        private CTimer DebugExpiryPeriod;

        /// <summary>
        /// Gets or sets the DebugSetting
        /// </summary>
        public eStreamDebuggingSetting DebugSetting { get; private set; }

        private uint _DebugTimeoutInMs;
        private const uint _DefaultDebugTimeoutMin = 30;

        /// <summary>
        /// Timeout in Minutes
        /// </summary>
        public uint DebugTimeoutMinutes
        {
            get
            {
                return _DebugTimeoutInMs / 60000;
            }
        }

        /// <summary>
        /// Gets or sets the RxStreamDebuggingIsEnabled
        /// </summary>
        public bool RxStreamDebuggingIsEnabled { get; private set; }

        /// <summary>
        /// Indicates that transmit stream debugging is enabled
        /// </summary>
        public bool TxStreamDebuggingIsEnabled { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentDeviceKey"></param>
        public CommunicationStreamDebugging(string parentDeviceKey)
        {
            ParentDeviceKey = parentDeviceKey;
        }


        /// <summary>
        /// Sets the debugging setting and if not setting to off, assumes the default of 30 mintues
        /// </summary>
        /// <param name="setting"></param>
        /// <summary>
        /// SetDebuggingWithDefaultTimeout method
        /// </summary>
        public void SetDebuggingWithDefaultTimeout(eStreamDebuggingSetting setting)
        {
            if (setting == eStreamDebuggingSetting.Off)
            {
                DisableDebugging();
                return;
            }

            SetDebuggingWithSpecificTimeout(setting, _DefaultDebugTimeoutMin);
        }

        /// <summary>
        /// Sets the debugging setting for the specified number of minutes
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="minutes"></param>
        /// <summary>
        /// SetDebuggingWithSpecificTimeout method
        /// </summary>
        public void SetDebuggingWithSpecificTimeout(eStreamDebuggingSetting setting, uint minutes)
        {
            if (setting == eStreamDebuggingSetting.Off)
            {
                DisableDebugging();
                return;
            }

            _DebugTimeoutInMs = minutes * 60000;

            StopDebugTimer();

            DebugExpiryPeriod = new CTimer((o) => DisableDebugging(), _DebugTimeoutInMs);

            if ((setting & eStreamDebuggingSetting.Rx) == eStreamDebuggingSetting.Rx)
                RxStreamDebuggingIsEnabled = true;

            if ((setting & eStreamDebuggingSetting.Tx) == eStreamDebuggingSetting.Tx)
                TxStreamDebuggingIsEnabled = true;

            Debug.SetDeviceDebugSettings(ParentDeviceKey, setting);

        }

        /// <summary>
        /// Disabled debugging
        /// </summary>
        private void DisableDebugging()
        {
            StopDebugTimer();

            Debug.SetDeviceDebugSettings(ParentDeviceKey, eStreamDebuggingSetting.Off);
        }

        private void StopDebugTimer()
        {
            RxStreamDebuggingIsEnabled = false;
            TxStreamDebuggingIsEnabled = false;

            if (DebugExpiryPeriod == null)
            {
                return;
            }

            DebugExpiryPeriod.Stop();
            DebugExpiryPeriod.Dispose();
            DebugExpiryPeriod = null;
        }
    }
}
