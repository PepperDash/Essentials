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
        public static void LinkToApi(this DmpsAudioOutputController dmAudioOutputController, BasicTriList trilis, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmpsAudioOutputControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmpsAudioOutputControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, dmAudioOutputController, "Linking to Trilist '{0}'", trilis.ID.ToString("X"));


        }

        public class DmpsAudioOutputControllerJoinMap: JoinMapBase
        {
            // Digital
            public uint MasterVolumeMute { get; set; }
            public uint SourceVolumeMute { get; set; }
            public uint Codec1VolumeMute { get; set; }
            public uint Codec2VolumeMute { get; set; }


            public DmpsAudioOutputControllerJoinMap()
            {
                MasterVolumeMute = 1; // 1-10
                SourceVolumeMute = 11; // 11-20
                Codec1VolumeMute = 21; // 21-30
                Codec2VolumeMute = 31; // 31-40
            }

            public override void  OffsetJoinNumbers(uint joinStart)
            {
 	            var joinOffset = joinStart -1;

                MasterVolumeMute = MasterVolumeMute + joinOffset;
                SourceVolumeMute = SourceVolumeMute + joinOffset;
                Codec1VolumeMute = Codec1VolumeMute + joinOffset;
                Codec2VolumeMute = Codec2VolumeMute + joinOffset;
            }
        }
    }
}