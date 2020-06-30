using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class C3Ry16Controller:C3CardControllerBase, IRelayPorts
    {
        private readonly C3ry16 _card;

        public C3Ry16Controller(string key, string name, C3ry16 sensor) : base(key, name, sensor)
        {
            _card = sensor;
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