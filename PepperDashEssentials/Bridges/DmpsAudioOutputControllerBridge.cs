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

namespace PepperDash.Essentials.Bridges
{
    public static class DmpsAudioOutputControllerApiExtensions
    {
        public static void LinkToApi(this DmpsAudioOutputController dmAudioOutputController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmpsAudioOutputControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmpsAudioOutputControllerJoinMap();

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
            var muteOnJoin = joinStart + 1;
            var muteOffJoin = joinStart + 2;
            var volumeUpJoin = joinStart + 3;
            var volumeDownJoin = joinStart + 4;


            trilist.SetUShortSigAction(volumeLevelJoin, new Action<ushort>(o => output.SetVolume(o)));
            output.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[volumeLevelJoin]);

            trilist.SetSigTrueAction(muteOnJoin, new Action(output.MuteOn));
            output.MuteFeedback.LinkInputSig(trilist.BooleanInput[muteOnJoin]);
            trilist.SetSigTrueAction(muteOffJoin, new Action(output.MuteOff));
            output.MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[muteOffJoin]);

            trilist.SetBoolSigAction(volumeUpJoin, new Action<bool>(b => output.VolumeUp(b)));
            trilist.SetBoolSigAction(volumeDownJoin, new Action<bool>(b => output.VolumeDown(b)));
        }

        public class DmpsAudioOutputControllerJoinMap: JoinMapBase
        {
            // Digital
            public uint MasterVolume { get; set; }
            public uint SourceVolume { get; set; }
            public uint Codec1Volume { get; set; }
            public uint Codec2Volume { get; set; }


            public DmpsAudioOutputControllerJoinMap()
            {
                MasterVolume = 1; // 1-10
                SourceVolume = 11; // 11-20
                Codec1Volume = 21; // 21-30
                Codec2Volume = 31; // 31-40
            }

            public override void  OffsetJoinNumbers(uint joinStart)
            {
 	            var joinOffset = joinStart -1;

                MasterVolume = MasterVolume + joinOffset;
                SourceVolume = SourceVolume + joinOffset;
                Codec1Volume = Codec1Volume + joinOffset;
                Codec2Volume = Codec2Volume + joinOffset;
            }
        }
    }
}