using Crestron.SimplSharpPro;
using Crestron.SimplSharpProInternal;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Crestron_IO.Cards
{
    public class C3CardControllerBase:CrestronGenericBaseDevice
    {
        private C3Card _card;

        public C3CardControllerBase(string key, string name, C3Card hardware) : base(key, name, hardware)
        {
            _card = hardware;
        }
    }
}