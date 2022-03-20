using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class C3Io16Controller:C3CardControllerBase,IIOPorts
    {
        private readonly C3io16 _card;

        public C3Io16Controller(string key, string name, C3io16 hardware) : base(key, name, hardware)
        {
            _card = hardware;
        }

        #region Implementation of IIOPorts

        public CrestronCollection<Versiport> VersiPorts
        {
            get { return _card.VersiPorts; }
        }

        public int NumberOfVersiPorts
        {
            get { return _card.NumberOfVersiPorts; }
        }

        #endregion
    }
}