using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    [ConfigSnippet("\"properties\":{\"cards\":{\"1\":\"c3com3\",\"2\":\"c3ry16\",\"3\":\"c3ry8\"}}")]
    public class CenCi33Controller : CrestronGenericBaseDevice
    {
        private const string CardKeyTemplate = "{0}-card{1}";
        private const string CardNameTemplate = "{0}:{1}:{2}";
        private const uint CardSlots = 3;
        private readonly CenCi33 _cardCage;
        private readonly CenCi33Configuration _config;

        private readonly Dictionary<string, Func<CenCi33, uint, C3CardControllerBase>> _cardDict; 

        public CenCi33Controller(string key, string name, CenCi33Configuration config, CenCi33 sensor) : base(key, name, sensor)
        {
            _cardCage = sensor;

            _config = config;

            _cardDict = new Dictionary<string, Func<CenCi33, uint, C3CardControllerBase>>
            {
                {
                    "c3com3",
                    (c, s) =>
                        new C3Com3Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Com3"), new C3com3(s,_cardCage))
                },
                {
                    "c3io16",
                    (c, s) =>
                        new C3Io16Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Io16"), new C3io16(s,_cardCage))
                },
                {
                    "c3ir8",
                    (c, s) =>
                        new C3Ir8Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ir8"), new C3ir8(s,_cardCage))
                },
                {
                    "c3ry16",
                    (c, s) =>
                        new C3Ry16Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ry16"), new C3ry16(s,_cardCage))
                },
                {
                    "c3ry8",
                    (c, s) =>
                        new C3Ry8Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ry8"), new C3ry8(s,_cardCage))
                },

            };

            GetCards();
        }

        private void GetCards()
        {
            if (_config.Cards == null)
            {
                Debug.Console(0, this, "No card configuration for this device found");
                return;
            }

            for (uint i = 1; i <= CardSlots; i++)
            {
                string cardType;
                if (!_config.Cards.TryGetValue(i, out cardType))
                {
                    Debug.Console(1, this, "No card found for slot {0}", i);
                    continue;
                }

                if (String.IsNullOrEmpty(cardType))
                {
                    Debug.Console(0, this, "No card specified for slot {0}", i);
                    return;
                }

                Func<CenCi33, uint, C3CardControllerBase> cardBuilder;
                if (!_cardDict.TryGetValue(cardType.ToLower(), out cardBuilder))
                {
                    Debug.Console(0, "Unable to find factory for 3-Series card type {0}.", cardType);
                    return;
                }

                var device = cardBuilder(_cardCage, i);

                DeviceManager.AddDevice(device);
            }
        }
    }

    public class CenCi33Configuration
    {
        [JsonProperty("cards")]
        public Dictionary<uint, string> Cards { get; set; }
    }

    public class CenCi33ControllerFactory : EssentialsDeviceFactory<CenCi33Controller>
    {
        public CenCi33ControllerFactory()
        {
            TypeNames = new List<string> {"cenci33"};
        }
        #region Overrides of EssentialsDeviceFactory<CenCi33Controller>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory attempting to build new CEN-CI-3");

            var controlProperties = CommFactory.GetControlPropertiesConfig(dc);
            var ipId = controlProperties.IpIdInt;

            var cardCage = new CenCi33(ipId, Global.ControlSystem);
            var config = dc.Properties.ToObject<CenCi33Configuration>();

            return new CenCi33Controller(dc.Key, dc.Name, config, cardCage);
        }

        #endregion
    }
}