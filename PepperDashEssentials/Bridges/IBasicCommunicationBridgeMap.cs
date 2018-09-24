using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    public static class IBasicCommunicationExtensions
    {
        public static void LinkToApi(this GenericComm comm, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = GetJoinMapForDevice(joinMapKey);

            if (joinMap == null)
                joinMap = new IBasicCommunicationJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            if (comm.CommPort == null)
            {
                Debug.Console(1, comm, "Unable to link device '{0}'.  CommPort is null", comm.Key);
                return;
            }

            Debug.Console(1, comm, "Linking device '{0}' to Trilist '{1}'", comm.Key, trilist.ID);

            // this is a permanent event handler. This cannot be -= from event
            comm.CommPort.TextReceived += (s, a) => trilist.SetString(joinMap.TextReceived, a.Text);
            trilist.SetStringSigAction(joinMap.SendText, new Action<string>(s => comm.CommPort.SendText(s)));
            trilist.SetStringSigAction(joinMap.SetPortConfig + 1, new Action<string>(s => comm.SetPortConfig(s)));
        

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

        /// <summary>
        /// Attempts to get the join map from config
        /// </summary>
        /// <param name="joinMapKey"></param>
        /// <returns></returns>
        public static IBasicCommunicationJoinMap GetJoinMapForDevice(string joinMapKey)
        {
            if(!string.IsNullOrEmpty(joinMapKey))
                return null;

            // TODO: Get the join map from the ConfigReader.ConfigObject 

            return null;
        }

        public class IBasicCommunicationJoinMap
        {
            // Default joins
            public uint TextReceived { get; set; }
            public uint SendText { get; set; }
            public uint SetPortConfig { get; set; }
            public uint Connect { get; set; }
            public uint Connected { get; set; }
            public uint Status { get; set; }

            public IBasicCommunicationJoinMap()
            {
                TextReceived = 1;
                SendText = 1;
                SetPortConfig = 2;
                Connect = 1;
                Connected = 1;
                Status = 1;
            }

            /// <summary>
            /// Modifies all the join numbers by adding the offset.  This should never be called twice
            /// </summary>
            /// <param name="joinStart"></param>
            public void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                TextReceived = TextReceived + joinOffset;
                SendText = SendText + joinOffset;
                SetPortConfig = SetPortConfig + joinOffset;
                Connect = Connect + joinOffset;
                Connected = Connected + joinOffset;
                Status = Status + joinOffset;
            }
        }
    }
    ///// <summary>
    ///// 
    ///// </summary>
    //public static class DmChassisControllerApiExtensions
    //{
    //    public static void LinkToApi(this PepperDash.Essentials.DM.DmChassisController chassis, 
    //        BasicTriList trilist, Dictionary<string,uint> map, uint joinstart)
    //    {
    //        uint joinOffset = joinstart - 1;

    //        uint videoSelectOffset = 0 + joinOffset;
    //        uint audioSelectOffset = 40 + joinOffset;


    //        // loop chassis number of inupts
    //        for (uint i = 1; i <= chassis.Chassis.NumberOfOutputs; i++)
    //        {
    //            trilist.SetUShortSigAction(videoSelectOffset + i, new Action<ushort>(u => chassis.ExecuteSwitch(u, i, eRoutingSignalType.Video)));
    //            trilist.SetUShortSigAction(audioSelectOffset + i, new Action<ushort>(u => chassis.ExecuteSwitch(u, i, eRoutingSignalType.Audio)));
    //        }

    //        // wire up output change detection (try to add feedbacks or something to DMChassisController??

    //        // names?

    //        // HDCP?


    //    }
    //}




}