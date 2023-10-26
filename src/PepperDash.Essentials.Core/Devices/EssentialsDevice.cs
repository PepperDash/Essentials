using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the basic needs for an EssentialsDevice to enable it to be build by an IDeviceFactory class
    /// </summary>
    [Description("The base Essentials Device Class")]
    public abstract class EssentialsDevice : Device
    {
        protected EssentialsDevice(string key)
            : base(key)
        {
            SubscribeToActivateComplete();
        }

        protected EssentialsDevice(string key, string name)
            : base(key, name)
        {
            SubscribeToActivateComplete();
        }

        private void SubscribeToActivateComplete()
        {
            DeviceManager.AllDevicesActivated += DeviceManagerOnAllDevicesActivated;
        }

        private void DeviceManagerOnAllDevicesActivated(object sender, EventArgs eventArgs)
        {
            CrestronInvoke.BeginInvoke((o) =>
            {
                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Error, "Exception initializing device: {0}", ex.Message);
                    Debug.Console(1, this, Debug.ErrorLogLevel.Error, "Stack Trace: {0}", ex.StackTrace);
                }
            });
        }
    }
}