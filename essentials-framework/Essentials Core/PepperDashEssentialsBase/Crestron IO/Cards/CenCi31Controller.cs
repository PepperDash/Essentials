using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using Crestron.SimplSharpProInternal;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash_Essentials_Core.Crestron_IO.Cards
{
    public class CenCi31Controller :CrestronGenericBridgeableBaseDevice
    {
        private const string CardKeyTemplate = "{0}-card{1}";
        private readonly CenCi31 _cardCage;
        private CenCi3Configuration _config;

        private Dictionary<string, Func<CenCi31, uint, C3CardControllerBase>> _cardDict; 

        public CenCi31Controller(string key, string name, DeviceConfig config, CenCi31 hardware) : base(key, name, hardware)
        {
            _cardCage = hardware;
            _config = config.Properties.ToObject<CenCi3Configuration>();

            _cardDict = new Dictionary<string, Func<CenCi31, uint, C3CardControllerBase>>
            {
                {
                    "c3com3",
                    (c, s) =>
                        new C3Com3Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardKeyTemplate, key, s), new C3com3(_cardCage))
                },
            };

        }

        private void GetCards()
        {
            foreach (var card in _config.Cards)
            {
                
            }
        }

        #region Overrides of CrestronGenericBridgeableBaseDevice

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class CenCi3Configuration
    {
        [JsonProperty("cards")]
        public Dictionary<uint, string> Cards { get; set; }
    }
}