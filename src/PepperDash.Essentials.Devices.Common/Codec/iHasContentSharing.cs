using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasContentSharing
    {
        BoolFeedback SharingContentIsOnFeedback { get; }
        StringFeedback SharingSourceFeedback { get; }

        bool AutoShareContentWhileInCall { get; }

        void StartSharing();
        void StopSharing();
    }

}