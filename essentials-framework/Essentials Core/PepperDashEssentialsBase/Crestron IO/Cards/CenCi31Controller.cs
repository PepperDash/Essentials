using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    public class CenCi31Controller : CrestronGenericBaseDevice
    {
        private const string CardKeyTemplate = "{0}-card{1}";
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
                        new C3Com3Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardKeyTemplate, key, s), new C3com3(_cardCage))
                },
            };

            GetCards();
        }

        private void GetCards()
        {
            Func<CenCi31, uint, C3CardControllerBase> cardBuilder;

            if (String.IsNullOrEmpty(_config.Card))
            {
                
            }

            if (!_cardDict.TryGetValue(_config.Card, out cardBuilder))
            {
                Debug.Console(0, "Unable to find factory for 3-Series card type {0}.", _config.Card);
                return;
            }

            var device = cardBuilder(_cardCage, CardSlot);

            DeviceManager.AddDevice(device);
        }
    }

    public class CenCi31Configuration
    {
        [JsonProperty("card")]
        public string Card { get; set; }
    }

    public class CenCi3ControllerFactory : EssentialsDeviceFactory<CenCi31Controller>
    {
        public CenCi3ControllerFactory()
        {
            TypeNames = new List<string> {"cenci31"};
        }
        #region Overrides of EssentialsDeviceFactory<CenCi31Controller>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory attempting to build new CEN-CI-1");

            var controlProperties = CommFactory.GetControlPropertiesConfig(dc);
            var ipId = controlProperties.IpIdInt;

            var cardCage = new CenCi31(ipId, Global.ControlSystem);
            var config = dc.Properties.ToObject<CenCi31Configuration>();

            return new CenCi31Controller(dc.Key, dc.Name, config, cardCage);
        }

        #endregion
    }
}