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

            Parent.AvDriver.PopupInterlock.IsShownFeedback.OutputChange += IsShownFeedback_OutputChange;

            // Calculate the join offests for each device page and assign join actions for each button
        }

        void IsShownFeedback_OutputChange(object sender, EventArgs e)
        {
            // Hide this driver and all sub drivers if popup interlock is not shown
            if (Parent.AvDriver.PopupInterlock.IsShownFeedback.BoolValue == false)
            {
                foreach (var driver in DeviceSubDrivers)
                {
                    driver.Hide();
                }

                base.Hide();
            }
        }

        /// <summary>
        /// Shows this driver and all sub drivers
        /// </summary>
        public override void Show()
        {
            Parent.AvDriver.PopupInterlock.ShowInterlockedWithToggle(BackgroundSubpageJoin);

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
            Parent.AvDriver.PopupInterlock.HideAndClear();

            foreach (var driver in DeviceSubDrivers)
            {
                driver.Hide();
            }

            base.Hide();
        }

        public override void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
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

                uint column = 1;

                foreach (var dKey in EnvironmentPropertiesConfig.DeviceKeys)
                {
                    var device = DeviceManager.GetDeviceForKey(dKey);

                    if (device != null)
                    {
                        Devices.Add(device);

                        // Build the driver
                        var devicePanelDriver = GetPanelDriverForDevice(device, column);

                        // Add new PanelDriverBase SubDriver 
                        if (devicePanelDriver != null)
                            DeviceSubDrivers.Add(devicePanelDriver);

                        Debug.Console(1, "Adding '{0}' to Environment Devices", device.Key);

                        column++;

                        // Quit if device count is exceeded
                        if (column > 4)
                            break;
                    }

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

            uint buttonPressJoinBase = 0;
            uint buttonVisibleJoinBase = 0;
            uint stringJoinBase = 0;
            uint shadeTypeVisibleBase = 0;
            uint lightingTypeVisibleBase = 0;

            switch (column)
            {
                case 1:
                    {
                        buttonPressJoinBase = UIBoolJoin.EnvironmentColumnOneButtonPressBase;
                        buttonVisibleJoinBase = UIBoolJoin.EnvironmentColumnOneButtonVisibleBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnOneLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnOneShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnOneLightingTypeVisibleBase;
                        break;
                    }
                case 2:
                    {
                        buttonPressJoinBase = UIBoolJoin.EnvironmentColumnTwoButtonPressBase;
                        buttonVisibleJoinBase = UIBoolJoin.EnvironmentColumnTwoButtonVisibleBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnTwoLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnTwoShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnTwoLightingTypeVisibleBase;
                        break;
                    }
                case 3:
                    {
                        buttonPressJoinBase = UIBoolJoin.EnvironmentColumnThreeButtonPressBase;
                        buttonVisibleJoinBase = UIBoolJoin.EnvironmentColumnThreeButtonVisibleBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnThreeLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnThreeShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnThreeLightingTypeVisibleBase;
                        break;
                    }
                case 4:
                    {
                        buttonPressJoinBase = UIBoolJoin.EnvironmentColumnFourButtonPressBase;
                        buttonVisibleJoinBase = UIBoolJoin.EnvironmentColumnFourButtonVisibleBase;
                        stringJoinBase = UIStringJoin.EnvironmentColumnFourLabelBase;
                        shadeTypeVisibleBase = UIBoolJoin.EnvironmentColumnFourShadingTypeVisibleBase;
                        lightingTypeVisibleBase = UIBoolJoin.EnvironmentColumnFourLightingTypeVisibleBase;
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
                panelDriver = new EssentialsShadeDriver(this, device.Key, buttonPressJoinBase, stringJoinBase, shadeTypeVisibleBase);
            }
            else if (device is LightingBase)
            {
                panelDriver = new EssentialsLightingDriver(this, device.Key, buttonPressJoinBase, buttonVisibleJoinBase, stringJoinBase, lightingTypeVisibleBase);
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
            BackgroundSubpageJoin = UIBoolJoin.EnvironmentBackgroundSubpageVisibleBase + (uint)DeviceSubDrivers.Count;


        }

    }

    public interface IEnvironmentSubdriver
    {
        uint SubpageVisibleJoin { get; }
    }
    
}