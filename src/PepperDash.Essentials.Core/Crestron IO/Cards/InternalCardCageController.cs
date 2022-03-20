using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.ThreeSeriesCards;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
    [ConfigSnippet("\"properties\":{\"cards\":{\"1\":\"c3com3\",\"2\":\"c3ry16\",\"3\":\"c3ry8\"}}")]
    public class InternalCardCageController : EssentialsDevice
    {
        private const string CardKeyTemplate = "{0}-card{1}";
        private const string CardNameTemplate = "{0}:{1}:{2}";
        private const uint CardSlots = 3;
    
        private readonly InternalCardCageConfiguration _config;

        private readonly Dictionary<string, Func<uint, C3CardControllerBase>> _cardDict; 

        public InternalCardCageController(string key, string name, InternalCardCageConfiguration config) : base(key, name)
        {
            _config = config;

            _cardDict = new Dictionary<string, Func<uint, C3CardControllerBase>>
            {
                {
                    "c3com3",
                    (s) =>
                        new C3Com3Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Com3"), new C3com3(s,Global.ControlSystem))
                },
                {
                    "c3io16",
                    (s) =>
                        new C3Io16Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Io16"), new C3io16(s,Global.ControlSystem))
                },
                {
                    "c3ir8",
                    (s) =>
                        new C3Ir8Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ir8"), new C3ir8(s,Global.ControlSystem))
                },
                {
                    "c3ry16",
                    (s) =>
                        new C3Ry16Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ry16"), new C3ry16(s,Global.ControlSystem))
                },
                {
                    "c3ry8",
                    (s) =>
                        new C3Ry8Controller(String.Format(CardKeyTemplate, key, s),
                            String.Format(CardNameTemplate, key, s, "C3Ry8"), new C3ry8(s,Global.ControlSystem))
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
                    Debug.Console(0, this, "No card found for slot {0}", i);
                    continue;
                }

                if (String.IsNullOrEmpty(cardType))
                {
                    Debug.Console(0, this, "No card specified for slot {0}", i);
                    continue;
                }

                Func<uint, C3CardControllerBase> cardBuilder;
                if (!_cardDict.TryGetValue(cardType.ToLower(), out cardBuilder))
                {
                    Debug.Console(0, "Unable to find factory for 3-Series card type {0}.", cardType);
                    continue;
                }

                try
                {
                    var device = cardBuilder(i);


                    DeviceManager.AddDevice(device);
                }
                catch (InvalidOperationException ex)
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                        "Unable to add card {0} to internal card cage.\r\nError Message: {1}\r\nStack Trace: {2}",
                        cardType, ex.Message, ex.StackTrace);
                }
            }
        }
    }

    public class InternalCardCageConfiguration
    {
        [JsonProperty("cards")]
        public Dictionary<uint, string> Cards { get; set; }
    }

    public class InternalCardCageControllerFactory : EssentialsDeviceFactory<InternalCardCageController>
    {
        public InternalCardCageControllerFactory()
        {
            TypeNames = new List<string> {"internalcardcage"};
        }
        #region Overrides of EssentialsDeviceFactory<InternalCardCageController>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory attempting to build new Internal Card Cage Controller");

            if (!Global.ControlSystem.SupportsThreeSeriesPlugInCards)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Warning, "Current control system does NOT support 3-Series cards. Everything is NOT awesome.");
                return null;
            }

            var config = dc.Properties.ToObject<InternalCardCageConfiguration>();

            return new InternalCardCageController(dc.Key, dc.Name, config);
        }

        #endregion
    }
}