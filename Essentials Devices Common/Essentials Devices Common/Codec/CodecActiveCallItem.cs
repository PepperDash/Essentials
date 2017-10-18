using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec

{
    public class CodecActiveCallItem
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public eCodecCallType Type { get; set; }

        public eCodecCallStatus Status { get; set; }

        public eCodecCallDirection Direction { get; set; }

        public string Id { get; set; }

        //public object CallMetaData { get; set; }

        /// <summary>
        /// Returns true when this call is any status other than 
        /// Unknown, Disconnected, Disconnecting
        /// </summary>
        public bool IsActiveCall
        {
            get
            {
                return !(Status == eCodecCallStatus.Disconnected
                    || Status == eCodecCallStatus.Disconnecting
					|| Status == eCodecCallStatus.Idle
                    || Status == eCodecCallStatus.Unknown);
            }
        }
    }
}