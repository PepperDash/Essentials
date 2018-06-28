using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Devices.Common.Environment.Somfy;

namespace PepperDash.Essentials
{
    public class EssentialsShadeDriver : PanelDriverBase, IEnvironmentSubdriver
    {
        EssentialsEnvironmentDriver Parent;

        public ShadeBase ShadeDevice { get; private set; }

        public uint SubpageVisibleJoin { get; private set; }

        /// <summary>
        /// The base join number that all button presses are offset from
        /// </summary>
        uint ButtonPressJoinBase;

        /// <summary>
        /// The base join number that all string lables are offset from
        /// </summary>
        uint StringJoinBase;

        eShadeDeviceType DeviceType;

        const uint DeviceNameJoinOffset = 50;

        public EssentialsShadeDriver(EssentialsEnvironmentDriver parent, string deviceKey, uint buttonPressJoinBase, uint stringJoinBase, uint subpageVisibleBase)
            : base(parent.TriList)
        {
            Parent = parent;

            ButtonPressJoinBase = buttonPressJoinBase;
            StringJoinBase = stringJoinBase;

            ShadeDevice = DeviceManager.GetDeviceForKey(deviceKey) as ShadeBase;

            SetDeviceType();

            SetSubpageVisibleJoin(subpageVisibleBase);

            SetUpDeviceName();

            SetUpButtonActions();
        }

        public override void Show()
        {
            TriList.SetBool(SubpageVisibleJoin, true);

            base.Show();
        }

        public override void Hide()
        {
            TriList.SetBool(SubpageVisibleJoin, false);

            base.Hide();
        }

        void SetUpDeviceName()
        {
            Parent.TriList.SetString(StringJoinBase + DeviceNameJoinOffset, ShadeDevice.Name);
        }

        void SetDeviceType()
        {
            if (ShadeDevice is IShadesOpenCloseStop)
                DeviceType = eShadeDeviceType.OpenCloseStop;
            else if (ShadeDevice is IShadesOpenClose)
                DeviceType = eShadeDeviceType.OpenClose;
        }

        void SetSubpageVisibleJoin(uint subpageVisibleBase)
        {
            SubpageVisibleJoin = subpageVisibleBase + (uint)DeviceType;
        }

        void SetUpButtonActions()
        {
            if(DeviceType == eShadeDeviceType.OpenClose)
            {
                TriList.SetSigTrueAction(ButtonPressJoinBase + 1, ShadeDevice.Open);

                TriList.SetSigFalseAction(ButtonPressJoinBase + 2, ShadeDevice.Close);
            }
            else if(DeviceType == eShadeDeviceType.OpenCloseStop)
            {
                TriList.SetSigFalseAction(ButtonPressJoinBase + 1, ShadeDevice.Open);

                TriList.SetSigFalseAction(ButtonPressJoinBase + 2, (ShadeDevice as IShadesOpenCloseStop).StopOrPreset);

                if(ShadeDevice is RelayControlledShade)
                    TriList.SetString(StringJoinBase + 2, (ShadeDevice as RelayControlledShade).StopOrPresetButtonLabel);
                
                TriList.SetSigFalseAction(ButtonPressJoinBase + 3, ShadeDevice.Close);
            }
        }
    }

    enum eShadeDeviceType : uint
    {
        None = 0,
        OpenCloseStop = 1,
        OpenClose = 2,
        DiscreteLevel = 3
    }
}