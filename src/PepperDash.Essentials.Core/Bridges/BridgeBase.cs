extern alias Full;
using Crestron.SimplSharp;

//using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Base class for all bridge class variants
    /// </summary>
    public class BridgeBase : EssentialsDevice
    {
        public BridgeApi Api { get; protected set; }

        public BridgeBase(string key) :
            base(key)
        {

        }
    }
}