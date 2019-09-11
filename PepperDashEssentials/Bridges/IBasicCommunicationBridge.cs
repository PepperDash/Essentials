using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class IBasicCommunicationApiExtensions
    {
        public static void LinkToApi(this GenericComm comm, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            IBasicCommunicationJoinMap joinMap = new IBasicCommunicationJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IBasicCommunicationJoinMap>(joinMapSerialized);
            joinMap.OffsetJoinNumbers(joinStart);

            if (comm.CommPort == null)
            {
                Debug.Console(1, comm, "Unable to link device '{0}'.  CommPort is null", comm.Key);
                return;
            }

            Debug.Console(1, comm, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            // this is a permanent event handler. This cannot be -= from event
            comm.CommPort.TextReceived += (s, a) =>
            {
                Debug.Console(2, comm, "RX: {0}", a.Text);
                trilist.SetString(joinMap.TextReceived, a.Text);
            };
            trilist.SetStringSigAction(joinMap.SendText, new Action<string>(s => comm.CommPort.SendText(s)));
            trilist.SetStringSigAction(joinMap.SetPortConfig, new Action<string>(s => comm.SetPortConfig(s)));


            var sComm = comm.CommPort as ISocketStatus;
            if (sComm != null)
            {
                sComm.ConnectionChange += (s, a) =>
                {
                    trilist.SetUshort(joinMap.Status, (ushort)(a.Client.ClientStatus));
                    trilist.SetBool(joinMap.Connected, a.Client.ClientStatus ==
                        Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED);
                };

                trilist.SetBoolSigAction(joinMap.Connect, new Action<bool>(b =>
                {
                    if (b)
                    {
                        sComm.Connect();
                    }
                    else
                    {
                        sComm.Disconnect();
                    }
                }));
            }
        }
    }
}