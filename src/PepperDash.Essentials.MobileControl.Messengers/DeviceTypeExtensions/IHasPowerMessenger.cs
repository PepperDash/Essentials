﻿using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
    public class IHasPowerMessenger : MessengerBase
    {
        private readonly IHasPowerControl powerDevice;
        public IHasPowerMessenger(string key, string messagePath, IHasPowerControl device) : base(key, messagePath, device as IKeyName)
        {
            powerDevice = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/powerOn", (id, content) => powerDevice?.PowerOn());
            AddAction("/powerOff", (id, content) => powerDevice?.PowerOff());
            AddAction("/powerToggle", (id, content) => powerDevice?.PowerToggle());
        }
    }
}