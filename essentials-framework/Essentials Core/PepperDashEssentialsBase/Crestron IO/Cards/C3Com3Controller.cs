using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
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