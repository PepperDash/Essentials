using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class C3Ry8Controller:C3CardControllerBase, IRelayPorts
    {
        private readonly C3ry8 _card;

        public C3Ry8Controller(string key, string name, C3ry8 hardware) : base(key, name, hardware)
        {
            _card = hardware;
        }

        #region Implementation of IRelayPorts

        public CrestronCollection<Relay> RelayPorts
        {
            get { return _card.RelayPorts; }
        }

        public int NumberOfRelayPorts
        {
            get { return _card.NumberOfRelayPorts; }
        }

        #endregion
    }
}