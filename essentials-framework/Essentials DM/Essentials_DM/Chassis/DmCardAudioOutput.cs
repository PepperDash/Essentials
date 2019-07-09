using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
    public class DmCardAudioOutputController : IBasicVolumeWithFeedback
    {
        public Audio.Output Output { get; private set; }

        public IntFeedback VolumeLevelFeedback { get; private set; }

        public BoolFeedback MuteFeedback { get; private set; }

        ushort PreMuteVolumeLevel;
        bool IsMuted;

        public DmCardAudioOutputController(Audio.Output output)
        {
            Output = output;
            VolumeLevelFeedback = new IntFeedback(() => Output.VolumeFeedback.UShortValue);
            MuteFeedback = new BoolFeedback(() => IsMuted);
        }

        #region IBasicVolumeWithFeedback Members

        /// <summary>
        /// 
        /// </summary>
        public void MuteOff()
        {
            SetVolume(PreMuteVolumeLevel);
            IsMuted = false;
            MuteFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MuteOn()
        {
            PreMuteVolumeLevel = Output.VolumeFeedback.UShortValue;
            SetVolume(0);
            IsMuted = true;
            MuteFeedback.FireUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetVolume(ushort level)
        {
            Debug.Console(2, "Set volume out {0}", level);
            Output.Volume.UShortValue = level;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void VolumeEventFromChassis()
        {
            VolumeLevelFeedback.FireUpdate();
        }

        #endregion

        #region IBasicVolumeControls Members

        /// <summary>
        /// 
        /// </summary>
        public void MuteToggle()
        {
            if (IsMuted)
                MuteOff();
            else
                MuteOn();
        }

        /// <summary>
        /// 
        /// </summary>
        public void VolumeDown(bool pressRelease)
        {
            if (pressRelease)
            {
                var remainingRatio = Output.Volume.UShortValue / 65535;
                Output.Volume.CreateRamp(0, (uint)(400 * remainingRatio));
            }
            else
                Output.Volume.StopRamp();
        }

        /// <summary>
        /// 
        /// </summary>
        public void VolumeUp(bool pressRelease)
        {
            if (pressRelease)
            {
                var remainingRatio = (65535 - Output.Volume.UShortValue) / 65535;
                Output.Volume.CreateRamp(65535, 400);
            }
            else
                Output.Volume.StopRamp();
        }

        #endregion
    }
}