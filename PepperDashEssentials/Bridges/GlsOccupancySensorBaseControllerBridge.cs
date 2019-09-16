using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Devices.Common.Occupancy;

using PepperDash.Essentials.Core;
using PepperDash.Core;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class GlsOccupancySensorBaseControllerApiExtensions
    {
        public static void LinkToApi(this GlsOccupancySensorBaseController occController, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            GlsOccupancySensorBaseJoinMap joinMap = new GlsOccupancySensorBaseJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GlsOccupancySensorBaseJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, occController, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            occController.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);

        }
    }
}