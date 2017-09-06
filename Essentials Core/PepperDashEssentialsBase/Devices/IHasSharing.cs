using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    public interface IHasSharing
    {

        BoolFeedback SharingSourceFeedback { get; }
    }
}