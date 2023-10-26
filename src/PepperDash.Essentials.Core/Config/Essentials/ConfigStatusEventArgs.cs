using System;

namespace PepperDash.Essentials.Core.Config
{
    public class ConfigStatusEventArgs : EventArgs
    {
        public eUpdateStatus UpdateStatus { get; private set; }

        public ConfigStatusEventArgs(eUpdateStatus status)
        {
            UpdateStatus = status;
        }
    }
}