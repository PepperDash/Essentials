using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
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