using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Supports a lighting device with up to 6 scenes
    /// </summary>
    public class EssentialsLightingDriver : PanelDriverBase, IEnvironmentSubdriver
    {
        EssentialsEnvironmentDriver Parent;

        public LightingBase LightingDevice { get; private set; }

        public uint SubpageVisibleJoin { get; private set; }

        /// <summary>
        /// The base join number that all button visibilty joins are offset from
        /// </summary>
        uint ButtonVisibleJoinBase;
 
        /// <summary>
        /// The base join number that all button presses are offset from
        /// </summary>
        uint ButtonPressJoinBase;

        /// <summary>
        /// The base join number that all string lables are offset from
        /// </summary>
        uint StringJoinBase;

        eLightsDeviceType DeviceType;

        const uint DeviceNameJoinOffset = 50;

        public EssentialsLightingDriver(EssentialsEnvironmentDriver parent, string deviceKey, uint buttonPressJoinBase, uint buttonVisibleJoinBase, uint stringJoinBase, uint subpageVisibleBase)
            : base(parent.TriList)
        {
            Parent = parent;

            ButtonPressJoinBase = buttonPressJoinBase;
            ButtonVisibleJoinBase = buttonVisibleJoinBase;
            StringJoinBase = stringJoinBase;

            LightingDevice = DeviceManager.GetDeviceForKey(deviceKey) as LightingBase;

            //LightingDevice.LightingSceneChange += new EventHandler<LightingSceneChangeEventArgs>(LightingDevice_LightingSceneChange);

            SetDeviceType();

            SetSubpageVisibleJoin(subpageVisibleBase);

            SetUpDeviceName();

            SetUpButtonActions();
        }

        /// <summary>
        /// Handles setting feedback for the currently selected scene button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LightingDevice_LightingSceneChange(object sender, LightingSceneChangeEventArgs e)
        {
            uint joinOffset = 1;

            foreach (var scene in LightingDevice.LightingScenes)
            {
                if (scene == e.CurrentLightingScene)
                    TriList.SetBool(ButtonPressJoinBase + joinOffset, true);
                else
                    TriList.SetBool(ButtonPressJoinBase + joinOffset, false);
            }
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
            Parent.TriList.SetString(StringJoinBase + DeviceNameJoinOffset, LightingDevice.Name);
        }

        void SetDeviceType()
        {
            if (LightingDevice is ILightingScenes)
                DeviceType = eLightsDeviceType.Scenes;
        }

        void SetSubpageVisibleJoin(uint subpageVisibleBase)
        {
            SubpageVisibleJoin = subpageVisibleBase + (uint)DeviceType;
        }

        /// <summary>
        /// Drase
        /// </summary>
        void SetUpButtonActions()
        {
            if (DeviceType == eLightsDeviceType.Scenes)
            {
                uint joinOffset = ComputeJoinOffset();

                // Clear preceding buttons
                for (uint i = 1; i < joinOffset; i++)
                {
                    TriList.SetString(StringJoinBase + i, "");
                    TriList.SetSigFalseAction(ButtonPressJoinBase + i, () => { });
                    TriList.SetBool(ButtonVisibleJoinBase + i, false);
                }

                foreach (var scene in LightingDevice.LightingScenes)
                {
                    TriList.SetString(StringJoinBase + joinOffset, scene.Name);
                    var tempScene = scene;
                    TriList.SetSigFalseAction(ButtonPressJoinBase + joinOffset, () => LightingDevice.SelectScene(tempScene));
                    scene.IsActiveFeedback.LinkInputSig(TriList.BooleanInput[ButtonPressJoinBase + joinOffset]);
                    TriList.SetBool(ButtonVisibleJoinBase + joinOffset, true);

                    joinOffset++;
                }

                // Clear following buttons
                for (uint i = joinOffset; i <= 6; i++)
                {
                    TriList.SetString(StringJoinBase + i, "");
                    TriList.SetSigFalseAction(ButtonPressJoinBase + i, () => { });
                    TriList.SetBool(ButtonVisibleJoinBase + i, false);
                }
            }

        }



        /// <summary>
        /// Computes the desired join offset to try to achieve the most centered appearance when using a subpage with 6 scene buttons
        /// </summary>
        /// <returns></returns>
        uint ComputeJoinOffset()
        {
            uint joinOffset = 0;

            switch (LightingDevice.LightingScenes.Count)
            {
                case 1:
                    {
                        joinOffset = 2;
                        break;
                    }
                case 2:
                    {
                        joinOffset = 3;
                        break;
                    }
                case 3:
                    {
                        joinOffset = 2;
                        break;
                    }
                case 4:
                    {
                        joinOffset = 2;
                        break;
                    }
                case 5:
                    {
                        joinOffset = 2;
                        break;
                    }
                case 6:
                    {
                        joinOffset = 1;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return joinOffset;
        }
    }

    enum eLightsDeviceType : uint
    {
        None = 0,
        Scenes = 1,
    }
}