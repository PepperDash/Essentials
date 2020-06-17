using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash_Essentials_Core.Crestron_IO.Cards
{
    public class C3Com3Controller:C3CardControllerBase, IComPorts
    {
        private readonly C3com3 _card;

        public C3Com3Controller(string key, string name, C3com3 hardware) : base(key, name, hardware)
        {
            _card = hardware;
        }

        #region Implementation of IComPorts

        public CrestronCollection<ComPort> ComPorts
        {
            get { return _card.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return _card.NumberOfComPorts; }
        }

        #endregion
    }
}