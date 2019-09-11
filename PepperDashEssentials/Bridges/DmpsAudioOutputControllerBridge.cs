using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class DmpsAudioOutputControllerApiExtensions
    {
        public static void LinkToApi(this DmpsAudioOutputController dmAudioOutputController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            DmpsAudioOutputControllerJoinMap joinMap = new DmpsAudioOutputControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmpsAudioOutputControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmAudioOutputController, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            if (dmAudioOutputController.MasterVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, dmAudioOutputController.MasterVolumeLevel, joinMap.MasterVolume);
            }

            if (dmAudioOutputController.SourceVolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, dmAudioOutputController.SourceVolumeLevel, joinMap.SourceVolume);
            }

            if (dmAudioOutputController.Codec1VolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, dmAudioOutputController.Codec1VolumeLevel, joinMap.Codec1Volume);
            }

            if (dmAudioOutputController.Codec2VolumeLevel != null)
            {
                SetUpDmpsAudioOutputJoins(trilist, dmAudioOutputController.Codec2VolumeLevel, joinMap.Codec2Volume);
            }

        }

        static void SetUpDmpsAudioOutputJoins(BasicTriList trilist, DmpsAudioOutput output, uint joinStart)
        {
            var volumeLevelJoin = joinStart;
            var muteOnJoin = joinStart;
            var muteOffJoin = joinStart + 1;
            var volumeUpJoin = joinStart + 2;
            var volumeDownJoin = joinStart + 3;


            trilist.SetUShortSigAction(volumeLevelJoin, new Action<ushort>(o => output.SetVolume(o)));
            output.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[volumeLevelJoin]);

            trilist.SetSigTrueAction(muteOnJoin, new Action(output.MuteOn));
            output.MuteFeedback.LinkInputSig(trilist.BooleanInput[muteOnJoin]);
            trilist.SetSigTrueAction(muteOffJoin, new Action(output.MuteOff));
            output.MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[muteOffJoin]);

            trilist.SetBoolSigAction(volumeUpJoin, new Action<bool>(b => output.VolumeUp(b)));
            trilist.SetBoolSigAction(volumeDownJoin, new Action<bool>(b => output.VolumeDown(b)));
        }
    }
}