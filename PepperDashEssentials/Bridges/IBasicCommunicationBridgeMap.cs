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
        public static void LinkToApi(this GenericComm comm, BasicTriList trilist, uint joinStart)
        {
            // this is a permanent event handler. This cannot be -= from event
            comm.CommPort.TextReceived += (s, a) => trilist.SetString(joinStart, a.Text);
            trilist.SetStringSigAction(joinStart, new Action<string>(s => comm.CommPort.SendText(s)));
            trilist.SetStringSigAction(joinStart + 1, new Action<string>(s => comm.SetPortConfig(s)));    
        

            var sComm = comm.CommPort as ISocketStatus;
            if (sComm != null)
            {
                sComm.ConnectionChange += (s, a) =>
                {
                    trilist.SetUshort(joinStart, (ushort)(a.Client.ClientStatus));
                    trilist.SetBool(joinStart, a.Client.ClientStatus ==
                        Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED);
                };

                trilist.SetBoolSigAction(joinStart, new Action<bool>(b =>
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