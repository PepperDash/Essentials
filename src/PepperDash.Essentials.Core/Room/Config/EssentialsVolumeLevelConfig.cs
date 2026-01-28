using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsRoomVolumesConfig
    /// </summary>
    public class EssentialsRoomVolumesConfig
    {
        /// <summary>
        /// Gets or sets the Master
        /// </summary>
        public EssentialsVolumeLevelConfig Master { get; set; }

        /// <summary>
        /// Gets or sets the Program
        /// </summary>
        public EssentialsVolumeLevelConfig Program { get; set; }

        /// <summary>
        /// Gets or sets the AudioCallRx
        /// </summary>
        public EssentialsVolumeLevelConfig AudioCallRx { get; set; }

        /// <summary>
        /// Gets or sets the AudioCallTx
        /// </summary>
        public EssentialsVolumeLevelConfig AudioCallTx { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsVolumeLevelConfig
    /// </summary>
    public class EssentialsVolumeLevelConfig
    {
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        public string DeviceKey { get; set; }
        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the Level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Helper to get the device associated with key - one timer.
        /// </summary>
        public IBasicVolumeWithFeedback GetDevice()
        {
            throw new NotImplementedException("This method references DM CHASSIS Directly");
            /*
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

            // DSP/DMPS format: deviceKey--levelName, biampTesira-1--master
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

                var dmps = DeviceManager.GetDeviceForKey(devKey) as DmpsAudioOutputController;
                if (dmps != null)
                {
                    var levelTag = match.Groups[2].Value;
                    switch (levelTag)
                    {
                        case "master":
                            return dmps.MasterVolumeLevel;
                        case "source":
                            return dmps.SourceVolumeLevel;
                        case "micsmaster":
                            return dmps.MicsMasterVolumeLevel;
                        case "codec1":
                            return dmps.Codec1VolumeLevel;
                        case "codec2":
                            return dmps.Codec2VolumeLevel;
                        default:
                            return dmps.MasterVolumeLevel;
                    }
                }
                // No volume for some reason. We have failed as developers
                return null;
            }

            return null;
        }
             * */
        }
    }
}