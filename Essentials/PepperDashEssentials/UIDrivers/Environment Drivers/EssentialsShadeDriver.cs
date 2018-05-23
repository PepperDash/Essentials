using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Shades;

namespace PepperDash.Essentials
{
    public class EssentialsShadeDriver : PanelDriverBase, IEnvironmentSubdriver
    {
        EssentialsEnvironmentDriver Parent;

        public ShadeBase ShadeDevice { get; private set; }

        public uint SubpageVisibleJoin { get; private set; }

        uint DigitalJoinBase;

        uint StringJoinBase;

        eShadeDeviceType DeviceType;

        public EssentialsShadeDriver(EssentialsEnvironmentDriver parent, string deviceKey, uint digitalJoinBase, uint stringJoinBase, uint subpageVisibleBase)
            : base(parent.TriList)
        {
            Parent = parent;

            DigitalJoinBase = digitalJoinBase;
            StringJoinBase = stringJoinBase;

            ShadeDevice = DeviceManager.GetDeviceForKey(deviceKey) as ShadeBase;

            SetDeviceType();

            SetSubpageVisibleJoin(subpageVisibleBase);
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
            Parent.TriList.SetString(StringJoinBase + 50, ShadeDevice.Name);
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
    }

    enum eShadeDeviceType : uint
    {
        None = 0,
        OpenCloseStop = 1,
        OpenClose = 2,
        DiscreteLevel = 3
    }
}