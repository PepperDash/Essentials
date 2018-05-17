using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials
{
    public class EssentialsEnvironmentDriver : PanelDriverBase
    {
        CrestronTouchpanelPropertiesConfig Config;
        
        /// <summary>
        /// The parent driver for this
        /// </summary>
        EssentialsPanelMainInterfaceDriver Parent;

        public EssentialsEnvironmentDriver(EssentialsPanelMainInterfaceDriver parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            Config = config;
            Parent = parent;
        }

    }
}