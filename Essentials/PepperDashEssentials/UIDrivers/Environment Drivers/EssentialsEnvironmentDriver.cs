using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials
{
    public class EssentialsEnvironmentDriver : PanelDriverBase
    {
        /// <summary>
        /// Do I need this here?
        /// </summary>
        CrestronTouchpanelPropertiesConfig Config;

        /// <summary>
        /// The list of devices this driver is responsible for controlling
        /// </summary>
        public List<IKeyed> Devices { get; private set; }
        
        /// <summary>
        /// The parent driver for this
        /// </summary>
        EssentialsPanelMainInterfaceDriver Parent;

        /// <summary>
        /// The list of sub drivers for the devices
        /// </summary>
        public List<PanelDriverBase> DeviceSubDrivers { get; private set; }

        public uint BackgroundSubpageJoin { get; private set; }

        public EssentialsEnvironmentDriver(EssentialsPanelMainInterfaceDriver parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            Config = config;
            Parent = parent;

            Devices = new List<IKeyed>();
            DeviceSubDrivers = new List<PanelDriverBase>();

            // Calculate the join offests for each device page and assign join actions for each button
        }

        /// <summary>
        /// Shows this driver and all sub drivers
        /// </summary>
        public override void Show()
        {
            //TriList.SetBool(BackgroundSubpageJoin, true);

            foreach (var driver in DeviceSubDrivers)
            {
                driver.Show();
            }

            base.Show();
        }

        /// <summary>
        /// Hides this driver and all sub drivers
        /// </summary>
        public override void Hide()
        {
            //TriList.SetBool(BackgroundSubpageJoin, false);

            foreach (var driver in DeviceSubDrivers)
            {
                driver.Hide();
            }

            base.Hide();
        }

        public override void Toggle()
        {
            base.Toggle();
        }


        /// <summary>
        /// Reads the device keys from the config and gets the devices by key
        /// </summary>
        public void GetDevicesFromConfig(Room.Config.EssentialsEnvironmentPropertiesConfig EnvironmentPropertiesConfig)
        {
            if (EnvironmentPropertiesConfig != null)
            {
                Devices.Clear();
                DeviceSubDrivers.Clear();

                uint column = 4;

                foreach (var dKey in EnvironmentPropertiesConfig.DeviceKeys)
                {
                    var device = DeviceManager.GetDeviceForKey(dKey);

                    if (device != null)
                    {
                        Devices.Add(device);
                        
                        // Add new PanelDriverBase SubDriver 
                        DeviceSubDrivers.Add(GetPanelDriverForDevice(device, column));

                        Debug.Console(1, "Adding '{0}' to Environment Devices", device.Key);
                    }

                    column --;
                }

                SetupEnvironmentUiJoins();
            }
            else
            {
                Debug.Console(1, "Unable to get devices from config.  No EnvironmentPropertiesConfig object in room config");
            }
        }

        /// <summary>
        /// Returns the appropriate panel driver for the device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        PanelDriverBase GetPanelDriverForDevice(IKeyed device, uint column)
        {
            PanelDriverBase panelDriver = null;

            uint digitalJoinBase = 0;
            uint stringJoinBase = 0;
            uint shadeTypeVisibleBase = 0;
            uint lightingTypeVisibleBase = 0;

            switch (column)
            {
                case 4:
                    {
                        digitalJoinBase = UIBoolJoin.EnvironmentColumnFourButtonPressBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnFourLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnFourShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnFourLightingTypeVisibleBase;
                        break;
                    }
                case 3:
                    {
                        digitalJoinBase = UIBoolJoin.EnvironmentColumnThreeButtonPressBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnThreeLabelBase;
                         shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnThreeShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnThreeLightingTypeVisibleBase;
                       break;
                    }
                case 2:
                    {
                        digitalJoinBase = UIBoolJoin.EnvironmentColumnTwoButtonPressBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnTwoLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnTwoShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnTwoLightingTypeVisibleBase;
                       break;
                    }
                case 1:
                    {
                        digitalJoinBase = UIBoolJoin.EnvironmentColumnOneButtonPressBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnOneLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnOneShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnOneLightingTypeVisibleBase;
                        break;
                    }
                default:
                    {
                        Debug.Console(1, "Environment Driver: Invalid column number specified");
                        break;
                    }
            }

            // Determine if device is a shade or lighting type and construct the appropriate driver
            if (device is ShadeBase)
            {
                panelDriver = new EssentialsShadeDriver(this, device.Key, digitalJoinBase, stringJoinBase, shadeTypeVisibleBase);
            }
            else if (device is LightingBase)
            {
                //panelDriver = new EssentialsLightingDriver(this, device.Key, digitalJoinBase, stringJoinBase, lightingTypeVisibleBase);
            }

            // Return the driver

            return panelDriver;
        }

        /// <summary>
        /// Determines the join values for the generic environment subpages
        /// </summary>
        void SetupEnvironmentUiJoins()
        {
            // Calculate which background subpage join to use
            BackgroundSubpageJoin = UIBoolJoin.EnvironmentPopupSubpageVisibleBase + (uint)DeviceSubDrivers.Count;


        }

    }

    public interface IEnvironmentSubdriver
    {
        uint SubpageVisibleJoin { get; }
    }
    
}