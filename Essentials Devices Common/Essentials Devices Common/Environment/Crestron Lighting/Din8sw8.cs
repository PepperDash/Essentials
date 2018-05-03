using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Lighting;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Devices.Common.Environment.Lighting
{
    public class Din8sw8Controller : Device, ISwitchedOutputCollection
    {
        // Need to figure out some sort of interface to make these switched outputs behave like processor relays so they can be used interchangably

        public Din8Sw8 SwitchModule { get; private set; }
        
        /// <summary>
        /// Collection of generic switched outputs
        /// </summary>
        public Dictionary<uint, ISwitchedOutput> SwitchedOutputs;

        public Din8sw8Controller(string key, string cresnetId)
            : base(key)
        {
            SwitchedOutputs = new Dictionary<uint, ISwitchedOutput>();

            PopulateDictionary();
        }

        /// <summary>
        /// Populates the generic collection with the loads from the Crestron collection
        /// </summary>
        void PopulateDictionary()
        {
            foreach (var item in SwitchModule.SwitchedLoads)
            {
                SwitchedOutputs.Add(item.Number, new Din8sw8Output(item));
            }
        }
    }

    /// <summary>
    /// Wrapper class to 
    /// </summary>
    public class Din8sw8Output :  ISwitchedOutput
    {
        SwitchedLoadWithOverrideParameter SwitchedOutput;

        public BoolFeedback OutputIsOnFeedback { get; protected set; }

        public Din8sw8Output(SwitchedLoadWithOverrideParameter switchedOutput)
        {
            SwitchedOutput = switchedOutput;

            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => SwitchedOutput.IsOn)); 
        }

        public void On()
        {
            SwitchedOutput.FullOn();
        }

        public void Off()
        {
            SwitchedOutput.FullOff();
        }
    }
}