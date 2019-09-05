using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Bridges
{
    public static class AppleTvApiExtensions
    {
        public static void LinkToApi(this AppleTV appleTv, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as AppleTvJoinMap;

            if (joinMap == null)
            {
                joinMap = new AppleTvJoinMap();
            }

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", appleTv.GetType().Name.ToString());

            trilist.SetBoolSigAction(joinMap.UpArrow, (b) => appleTv.Up(b));
            trilist.SetBoolSigAction(joinMap.DnArrow, (b) => appleTv.Down(b));
            trilist.SetBoolSigAction(joinMap.LeftArrow, (b) => appleTv.Left(b));
            trilist.SetBoolSigAction(joinMap.RightArrow, (b) => appleTv.Right(b));
            trilist.SetBoolSigAction(joinMap.Select, (b) => appleTv.Select(b));
            trilist.SetBoolSigAction(joinMap.Menu, (b) => appleTv.Menu(b));
            trilist.SetBoolSigAction(joinMap.PlayPause, (b) => appleTv.Play(b));
        }
    }
}