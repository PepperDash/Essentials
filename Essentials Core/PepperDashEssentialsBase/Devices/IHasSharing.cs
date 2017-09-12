using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Indicates that the device has the capability to share sources outside the local room
    /// </summary>
    public interface IHasSharing
    {
        void StartSharing();
        void StopSharing();

        StringFeedback SharingSourceFeedback { get; }
    }
}