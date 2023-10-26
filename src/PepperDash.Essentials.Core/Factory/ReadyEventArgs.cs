using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core
{
}

namespace PepperDash_Essentials_Core
{
    [Obsolete("Use PepperDash.Essentials.Core")]
    public class IsReadyEventArgs : EventArgs
    {
        public bool IsReady { get; set; }

        public IsReadyEventArgs(bool data)
        {
            IsReady = data;
        }
    }

    [Obsolete("Use PepperDash.Essentials.Core")]
    public interface IHasReady
    {
        event EventHandler<IsReadyEventArgs> IsReadyEvent;
        bool IsReady { get; }
    }
}