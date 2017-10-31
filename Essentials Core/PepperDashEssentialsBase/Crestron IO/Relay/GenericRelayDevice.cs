using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core.Crestron_IO
{
    /// <summary>
    /// Represents a generic device controlled by relays
    /// </summary>
    public class GenericRelayDevice
    {
        //Relay RelayOutput { get; private set; }

        //public boolfeedback relaystatefeedback { get; private set; }

        //func<bool> relaystatefeedbackfunc
        //{
        //    get
        //    {
        //        return () => relayoutput.state;
        //    }
        //}

        //public genericrelaydevice(relay relay)
        //{
        //    relaystatefeedback = new boolfeedback(relaystatefeedbackfunc);

        //    if(relay.availableforuse)
        //        relayoutput = relay;

        //    relayoutput.statechange += new relayeventhandler(relayoutput_statechange);
        //}

        //void relayoutput_statechange(relay relay, relayeventargs args)
        //{
        //    relaystatefeedback.fireupdate();
        //}
    }
}