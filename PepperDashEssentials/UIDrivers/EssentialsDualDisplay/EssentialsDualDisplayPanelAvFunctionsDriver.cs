using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices.Codec;
using PepperDash.Essentials.Core.Devices.VideoCodec;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.Core.Touchpanels.Keyboards;
using PepperDash.Essentials.UIDrivers;
using PepperDash.Essentials.UIDrivers.VC;

namespace PepperDashEssentials.UIDrivers.EssentialsDualDisplay
{
    public class EssentialsDualDisplayPanelAvFunctionsDriver : EssentialsHuddleVtc1PanelAvFunctionsDriver
    {
        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) : base(parent, config)
        {
        }
    }
}