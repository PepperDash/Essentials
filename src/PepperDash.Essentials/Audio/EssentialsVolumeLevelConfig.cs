extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Full.Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Devices.Common.DSP;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsRoomVolumesConfig
    {
        public EssentialsVolumeLevelConfig Master { get; set; }
        public EssentialsVolumeLevelConfig Program { get; set; }
        public EssentialsVolumeLevelConfig AudioCallRx { get; set; }
        public EssentialsVolumeLevelConfig AudioCallTx { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsVolumeLevelConfig
    {
        public string DeviceKey { get; set; }
        public string Label { get; set; }
        public int Level { get; set; }

        /// <summary>
        /// Helper to get the device associated with key - one timer.
        /// </summary>
        public IBasicVolumeWithFeedback GetDevice()
        {
            // DM output card format: deviceKey--output~number, dm8x8-1--output~4
            var match = Regex.Match(DeviceKey, @"([-_\w]+)--(\w+)~(\d+)");
            if (match.Success)
            {
                var devKey = match.Groups[1].Value;
                var chassis = DeviceManager.GetDeviceForKey(devKey) as DmChassisController;
                if (chassis != null)
                {
                    var outputNum = Convert.ToUInt32(match.Groups[3].Value);
                    if (chassis.VolumeControls.ContainsKey(outputNum)) // should always...
                        return chassis.VolumeControls[outputNum];
                }
                // No volume for some reason. We have failed as developers
                return null;
            }

            // DSP format: deviceKey--levelName, biampTesira-1--master
            match = Regex.Match(DeviceKey, @"([-_\w]+)--(.+)");
            if (match.Success)
            {
                var devKey = match.Groups[1].Value;
                var dsp = DeviceManager.GetDeviceForKey(devKey) as BiampTesiraForteDsp;
                if (dsp != null)
                {
                    var levelTag = match.Groups[2].Value;
                    if (dsp.LevelControlPoints.ContainsKey(levelTag)) // should always...
                        return dsp.LevelControlPoints[levelTag];
                }
                // No volume for some reason. We have failed as developers
                return null;
            }

            return null;
        }
    }
}