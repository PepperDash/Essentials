namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Base class for bridge API variants
    /// </summary>
    public abstract class BridgeApi : EssentialsDevice
    {
        protected BridgeApi(string key) :
            base(key)
        {

        }
    }
}