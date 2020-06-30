using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class C3Ir8Controller:C3CardControllerBase, IIROutputPorts
    {
        private readonly C3ir8 _card;

        public C3Ir8Controller(string key, string name, C3ir8 sensor) : base(key, name, sensor)
        {
            _card = sensor;
        }

        #region Implementation of IIROutputPorts

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _card.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return _card.NumberOfIROutputPorts; }
        }

        #endregion
    }
}