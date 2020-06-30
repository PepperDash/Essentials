using System;
using Crestron.SimplSharpProInternal;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class C3CardControllerBase:CrestronGenericBaseDevice
    {
        private readonly C3Card _card;

        public C3CardControllerBase(string key, string name, C3Card sensor) : base(key, name, sensor)
        {
            _card = sensor;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return String.Format("{0} {1}", Key, _card.ToString());
        }

        #endregion
    }
}