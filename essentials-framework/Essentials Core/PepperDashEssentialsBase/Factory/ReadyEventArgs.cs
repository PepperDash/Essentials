using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash_Essentials_Core
{
    public delegate void IsReadyEventHandler(object source, IsReadyEventArgs e);

    public class IsReadyEventArgs : EventArgs
    {
        private readonly bool _EventData;

        public IsReadyEventArgs(bool data)
        {
            _EventData = data;
        }

        public bool GetData()
        {
            return _EventData;
        }
    }

    public interface IHasReady
    {
        event IsReadyEventHandler IsReady;
    }
}