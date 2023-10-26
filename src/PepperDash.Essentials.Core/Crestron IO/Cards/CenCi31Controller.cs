extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    [ConfigSnippet("\"properties\":{\"card\":\"c3com3\"}")]
    public class CenCi31Controller : CrestronGenericBaseDevice
    {
        private const string CardKeyTemplate = "{0}-card";
        private const string CardNameTemplate = "{0}:{1}:{2}";
        private const uint CardSlot = 1;
        private readonly CenCi31 _cardCage;
        private readonly CenCi31Configuration _config;

        private readonly Dictionary<string, Func<CenCi31, uint, C3CardControllerBase>> _cardDict; 

        public CenCi31Controller(string key, string name, CenCi31Configuration config, CenCi31 hardware) : base(key, name, hardware)
        {
            _cardCage = hardware;

            _config = config;

            _cardDict = new Dictionary<string, Func<CenCi31, uint, C3CardControllerBase>>
            {
                {
                    "c3com3",
                    (c, s) =>
                        new C3Com3Controller(String.Format(CardKeyTemplate, key),
                            String.Format(CardNameTemplate, key, s, "C3Com3"), new C3com3(_cardCage))
                },
                {
                    "c3io16",
                    (c, s) =>
                        new C3Io16Controller(String.Format(CardKeyTemplate, key),
                            String.Format(CardNameTemplate, key, s,"C3Io16"), new C3io16(_cardCage))
                },
                {
                    "c3ir8",
                    (c, s) =>
                        new C3Ir8Controller(String.Format(CardKeyTemplate, key),
                            String.Format(CardNameTemplate, key, s, "C3Ir8"), new C3ir8(_cardCage))
                },
                {
                    "c3ry16",
                    (c, s) =>
                        new C3Ry16Controller(String.Format(CardKeyTemplate, key),
                            String.Format(CardNameTemplate, key, s, "C3Ry16"), new C3ry16(_cardCage))
                },
                {
                    "c3ry8",
                    (c, s) =>
                        new C3Ry8Controller(String.Format(CardKeyTemplate, key),
                            String.Format(CardNameTemplate, key, s, "C3Ry8"), new C3ry8(_cardCage))
                },

            };

            GetCards();
        }

        private void GetCards()
        {
            Func<CenCi31, uint, C3CardControllerBase> cardBuilder;
            
            if (String.IsNullOrEmpty(_config.Card))
            {
                Debug.Console(0, this, "No card specified");
                return;
            }

            if (!_cardDict.TryGetValue(_config.Card.ToLower(), out cardBuilder))
            {
                Debug.Console(0, "Unable to find factory for 3-Series card type {0}.", _config.Card);
                return;
            }

            var device = cardBuilder(_cardCage, CardSlot);

            DeviceManager.AddDevice(device);
        }
    }
}